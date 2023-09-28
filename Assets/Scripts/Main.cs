using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

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
    [SerializeField] GameObject btnFilePicker = null;
    [SerializeField] GameObject bellhop = null;

    private SourceParams.Properties? oldSourceParams = null;
    private BellhopParams.Properties? oldBellhopParams = null;

    //private ComputeBuffer _rayPointsBuffer;
    //private RayData[] rds = null;

    private RayTracingVisualization sourceCameraScript = null;
    private RenderTexture _target;

    private bool doRayTracing = false;
    private bool lockRayTracing = false;    

    private LineRenderer line = null;
    private List<LineRenderer> lines = new List<LineRenderer>();

    private RayTracingAccelerationStructure rtas = null;
    private bool rebuildRTAS = false;

    private SurfaceAndSeafloorInstanceData surfaceInstanceData = null;
    private SurfaceAndSeafloorInstanceData seafloorInstanceData = null;
    private WaterplaneInstanceData waterplaneInstanceData = null;
    private TargetInstanceData targetInstanceData = null;    

    private Vector3 oldTargetPostion;

    private SSPFileReader _SSPFileReader = null;
    private List<SSPFileReader.SSP_Data> SSP = null;
    private ComputeBuffer _SSPBuffer;

    //private int bellhop_size = 1000; //4096;
    private ComputeBuffer xrayBuf;
    private float3[] bds = null;

    struct RayData
    {
        public Vector3 origin;
        public int set;
    };
    private int raydatabytesize = 16; // update this if the struct RayData is modified

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

        //_rayPointsBuffer?.Release();
        _SSPBuffer?.Release();
        xrayBuf?.Release();
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
        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();

        /*if (_rayPointsBuffer == null)
        {
            _rayPointsBuffer = new ComputeBuffer(sourceParams.ntheta*sourceParams.nphi*sourceParams.MAXINTERACTIONS, raydatabytesize);
        }*/

        if (xrayBuf == null)
        {            
            xrayBuf = new ComputeBuffer(bellhopParams.BELLHOPINTEGRATIONSTEPS * sourceParams.nphi * sourceParams.ntheta, 3 * sizeof(float));
            SetComputeBuffer("xrayBuf", xrayBuf);
        }

        /*if (rds == null)
        {
            rds = new RayData[sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS];
            Debug.Log(rds.Length);
        }*/

        if (bds == null)
        {            
            bds = new float3[bellhopParams.BELLHOPINTEGRATIONSTEPS * sourceParams.nphi * sourceParams.ntheta];
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

        //SetComputeBuffer("_RayPoints", _rayPointsBuffer);        
        //SetComputeBuffer("xrayBuf", xrayBuf);        

        computeShader.SetVector("srcDirection", srcSphere.transform.forward);
        computeShader.SetVector("srcPosition", srcSphere.transform.position);
        computeShader.SetVector("receiverPosition", targetSphere.transform.position);
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
        computeShader.SetFloat("depth", world.GetWaterDepth());
    }

    private void OnEnable()
    {
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

        BuildWorld();        
        rebuildRTAS = true;

        oldTargetPostion = targetSphere.transform.position;

        _SSPFileReader = btnFilePicker.GetComponent<SSPFileReader>();
    }

    int GetStartIndexBellhop(int idx, int idy)
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();

        return (idy * sourceParams.nphi + idx) * bellhopParams.BELLHOPINTEGRATIONSTEPS;
    }

    void PlotBellhop(int idx, int idy)
    {
        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();

        int offset = GetStartIndexBellhop(idx, idy);

        line = new GameObject("Line").AddComponent<LineRenderer>();
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;
        line.useWorldSpace = true;

        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < bellhopParams.BELLHOPINTEGRATIONSTEPS; i++)
        {
            if (bds[offset + i].x != 0f || bds[offset + i].y != 0f || bds[offset + i].z != 0f)
            {
                positions.Add(new Vector3(bds[offset + i].x, bds[offset + i].y, bds[offset + i].z));
            }
            else
            {
                break;
            }
        }

        line.positionCount = positions.Count;
        line.SetPositions(positions.ToArray());

        lines.Add(line);

    }

    // Update is called once per frame
    void Update()
    {        
        //
        // CHECK FOR UPDATES
        //
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();

        if (sourceParams.HasChanged(oldSourceParams) || bellhopParams.HasChanged(oldBellhopParams))
            {
            //Debug.Log("Reeinit raybuffer");
            // reinit rds array
            //rds = new RayData[sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS];

            // reinit raydatabuffer
            /*if (_rayPointsBuffer != null)
            {
                _rayPointsBuffer.Release();
            }
            _rayPointsBuffer = new ComputeBuffer(sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS, raydatabytesize);*/

            bds = new float3[bellhopParams.BELLHOPINTEGRATIONSTEPS * sourceParams.nphi * sourceParams.ntheta];
            oldSourceParams = sourceParams.ToStruct();
            oldBellhopParams = bellhopParams.ToStruct();

            if (xrayBuf != null)
            {
                xrayBuf.Release();
                xrayBuf = null;
            }

            // update values in shader
            computeShader.SetInt("theta", sourceParams.theta);
            computeShader.SetInt("ntheta", sourceParams.ntheta);
            computeShader.SetInt("phi", sourceParams.phi);
            computeShader.SetInt("nphi", sourceParams.nphi);

            computeShader.SetInt("_BELLHOPSIZE", bellhopParams.BELLHOPINTEGRATIONSTEPS);
            computeShader.SetFloat("deltas", bellhopParams.BELLHOPSTEPSIZE);
        }

        World world = world_manager.GetComponent<World>();

        if (_SSPFileReader.SSPFileHasChanged())
        {
            _SSPFileReader.AckSSPFileHasChanged();            
            SSP = _SSPFileReader.GetSSPData();

            if (_SSPBuffer != null)
            {
                _SSPBuffer.Release();
            }
            _SSPBuffer = new ComputeBuffer(SSP.Count, sizeof(float)*4); // SSP_data struct consists of 4 floats
            _SSPBuffer.SetData(SSP.ToArray(), 0, 0, SSP.Count);
            SetComputeBuffer("_SSPBuffer", _SSPBuffer);
            world.SetNrOfWaterplanes(SSP.Count - 2);
            world.SetWaterDepth(SSP.Last().depth);
            _SSPFileReader.UpdateDepthSlider();
        }        
        
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
        
        //
        // CHECK FOR UPDATES OVER //
        //

        if ((!lockRayTracing && doRayTracing) || sourceParams.sendRaysContinously) // do raytracing if the user has pressed key C. only do it once though. or do it continously
        {
            foreach (LineRenderer line in lines)
            {
                Destroy(line.gameObject);
            }
            lines.Clear();

            lockRayTracing = true; // disable raytracing being done several times during one keypress
            // do raytracing

            CreateResources();

            if (rebuildRTAS)
            {
                BuildRTAS();
                rebuildRTAS = false;
            }            

            SetShaderParameters();

            InitRenderTexture(sourceParams);
            
            computeShader.SetTexture(0, "Result", _target);

            int threadGroupsX = Mathf.FloorToInt(sourceParams.nphi / 8.0f);
            int threadGroupsY = Mathf.FloorToInt(sourceParams.ntheta / 8.0f);

            computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
     

            xrayBuf.GetData(bds);
 

            if (sourceParams.visualizeRays)
            {
                for (int itheta = 0; itheta < sourceParams.ntheta; itheta++) {
                    PlotBellhop((int)sourceParams.nphi/2, itheta);
                }

                //_rayPointsBuffer.GetData(rds);

                //Vector3 srcOrigin = srcSphere.transform.position;

                //// visualize all lines
                //for (int i = 0; i < rds.Length/sourceParams.MAXINTERACTIONS; i++)
                //{
                //    List<Vector3> positions = new List<Vector3>();
                //    if (rds[i*sourceParams.MAXINTERACTIONS].set != 12345) // check if the ray hit something
                //    {                        
                //        continue;
                //    }

                //    line = new GameObject("Line").AddComponent<LineRenderer>();
                //    line.startWidth = 0.01f;
                //    line.endWidth = 0.01f;
                //    line.positionCount = 2;
                //    line.useWorldSpace = true;

                //    // add ray source and first hit
                //    positions.Add(srcOrigin);
                //    positions.Add(rds[i * sourceParams.MAXINTERACTIONS].origin);                    

                //    for (int j = 1; j < sourceParams.MAXINTERACTIONS; j++)
                //    {
                //        if (rds[i*sourceParams.MAXINTERACTIONS + j].set != 12345) // check if the next ray hit or miss
                //        {
                //            break;
                //        }
                //        positions.Add(rds[i * sourceParams.MAXINTERACTIONS + j].origin); // add next hit                        
                //    }

                //    line.positionCount = positions.Count;
                //    line.SetPositions(positions.ToArray());                    
                //    lines.Add(line);
                //}

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

            //empty bds and buffer


            //sourceCameraScript.receiveData(_target);
            //xrayBuf.Dispose();
            //xrayBuf.Release();
            //xrayBuf = new ComputeBuffer(bellhop_size * sourceParams.nphi * sourceParams.ntheta, 3 * sizeof(float));

            //Array.Clear(bds, 0, bds.Length);
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
        //World world = world_manager.GetComponent<World>();

        //rtas.ClearInstances();

        //// add surface
        //Mesh surfaceMesh = surface.GetComponent<MeshFilter>().mesh;
        //Material surfaceMaterial = surface.GetComponent<MeshRenderer>().material;
        //RayTracingMeshInstanceConfig surfaceConfig = new RayTracingMeshInstanceConfig(surfaceMesh, 0, surfaceMaterial);
        //rtas.AddInstances(surfaceConfig, surfaceInstanceData.matrices, id: 1); // add config to rtas with id, id is used to determine what object has been hit in raytracing

        //// add seafloor
        //Mesh seafloorMesh = seafloor.GetComponent<MeshFilter>().mesh;
        //Material seafloorMaterial = seafloor.GetComponent<MeshRenderer>().material;
        //RayTracingMeshInstanceConfig seafloorConfig = new RayTracingMeshInstanceConfig(seafloorMesh, 0, seafloorMaterial);
        //rtas.AddInstances(seafloorConfig, seafloorInstanceData.matrices, id: 3);

        //// add waterplane(s)
        //Mesh waterplaneMesh = waterplane.GetComponent<MeshFilter>().mesh;
        //Material waterplaneMaterial = waterplane.GetComponent<MeshRenderer>().material;
        //RayTracingMeshInstanceConfig waterplaneConfig = new RayTracingMeshInstanceConfig(waterplaneMesh, 0, waterplaneMaterial);        
        //if (waterplaneInstanceData != null && world.GetNrOfWaterplanes() > 0)
        //{
        //    rtas.AddInstances(waterplaneConfig, waterplaneInstanceData.matrices, id: 2);
        //    Debug.Log(waterplaneInstanceData.matrices.Length);
        //}        

        //// targetmesh is a predefined mesh in unity, its vertices will all be defined in local coordinates, therefore a copy of the mesh is created but the vertices
        //// are defined in global coordinates, this copy is used in the acceleration structure to make sure that the ray tracing works properly. these actions will be 
        //// necessary on all predefined meshes
        //Mesh targetMesh = targetSphere.GetComponent<MeshFilter>().mesh;
        //Vector3[] transformed_vertices = new Vector3[targetMesh.vertexCount];

        //targetSphere.transform.TransformPoints(targetMesh.vertices, transformed_vertices);

        //Material targetMaterial = targetSphere.GetComponent<MeshRenderer>().material;

        //Mesh realTargetMesh = new Mesh();
        //realTargetMesh.vertices = transformed_vertices;
        //realTargetMesh.triangles = targetMesh.triangles;
        //realTargetMesh.normals = targetMesh.normals;
        //realTargetMesh.tangents = targetMesh.tangents;

        //RayTracingMeshInstanceConfig targetConfig = new RayTracingMeshInstanceConfig(realTargetMesh, 0, targetMaterial);

        //rtas.AddInstances(targetConfig, targetInstanceData.matrices, id: 4);

        //rtas.Build();
        //Debug.Log("RTAS built");

        //computeShader.SetRayTracingAccelerationStructure(0, "g_AccelStruct", rtas);
    }
}