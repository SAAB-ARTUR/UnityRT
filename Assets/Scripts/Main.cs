using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class Main : MonoBehaviour
{    
    public ComputeShader computeShaderTest = null;

    [SerializeField] GameObject srcSphere = null;
    [SerializeField] GameObject targetSphere = null;
    [SerializeField] GameObject surface = null;
    [SerializeField] GameObject seafloor = null;
    [SerializeField] GameObject waterplane = null;
    [SerializeField] Camera secondCamera = null;
    [SerializeField] bool sendRaysContinuosly = false;
    [SerializeField] bool visualizeRays = false;
    [SerializeField] float lineLength = 1;
    [SerializeField] Material lineMaterial = null;
    [SerializeField] GameObject world_manager = null;

    private SourceParams oldSourceParams = null;

    uint cameraWidth = 0;
    uint cameraHeight = 0;

    private List<GameObject> waterplanes = new List<GameObject>();    

    private ComputeBuffer _rayPointsBuffer;
    
    RayTracingVisualization secondCameraScript = null;
    private RenderTexture _target;

    private bool doRayTracing = false;
    private bool lockRayTracing = false;

    RayData[] rds = null;

    LineRenderer line = null;
    private List<LineRenderer> lines = new List<LineRenderer>();

    LineRenderer srcDirectionLine = null;
    LineRenderer srcViewLine1 = null;
    LineRenderer srcViewLine2 = null;
    LineRenderer srcViewLine3 = null;
    LineRenderer srcViewLine4 = null;

    RayTracingAccelerationStructure rtas = null;
    private bool rebuildRTAS = false;

    SurfaceAndSeafloorInstanceData surfaceInstanceData = null;
    SurfaceAndSeafloorInstanceData seafloorInstanceData = null;
    WaterplaneInstanceData waterplaneInstanceData = null;

    const float PI = 3.14159265f;    

    struct RayData
    {
        public Vector3 origin;
        public int set;
    };
    private int raydatabytesize = 16;

    private void ReleaseResources()
    {
        if (rtas != null)
        {
            rtas.Release();
            rtas = null;
        }

        if (_target)
        {
            _target.Release();
            _target = null;
        }

        cameraHeight = 0;
        cameraWidth = 0;

        if (surfaceInstanceData != null)
        {
            surfaceInstanceData.Dispose();
            surfaceInstanceData = null;
        }
        if (waterplaneInstanceData != null)
        {
            waterplaneInstanceData.Dispose();
            waterplaneInstanceData = null;
        }
        if (seafloorInstanceData != null)
        {
            seafloorInstanceData.Dispose();
            seafloorInstanceData = null;
        }

        _rayPointsBuffer?.Release();        
    }

    void OnDestroy()
    {
        ReleaseResources();        
    }

    void OnDisable()
    {
        ReleaseResources();
    }

    private void CreateResources()
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();        

        if (_rayPointsBuffer == null)
        {
            _rayPointsBuffer = new ComputeBuffer(sourceParams.ntheta*sourceParams.nphi*sourceParams.MAXINTERACTIONS, raydatabytesize);
        }

        if (rds == null)
        {
            rds = new RayData[sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS];
        }

        if (surfaceInstanceData == null)
        {
            if (surfaceInstanceData != null)
            {
                surfaceInstanceData.Dispose();
            }
            surfaceInstanceData = new SurfaceAndSeafloorInstanceData();
        }        
        if (seafloorInstanceData == null)
        {
            if (seafloorInstanceData != null)
            {
                seafloorInstanceData.Dispose();
            }
            seafloorInstanceData = new SurfaceAndSeafloorInstanceData();
        }

        World world = world_manager.GetComponent<World>();
        int nrOfWaterplanes = world.GetNrOfWaterplanes();
        float depth = world.GetWaterDepth();
        if (nrOfWaterplanes > 0 && (waterplaneInstanceData == null || waterplaneInstanceData.layers != nrOfWaterplanes || waterplaneInstanceData.depth != depth))
        {
            if (waterplaneInstanceData != null)
            {
                waterplaneInstanceData.Dispose();
            }
            waterplaneInstanceData = new WaterplaneInstanceData(nrOfWaterplanes, depth);
        }
        else if (nrOfWaterplanes <= 0)
        {
            if (waterplaneInstanceData != null)
            {
                waterplaneInstanceData.Dispose();
            }
        }
    }

    private void SetComputeBuffer(string name, ComputeBuffer buffer)
    {
        if (buffer != null)
        {        
            computeShaderTest.SetBuffer(0, name, buffer);
        }
    }

    private void SetShaderParameters()
    {
        computeShaderTest.SetMatrix("_CameraToWorld", Camera.main.cameraToWorldMatrix);
        computeShaderTest.SetMatrix("_CameraInverseProjection", Camera.main.projectionMatrix.inverse);
        computeShaderTest.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
        computeShaderTest.SetFloat("_Seed", UnityEngine.Random.value);
        
        SetComputeBuffer("_RayPoints", _rayPointsBuffer);

        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();

        computeShaderTest.SetInt("theta", sourceParams.theta);
        computeShaderTest.SetInt("ntheta", sourceParams.ntheta);
        computeShaderTest.SetInt("phi", sourceParams.phi);
        computeShaderTest.SetInt("nphi", sourceParams.nphi);
        computeShaderTest.SetVector("srcDirection", srcSphere.transform.forward);

        computeShaderTest.SetInt("_MAXINTERACTIONS", sourceParams.MAXINTERACTIONS);

        computeShaderTest.SetRayTracingAccelerationStructure(0, "g_AccelStruct", rtas);
    }

    private void InitRenderTexture(SourceParams sourceParams)
    {
        if (_target == null || _target.width != sourceParams.nphi || _target.height != sourceParams.ntheta)
        {
            // Release render texture if we already have one
            if (_target != null)
            {
                _target.Release();                
            }

             // Get a render target for Ray Tracing
            _target = new RenderTexture(sourceParams.nphi, sourceParams.ntheta, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }

    private void BuildWorld() {
        World world = world_manager.GetComponent<World>();
        world.AddSource(srcSphere);
        world.AddSurface(surface);
        world.AddBottom(seafloor);
        if (world.GetNrOfWaterplanes() > 0)
        {
            world.AddWaterplane(waterplane);
        }        
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        if (secondCamera != null)
        {
            secondCameraScript = secondCamera.GetComponent<RayTracingVisualization>();
        }

        rtas = new RayTracingAccelerationStructure();
    }

    #region SourceViewLines
    private LineRenderer CreateSrcViewLine(string name)
    {
        LineRenderer viewLine = new GameObject(name).AddComponent<LineRenderer>();
        viewLine.startWidth = 0.01f;
        viewLine.endWidth = 0.01f;
        viewLine.positionCount = 2;
        viewLine.useWorldSpace = true;        

        viewLine.material = lineMaterial;
        viewLine.material.color = Color.black;

        return viewLine;
    }

    private void UpdateSourceViewLines()
    {
        Vector3[] viewLines = ViewLines();

        srcViewLine1.SetPosition(0, srcSphere.transform.position);
        srcViewLine1.SetPosition(1, srcSphere.transform.position + viewLines[0] * lineLength);

        srcViewLine2.SetPosition(0, srcSphere.transform.position);
        srcViewLine2.SetPosition(1, srcSphere.transform.position + viewLines[1] * lineLength);

        srcViewLine3.SetPosition(0, srcSphere.transform.position);
        srcViewLine3.SetPosition(1, srcSphere.transform.position + viewLines[2] * lineLength);

        srcViewLine4.SetPosition(0, srcSphere.transform.position);
        srcViewLine4.SetPosition(1, srcSphere.transform.position + viewLines[3] * lineLength);
    }

    Vector3[] ViewLines()
    {
        // angles for srcSphere's forward vector (which is of length 1 meaning that r can be removed from all equations below)

        SourceParams srcParams = srcSphere.GetComponent<SourceParams>();

        float origin_theta = (float)Math.Acos(srcSphere.transform.forward.y);
        float origin_phi = (float)Math.Atan2(srcSphere.transform.forward.z, srcSphere.transform.forward.x);

        float theta_rad = srcParams.theta * PI / 180; //convert to radians
        float phi_rad = srcParams.phi * PI / 180;

        float s0 = (float)Math.Sin(origin_phi);
        float c0 = (float)Math.Cos(origin_phi);

        // create angular spans in both dimensions
        float[] theta_offsets = new float[2] { origin_theta - theta_rad / 2, origin_theta + theta_rad / 2 };
        float[] phi_offsets = new float[2] { origin_phi - phi_rad / 2, origin_phi + phi_rad / 2 };

        Vector3[] viewLines = new Vector3[4];

        int k = 0;
        for (int i = 0; i < 2; i++) // loop over phi
        {
            float s1 = (float)Math.Sin(phi_offsets[i] - origin_phi);
            float c1 = (float)Math.Cos(phi_offsets[i] - origin_phi);

            for (int j = 0; j < 2; j++) // loop over theta
            {
                float x = c0 * c1 * (float)Math.Sin(theta_offsets[j]) - s0 * s1;
                float z = s0 * c1 * (float)Math.Sin(theta_offsets[j]) + c0 * s1;
                float y = c1 * (float)Math.Cos(theta_offsets[j]);
                viewLines[k] = new Vector3(x, y, z);
                k++;
            }
        }

        return viewLines;
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {        
        Renderer srcRenderer = srcSphere.GetComponent<Renderer>();
        srcRenderer.material.SetColor("_Color", Color.green);
        Renderer targetRenderer = targetSphere.GetComponent<Renderer>();
        targetRenderer.material.SetColor("_Color", Color.red);
        Debug.Log("Start");

        srcDirectionLine = CreateSrcViewLine("SourceDirectionLine");

        srcDirectionLine.SetPosition(0, srcSphere.transform.position);
        srcDirectionLine.SetPosition(1, srcSphere.transform.position + srcSphere.transform.forward * lineLength);

        srcDirectionLine.material = lineMaterial;
        srcDirectionLine.material.color = Color.black;        
        
        Vector3[] viewLines = ViewLines();

        // line1
        srcViewLine1 = CreateSrcViewLine("View line1");        

        srcViewLine1.SetPosition(0, srcSphere.transform.position);
        srcViewLine1.SetPosition(1, srcSphere.transform.position + viewLines[0] * lineLength);

        // line2
        srcViewLine2 = CreateSrcViewLine("View line2");            

        srcViewLine2.SetPosition(0, srcSphere.transform.position);
        srcViewLine2.SetPosition(1, srcSphere.transform.position + viewLines[1] * lineLength);

        // line3
        srcViewLine3 = CreateSrcViewLine("View line3");

        srcViewLine3.SetPosition(0, srcSphere.transform.position);
        srcViewLine3.SetPosition(1, srcSphere.transform.position + viewLines[2] * lineLength);

        // line4
        srcViewLine4 = CreateSrcViewLine("View line4");

        srcViewLine4.SetPosition(0, srcSphere.transform.position);
        srcViewLine4.SetPosition(1, srcSphere.transform.position + viewLines[3] * lineLength);

        BuildWorld();        
        rebuildRTAS = true;
    }

    // Update is called once per frame
    void Update()
    {   
        if (srcDirectionLine != null)
        {
            srcDirectionLine.SetPosition(0, srcSphere.transform.position);
            srcDirectionLine.SetPosition(1, srcSphere.transform.position + srcSphere.transform.forward * lineLength);

            UpdateSourceViewLines();
        }

        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();

        if (sourceParams != oldSourceParams)
            {            
            // reinit rds arrau
            rds = new RayData[sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS];
            // reinit raydatabuffer
            if (_rayPointsBuffer != null)
            {
                _rayPointsBuffer.Release();
            }
            _rayPointsBuffer = new ComputeBuffer(sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS, raydatabytesize);            
            
            oldSourceParams = (SourceParams)sourceParams.Clone();
        }

        World world = world_manager.GetComponent<World>();
        if (world.StateChanged())
        {
            BuildWorld();
        }

        if (Input.GetKey(KeyCode.C)){
            doRayTracing = true;            
        }
        else
        {
            doRayTracing = false;
            lockRayTracing = false;
        }        

        if ((!lockRayTracing && doRayTracing) || sendRaysContinuosly) // do raytracing if the user has pressed key C. only do it once though. or do it continously
        {
            foreach (LineRenderer line in lines)
            {
                Destroy(line.gameObject);
            }
            lines.Clear();

            lockRayTracing = true; // disable raytracing being done several times during one keypress
            // do raytracing
            Debug.Log("RayTrace");

            CreateResources();

            if (rebuildRTAS)
            {
                BuildRTAS();
                rebuildRTAS = false;
            }            

            SetShaderParameters();

            InitRenderTexture(sourceParams);

            if(_target == null)
            {
                Debug.Log("Null");
            }
            
            computeShaderTest.SetTexture(0, "Result", _target);

            int threadGroupsX = Mathf.FloorToInt(sourceParams.nphi / 8.0f);
            int threadGroupsY = Mathf.FloorToInt(sourceParams.ntheta / 8.0f);

            computeShaderTest.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            if (visualizeRays)
            {
                _rayPointsBuffer.GetData(rds);

                // do something with rds
                Vector3 srcOrigin = srcSphere.transform.position;

                // visualize all lines
                for (int i = 0; i < rds.Length; i++)
                {
                    if (rds[i].set != 12345)
                    {
                        // skip to next ray
                        int mod = i % sourceParams.MAXINTERACTIONS;
                        int step = sourceParams.MAXINTERACTIONS - mod - 1;
                        i += step;
                        continue;
                    }

                    line = new GameObject("Line").AddComponent<LineRenderer>();
                    line.startWidth = 0.01f;
                    line.endWidth = 0.01f;
                    line.positionCount = 2;
                    line.useWorldSpace = true;

                    if (i % sourceParams.MAXINTERACTIONS == 0) // first interaction for a line, draw line from source to first interaction
                    {
                        line.SetPosition(0, srcOrigin);
                    }
                    else
                    {
                        line.SetPosition(0, rds[i - 1].origin);
                    }

                    line.SetPosition(1, rds[i].origin);
                    lines.Add(line);                    
                }

                // visualize one line
                /*for (int i = 0; i < MAXINTERACTIONS; i++)
                {
                    if (rds[i].set != 12345)
                    {
                        break;
                    }
                    line = new GameObject("Line").AddComponent<LineRenderer>();

                    line.startWidth = 0.01f;
                    line.endWidth = 0.01f;
                    line.positionCount = 2;
                    line.useWorldSpace = true;

                    if (i == 0)
                    {
                        line.SetPosition(0, srcOrigin);
                    }
                    else
                    {
                        line.SetPosition(0, rds[i - 1].origin);
                    }

                    line.SetPosition(1, rds[i].origin);
                    lines.Add(line);
                }*/
            }            

            secondCameraScript.receiveData(_target);            
        }

        if (!visualizeRays)
        {
            foreach (LineRenderer line in lines)
            {
                Destroy(line.gameObject);
            }
            lines.Clear();
        }
    }

    void BuildRTAS()
    {
        World world = world_manager.GetComponent<World>();

        rtas.ClearInstances();

        // add surface
        Mesh surfaceMesh = surface.GetComponent<MeshFilter>().mesh;
        Material surfaceMaterial = surface.GetComponent<MeshRenderer>().material;
        RayTracingMeshInstanceConfig surfaceConfig = new RayTracingMeshInstanceConfig(surfaceMesh, 0, surfaceMaterial);

        //add seafloor
        Mesh seafloorMesh = seafloor.GetComponent<MeshFilter>().mesh;
        Material seafloorMaterial = seafloor.GetComponent<MeshRenderer>().material;
        RayTracingMeshInstanceConfig seafloorConfig = new RayTracingMeshInstanceConfig(seafloorMesh, 0, seafloorMaterial);

        Mesh waterplaneMesh = waterplane.GetComponent<MeshFilter>().mesh;
        Material waterplaneMaterial = waterplane.GetComponent<MeshRenderer>().material;
        RayTracingMeshInstanceConfig waterplaneConfig = new RayTracingMeshInstanceConfig(waterplaneMesh, 0, waterplaneMaterial);

        rtas.AddInstances(surfaceConfig, surfaceInstanceData.matrices, id: 1); // add config to rtas with id, id is used to determine what object has been hit in raytracing
        if (waterplaneInstanceData != null && world.GetNrOfWaterplanes() > 0)
        {
            rtas.AddInstances(waterplaneConfig, waterplaneInstanceData.matrices, id: 2);
        }        
        rtas.AddInstances(seafloorConfig, seafloorInstanceData.matrices, id: 3);

        rtas.Build();
        Debug.Log("RTAS built");
    }    
}