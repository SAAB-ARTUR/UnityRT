using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class Main : MonoBehaviour
{    
    public ComputeShader computeShader = null;

    [SerializeField] GameObject srcSphere = null;
    [SerializeField] GameObject targetSphere = null;
    [SerializeField] GameObject surface = null;
    [SerializeField] GameObject seafloor = null;
    [SerializeField] GameObject waterplane = null;
    [SerializeField] Camera sourceCamera = null; 
    [SerializeField] GameObject world_manager = null;

    private SourceParams.Properties? oldSourceParams = null;

    private ComputeBuffer _rayPointsBuffer;
    
    RayTracingVisualization sourceCameraScript = null;
    private RenderTexture _target;

    private bool doRayTracing = false;
    private bool lockRayTracing = false;

    RayData[] rds = null;

    LineRenderer line = null;
    private List<LineRenderer> lines = new List<LineRenderer>();

    RayTracingAccelerationStructure rtas = null;
    private bool rebuildRTAS = false;

    SurfaceAndSeafloorInstanceData surfaceInstanceData = null;
    SurfaceAndSeafloorInstanceData seafloorInstanceData = null;
    WaterplaneInstanceData waterplaneInstanceData = null;
    TargetInstanceData targetInstanceData = null;    

    private Vector3 oldTargetPostion;

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
        if (targetInstanceData != null)
        {
            targetInstanceData.Dispose();
            targetInstanceData = null;
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
            Debug.Log(rds.Length);
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

        if (targetInstanceData == null)
        {
            if (targetInstanceData != null)
            {
                targetInstanceData.Dispose();
            }
            targetInstanceData = new TargetInstanceData();
        }
    }

    private void SetComputeBuffer(string name, ComputeBuffer buffer)
    {
        if (buffer != null)
        {        
            computeShader.SetBuffer(0, name, buffer);
        }
    }

    private void SetShaderParameters()
    {
        computeShader.SetMatrix("_SourceCameraToWorld", sourceCamera.cameraToWorldMatrix);
        computeShader.SetMatrix("_CameraInverseProjection", sourceCamera.projectionMatrix.inverse);
        computeShader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
        computeShader.SetFloat("_Seed", UnityEngine.Random.value);
        
        SetComputeBuffer("_RayPoints", _rayPointsBuffer);

        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();

        computeShader.SetInt("theta", sourceParams.theta);
        computeShader.SetInt("ntheta", sourceParams.ntheta);
        computeShader.SetInt("phi", sourceParams.phi);
        computeShader.SetInt("nphi", sourceParams.nphi);
        computeShader.SetVector("srcDirection", srcSphere.transform.forward);

        computeShader.SetInt("_MAXINTERACTIONS", sourceParams.MAXINTERACTIONS);

        computeShader.SetRayTracingAccelerationStructure(0, "g_AccelStruct", rtas);
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
        world.AddSource(sourceCamera);
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
        if (sourceCamera != null)
        {
            sourceCameraScript = sourceCamera.GetComponent<RayTracingVisualization>();
        }

        rtas = new RayTracingAccelerationStructure();
    }

    // Start is called before the first frame update
    void Start()
    {        
        Renderer srcRenderer = srcSphere.GetComponent<Renderer>();
        srcRenderer.material.SetColor("_Color", Color.green);
        Debug.Log("Start");

        BuildWorld();        
        rebuildRTAS = true;

        oldTargetPostion = targetSphere.transform.position;
    }

    // Update is called once per frame
    void Update()
    {   
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();

        
        if (sourceParams.HasChanged(oldSourceParams))
            {
            //Debug.Log("Reeinit raybuffer");
            // reinit rds array
            rds = new RayData[sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS];
            
            // reinit raydatabuffer
            if (_rayPointsBuffer != null)
            {
                _rayPointsBuffer.Release();
            }
            _rayPointsBuffer = new ComputeBuffer(sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS, raydatabytesize);            
            
            oldSourceParams = sourceParams.ToStruct();
        }

        World world = world_manager.GetComponent<World>();
        if (world.StateChanged())
        {
            BuildWorld();
            rebuildRTAS = true;
        }

        if (oldTargetPostion != targetSphere.transform.position) // flytta till world??
        {
            oldTargetPostion = targetSphere.transform.position;
            rebuildRTAS = true;
        }

        if (Input.GetKey(KeyCode.C)){
            doRayTracing = true;            
        }
        else
        {
            doRayTracing = false;
            lockRayTracing = false;
        }        

        if ((!lockRayTracing && doRayTracing) || sourceParams.sendRaysContinously) // do raytracing if the user has pressed key C. only do it once though. or do it continously
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
            
            computeShader.SetTexture(0, "Result", _target);

            int threadGroupsX = Mathf.FloorToInt(sourceParams.nphi / 8.0f);
            int threadGroupsY = Mathf.FloorToInt(sourceParams.ntheta / 8.0f);

            computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            if (sourceParams.visualizeRays)
            {
                _rayPointsBuffer.GetData(rds);
                
                Vector3 srcOrigin = srcSphere.transform.position;

                // visualize all lines
                for (int i = 0; i < rds.Length/sourceParams.MAXINTERACTIONS; i++)
                {
                    List<Vector3> positions = new List<Vector3>();
                    if (rds[i*sourceParams.MAXINTERACTIONS].set != 12345) // check if the ray hit something
                    {                        
                        continue;
                    }

                    line = new GameObject("Line").AddComponent<LineRenderer>();
                    line.startWidth = 0.01f;
                    line.endWidth = 0.01f;
                    line.positionCount = 2;
                    line.useWorldSpace = true;

                    // add ray source and first hit
                    positions.Add(srcOrigin);
                    positions.Add(rds[i * sourceParams.MAXINTERACTIONS].origin);                    

                    for (int j = 1; j < sourceParams.MAXINTERACTIONS; j++)
                    {
                        if (rds[i*sourceParams.MAXINTERACTIONS + j].set != 12345) // check if the next ray hit or miss
                        {
                            break;
                        }
                        positions.Add(rds[i * sourceParams.MAXINTERACTIONS + j].origin); // add next hit                        
                    }
                    
                    line.positionCount = positions.Count;
                    line.SetPositions(positions.ToArray());                    
                    lines.Add(line);
                }

                // visualize one line
                /*for (int i = 0; i < sourceParams.MAXINTERACTIONS; i++)
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

            sourceCameraScript.receiveData(_target);
        }

        if (!sourceParams.visualizeRays)
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
        rtas.AddInstances(surfaceConfig, surfaceInstanceData.matrices, id: 1); // add config to rtas with id, id is used to determine what object has been hit in raytracing

        // add seafloor
        Mesh seafloorMesh = seafloor.GetComponent<MeshFilter>().mesh;
        Material seafloorMaterial = seafloor.GetComponent<MeshRenderer>().material;
        RayTracingMeshInstanceConfig seafloorConfig = new RayTracingMeshInstanceConfig(seafloorMesh, 0, seafloorMaterial);
        rtas.AddInstances(seafloorConfig, seafloorInstanceData.matrices, id: 3);

        // add waterplane(s)
        Mesh waterplaneMesh = waterplane.GetComponent<MeshFilter>().mesh;
        Material waterplaneMaterial = waterplane.GetComponent<MeshRenderer>().material;
        RayTracingMeshInstanceConfig waterplaneConfig = new RayTracingMeshInstanceConfig(waterplaneMesh, 0, waterplaneMaterial);        
        if (waterplaneInstanceData != null && world.GetNrOfWaterplanes() > 0)
        {
            rtas.AddInstances(waterplaneConfig, waterplaneInstanceData.matrices, id: 2);
            Debug.Log(waterplaneInstanceData.matrices.Length);
        }        

        // targetmesh is a predefined mesh in unity, its vertices will all be defined in local coordinates, therefore a copy of the mesh is created but the vertices
        // are defined in global coordinates, this copy is used in the acceleration structure to make sure that the ray tracing works properly. these actions will be 
        // necessary on all predefined meshes
        Mesh targetMesh = targetSphere.GetComponent<MeshFilter>().mesh;
        Vector3[] transformed_vertices = new Vector3[targetMesh.vertexCount];

        targetSphere.transform.TransformPoints(targetMesh.vertices, transformed_vertices);

        Material targetMaterial = targetSphere.GetComponent<MeshRenderer>().material;

        Mesh realTargetMesh = new Mesh();
        realTargetMesh.vertices = transformed_vertices;
        realTargetMesh.triangles = targetMesh.triangles;
        realTargetMesh.normals = targetMesh.normals;
        realTargetMesh.tangents = targetMesh.tangents;

        RayTracingMeshInstanceConfig targetConfig = new RayTracingMeshInstanceConfig(realTargetMesh, 0, targetMaterial);

        rtas.AddInstances(targetConfig, targetInstanceData.matrices, id: 4);

        rtas.Build();
        Debug.Log("RTAS built");
    }    
}