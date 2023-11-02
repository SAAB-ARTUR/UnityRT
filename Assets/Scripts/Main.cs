using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityTemplateProjects;

public class Main : MonoBehaviour
{
    public ComputeShader computeShader = null;

    public GameObject srcSphere = null;
    public Camera sourceCamera = null; 
    [SerializeField] GameObject worldManager = null;
    [SerializeField] GameObject btnSSPFilePicker = null;
    [SerializeField] GameObject btnSTLFilePicker = null; 
    [SerializeField] GameObject RTModel = null;

    apiv2 api = null;

    private SourceParams.Properties? oldSourceParams = null;
    private RTModelParams.Properties? oldRTModelParams = null;
    private int oldMaxSurfaceHits = 0;
    private int oldMaxBottomHits = 0;    

    public bool doRayTracing = false;
    private bool lockRayTracing = false;
    private bool errorFree = true;

    private LineRenderer line = null;
    private List<LineRenderer> lines = new List<LineRenderer>();    

    private RayTracingAccelerationStructure rtas = null;
    private bool rebuildRTAS = false;

    private SurfaceAndBottomInstanceData surfaceInstanceData = null;
    private SurfaceAndBottomInstanceData bottomInstanceData = null; 

    private SSPFileReader _SSPFileReader = null;
    private List<SSPFileReader.SSP_Data> SSP = null;
    private ComputeBuffer SSPBuffer;

    private STLFileReader _STLFileReader = null;

    private ComputeBuffer RayPositionsBuffer;
    private float3[] rayPositions = null;
    private bool rayPositionDataAvail = false;

    private bool oldVisualiseRays = false;
    private bool oldVisualiseContributingRays = false;

    private const int BellhopTraceRaysKernelIdx = 0;
    private const int BellhopTraceContributingRaysKernelIdx = 1;
    private const int HovemTraceRaysKernelIdx = 2;
    private const int HovemTraceContributingRaysKernelIdx = 3;
    private const int HovemRTASTraceRaysKernelIdx = 5;

    struct PerRayData
    {
        public float beta;
        public uint ntop;
        public uint nbot;
        public uint ncaust;
        public float delay;
        public float curve;
        public float xn;
        public float qi;
        public float theta;
        public float phi;
        public uint contributing;
        public float TL;
        public uint target;
    }
    private int perraydataByteSize = sizeof(uint) * 5 + sizeof(float) * 8;

    private ComputeBuffer PerRayDataBuffer;
    private PerRayData[] rayData = null;
    private float dtheta = 0;
    private List<PerRayData> contributingRays = new List<PerRayData>();    

    private ComputeBuffer ContributingAnglesBuffer;    
    private List<float2> contributingAngles = new List<float2>();
    private List<bool> isEigenRay = new List<bool>();
    private ComputeBuffer PerContributingRayDataBuffer;
    private PerRayData[] PerContributingRayData = null;
    private ComputeBuffer RayTargetsBuffer;
    private ComputeBuffer NormalBuffer;
    private List<uint> rayTargets = new List<uint>();

    private ComputeBuffer debugBuf;
    private float3[] debugger;

    private ComputeBuffer FreqDampBuffer;
    private float2[] freqsdamps;

    private ComputeBuffer targetBuffer;    
    private int oldNrOfTargets = 0;

    private ComputeBuffer alphaData;
    private float[] alphas = new float[128] { (float)-0.523598775598299, (float)-0.515353125588877, (float)-0.507107475579455, (float)-0.498861825570033, (float)-0.490616175560611, (float)-0.482370525551189, (float)-0.474124875541767,
                                            (float)-0.465879225532345, (float)-0.457633575522923, (float)-0.449387925513501, (float)-0.441142275504079, (float)-0.432896625494657, (float)-0.424650975485235, (float)-0.416405325475813,
                                            (float)-0.408159675466391, (float)-0.399914025456968, (float)-0.391668375447546, (float)-0.383422725438124, (float)-0.375177075428702, (float)-0.366931425419280, (float)-0.358685775409858,
                                            (float)-0.350440125400436, (float)-0.342194475391014, (float)-0.333948825381592, (float)-0.325703175372170, (float)-0.317457525362748, (float)-0.309211875353326, (float)-0.300966225343904,
                                            (float)-0.292720575334482, (float)-0.284474925325060, (float)-0.276229275315638, (float)-0.267983625306216, (float)-0.259737975296794, (float)-0.251492325287372, (float)-0.243246675277950,
                                            (float)-0.235001025268528, (float)-0.226755375259106, (float)-0.218509725249684, (float)-0.210264075240262, (float)-0.202018425230840, (float)-0.193772775221418, (float)-0.185527125211996,
                                            (float)-0.177281475202574, (float)-0.169035825193152, (float)-0.160790175183730, (float)-0.152544525174308, (float)-0.144298875164886, (float)-0.136053225155463, (float)-0.127807575146041,
                                            (float)-0.119561925136619, (float)-0.111316275127197, (float)-0.103070625117775, (float)-0.0948249751083534, (float)-0.0865793250989313, (float)-0.0783336750895093, (float)-0.0700880250800873,
                                            (float)-0.0618423750706652, (float)-0.0535967250612432, (float)-0.0453510750518212, (float)-0.0371054250423991, (float)-0.0288597750329771, (float)-0.0206141250235551, (float)-0.0123684750141330,
                                            (float)-0.00412282500471102, (float)0.00412282500471102, (float)0.0123684750141330, (float)0.0206141250235551, (float)0.0288597750329771, (float)0.0371054250423991, (float)0.0453510750518212,
                                            (float)0.0535967250612432, (float)0.0618423750706652, (float)0.0700880250800873, (float)0.0783336750895093, (float)0.0865793250989313, (float)0.0948249751083534, (float)0.103070625117775,
                                            (float)0.111316275127197, (float)0.119561925136619, (float)0.127807575146041, (float)0.136053225155463, (float)0.144298875164886, (float)0.152544525174308, (float)0.160790175183730, (float)0.169035825193152,
                                            (float)0.177281475202574, (float)0.185527125211996, (float)0.193772775221418, (float)0.202018425230840, (float)0.210264075240262, (float)0.218509725249684, (float)0.226755375259106, (float)0.235001025268528,
                                            (float)0.243246675277950, (float)0.251492325287372, (float)0.259737975296794, (float)0.267983625306216, (float)0.276229275315638, (float)0.284474925325060, (float)0.292720575334482, (float)0.300966225343904,
                                            (float)0.309211875353326, (float)0.317457525362748, (float)0.325703175372170, (float)0.333948825381592, (float)0.342194475391014, (float)0.350440125400436, (float)0.358685775409858, (float)0.366931425419280,
                                            (float)0.375177075428702, (float)0.383422725438124, (float)0.391668375447546, (float)0.399914025456968, (float)0.408159675466391, (float)0.416405325475813, (float)0.424650975485235, (float)0.432896625494657,
                                            (float)0.441142275504079, (float)0.449387925513501, (float)0.457633575522923, (float)0.465879225532345, (float)0.474124875541767, (float)0.482370525551189, (float)0.490616175560611, (float)0.498861825570033,
                                            (float)0.507107475579455, (float)0.515353125588877, (float)0.523598775598299 };    

    // Start is called before the first frame update
    void Start()
    {
        Renderer srcRenderer = srcSphere.GetComponent<Renderer>();
        srcRenderer.material.SetColor("_Color", Color.green);

        //BuildWorld();
        rtas = new RayTracingAccelerationStructure();
        rebuildRTAS = true;

        _SSPFileReader = btnSSPFilePicker.GetComponent<SSPFileReader>();
        _STLFileReader = btnSTLFilePicker.GetComponent<STLFileReader>();

        api = GetComponent<apiv2>();

        alphaData = new ComputeBuffer(128, sizeof(float));
        alphaData.SetData(alphas);
        RTModelParams modelParams = RTModel.GetComponent<RTModelParams>();
        SetComputeBuffer("thetaData", alphaData, modelParams.RTMODEL);
    }

    private void ReleaseResources()
    {
        if (rtas != null)
        {
            rtas.Release();
            rtas = null;
        }        

        if (surfaceInstanceData != null)
        {
            surfaceInstanceData.Dispose();
            surfaceInstanceData = null;
        }

        if (bottomInstanceData != null)
        {
            bottomInstanceData.Dispose();
            bottomInstanceData = null;
        }
        
        SSPBuffer?.Release();
        SSPBuffer = null;
        RayPositionsBuffer?.Release();
        RayPositionsBuffer = null;
        PerRayDataBuffer?.Release();
        PerRayDataBuffer = null;
        FreqDampBuffer?.Release();
        FreqDampBuffer = null;
        debugBuf?.Release();
        debugBuf = null;
        NormalBuffer?.Release();
        NormalBuffer = null;
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
        RTModelParams modelParams = RTModel.GetComponent<RTModelParams>();
        World world = worldManager.GetComponent<World>();        

        if (RayPositionsBuffer == null)
        {
            RayPositionsBuffer = new ComputeBuffer(modelParams.INTEGRATIONSTEPS * world.GetNrOfTargets() * sourceParams.ntheta, 3 * sizeof(float));
            SetComputeBuffer("RayPositionsBuffer", RayPositionsBuffer, modelParams.RTMODEL);
        }

        if (rayPositions == null)
        {
            rayPositions = new float3[modelParams.INTEGRATIONSTEPS * world.GetNrOfTargets() * sourceParams.ntheta];
            rayPositionDataAvail = false;
        }

        if (PerRayDataBuffer == null)
        {
            PerRayDataBuffer = new ComputeBuffer(world.GetNrOfTargets() * sourceParams.ntheta, perraydataByteSize);
            SetComputeBuffer("PerRayDataBuffer", PerRayDataBuffer, modelParams.RTMODEL);
        }

        if (rayData == null)
        {
            rayData = new PerRayData[world.GetNrOfTargets() * sourceParams.ntheta];
        }

        if (surfaceInstanceData == null)
        {            
            surfaceInstanceData = new SurfaceAndBottomInstanceData();
        }        
        if (bottomInstanceData == null)
        {            
            bottomInstanceData = new SurfaceAndBottomInstanceData();
        }
    }

    private void SetComputeBuffer(string name, ComputeBuffer buffer, RTModelParams.RT_Model rtmodel)
    {
        if (buffer != null)
        {
            switch (rtmodel)
            {
                case RTModelParams.RT_Model.Bellhop:
                    computeShader.SetBuffer(BellhopTraceRaysKernelIdx, name, buffer);
                    break;
                case RTModelParams.RT_Model.Hovem:
                    computeShader.SetBuffer(HovemTraceRaysKernelIdx, name, buffer);
                    break;
                case RTModelParams.RT_Model.HovemRTAS:
                    computeShader.SetBuffer(HovemRTASTraceRaysKernelIdx, name, buffer);
                    break;
                default:
                    computeShader.SetBuffer(BellhopTraceRaysKernelIdx, name, buffer);
                    break;
            }            
        }
    }

    private void SetShaderParameters()
    {
        computeShader.SetMatrix("_SourceCameraToWorld", sourceCamera.cameraToWorldMatrix);
        computeShader.SetMatrix("_CameraInverseProjection", sourceCamera.projectionMatrix.inverse);
        computeShader.SetVector("srcDirection", srcSphere.transform.forward);        
        computeShader.SetVector("srcPosition", srcSphere.transform.position);        
    }

    private void BuildWorld() {
        
        World world = worldManager.GetComponent<World>();
        RTModelParams modelParams = RTModel.GetComponent<RTModelParams>();        
        
        if (modelParams.RTMODEL == RTModelParams.RT_Model.HovemRTAS && _STLFileReader.GetBottomMesh() != null) { // if RTAS is to be used and a custom bathymetry file (.stl) has been selected
            // create the custom bottom                        
            world.AddCustomBottom(_STLFileReader.GetBottomMesh());

            List<Vector3> normals = _STLFileReader.GetBottomMeshNormals();            
            NormalBuffer = new ComputeBuffer(normals.Count, 3 * sizeof(float));            
            NormalBuffer.SetData(normals.ToArray());
            SetComputeBuffer("NormalBuffer", NormalBuffer, modelParams.RTMODEL);
            world.AddCustomSurface();
            // send bounds of the volume to the gpu
            float[] boundaries = world.GetBoundaries();
            computeShader.SetFloat("xmin", boundaries[0]); // boundaries are only used in the gpu code that uses the acceleration structure
            computeShader.SetFloat("xmax", boundaries[1]);
            computeShader.SetFloat("zmin", boundaries[2]);
            computeShader.SetFloat("zmax", boundaries[3]);
        }
        else
        {
            // create flat bottom
            world.AddPlaneBottom();
            world.AddPlaneSurface();
        }

        world.AddSource(sourceCamera);
        
        
        computeShader.SetFloat("maxdepth", world.GetWaterDepth());
        
        
    }

    #region plotting
    int GetRayStartIndex(int idx, int idy)
    {
        RTModelParams modelParams = RTModel.GetComponent<RTModelParams>();     
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();        
        
        return (idy + idx * sourceParams.ntheta) * modelParams.INTEGRATIONSTEPS;
    }
    
    void Plot()
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        World world = worldManager.GetComponent<World>();
        for (int iphi = 0; iphi < world.GetNrOfTargets(); iphi++)
        {
            for (int itheta = 0; itheta < sourceParams.ntheta; itheta++)
            {
                PlotLines(iphi, itheta, rayPositions);
            }
        }
    }

    List<Vector3> RayLine(int idx, int idy, float3[] bds) {

        RTModelParams modelParams = RTModel.GetComponent<RTModelParams>();

        int offset = GetRayStartIndex(idx, idy);

        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < modelParams.INTEGRATIONSTEPS; i++)
        {            
            if (i == 0) // add the first position
            {
                positions.Add(new Vector3(rayPositions[offset].x, rayPositions[offset].y, rayPositions[offset].z));
                continue;
            }
            else
            {
                // calc distance between current point and previous
                Vector3 pos1 = positions[positions.Count-1];
                Vector3 pos2 = rayPositions[offset + i];
                float distance = MathF.Sqrt(MathF.Pow(pos1.x - pos2.x, 2) + MathF.Pow(pos1.y - pos2.y, 2) + MathF.Pow(pos1.z - pos2.z, 2));
                /*if (distance < 1) // if points are too close, don't add the current point
                {
                    continue;
                }*/
            }
            
            if (rayPositions[offset + i].y <= 0f )
            {
                positions.Add(new Vector3(rayPositions[offset + i].x, rayPositions[offset + i].y, rayPositions[offset + i].z));
            }
            else
            {
                break; // faulty position, break loop
            }
        }

        return positions;


    }

    void PlotLines(int idx, int idy, float3[] bds)
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        
        int rayIdx = idy + sourceParams.ntheta * idx;

        if (sourceParams.showContributingRaysOnly && rayData[rayIdx].contributing != 1)
        {
            return;
        }

        List<Vector3> positions = RayLine(idx, idy, bds);   

        line = new GameObject("Line").AddComponent<LineRenderer>();
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;
        line.useWorldSpace = true;

        line.positionCount = positions.Count;
        line.SetPositions(positions.ToArray());

        lines.Add(line);        
    }

    void DestroyLines()
    {
        foreach (LineRenderer line in lines) // delete lines from previous runs
        {
            Destroy(line.gameObject);
        }
        lines.Clear();
    }
    #endregion

    /// <summary>
    /// maintain data structures that are related to the gpu, update them if necessary and return the status of the operations
    /// </summary>
    /// <returns>bool</returns>
    bool MaintainBuffersArraysSSPAndWorld()
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        RTModelParams modelParams = RTModel.GetComponent<RTModelParams>();
        World world = worldManager.GetComponent<World>();
        SimpleSourceController sourceController = sourceCamera.GetComponent<SimpleSourceController>();
        bool BuffersAndArraysSuccess = true;

        bool SSPReadSuccessfully = SSPFileCheck();

        if (world.TargetHasChanged() || sourceController.HasMoved() || modelParams.HasChanged(oldRTModelParams))
        {
            // either nr of targets has changed, positions of targets has changed or the source has moved, get the targets and recalculate their respective angles from the source            
            sourceController.AckMovement();
            oldNrOfTargets = world.GetNrOfTargets();
            targetBuffer = new ComputeBuffer(oldNrOfTargets, world.GetDataSizeOfTarget());
            targetBuffer.SetData(world.GetTargets(srcSphere.transform.position.x, srcSphere.transform.position.z).ToArray());            
            SetComputeBuffer("targetBuffer", targetBuffer, modelParams.RTMODEL);
        }        
        if (sourceParams.HasChanged(oldSourceParams) || modelParams.HasChanged(oldRTModelParams) || world.TargetHasChanged()) // this could probably be written a bit nicer, some unecessary updates are done when a field is changed, but many of these are connected in some way
        {            
            world.AckTargetChange();
            try
            {
                rayPositions = new float3[modelParams.INTEGRATIONSTEPS * world.GetNrOfTargets() * sourceParams.ntheta];
                rayData = new PerRayData[world.GetNrOfTargets() * sourceParams.ntheta];
            }
            catch (OverflowException e)
            {
                BuffersAndArraysSuccess = false; // disable raytracing since buffers were not able to be created
                Debug.Log(e);
                Debug.Log("Too many rays, reduce ntheta, number of targets or number of bellhop integration steps!");

            }
            catch (Exception e)
            {
                BuffersAndArraysSuccess = false; // disable raytracing since arrays were not able to be created
                Debug.Log(e);
            }
            rayPositionDataAvail = false;
            oldSourceParams = sourceParams.ToStruct();            

            if (RayPositionsBuffer != null)
            {
                RayPositionsBuffer.Release();
                RayPositionsBuffer = null;
            }

            if (PerRayDataBuffer != null)
            {
                PerRayDataBuffer.Release();
                PerRayDataBuffer = null;
            }

            // update values in shader
            computeShader.SetFloat("theta", sourceParams.theta);
            computeShader.SetInt("ntheta", sourceParams.ntheta);

            computeShader.SetInt("_MAXSTEPS", modelParams.INTEGRATIONSTEPS);
            computeShader.SetFloat("deltas", modelParams.BELLHOPSTEPSIZE);

            dtheta = (float)sourceParams.theta / (float)(sourceParams.ntheta + 1); //TODO: Lista ut hur vinklar ska hanteras. Gör som i matlab, och sen lös det på nåt sätt
            dtheta = dtheta * MathF.PI / 180; // to radians
            computeShader.SetFloat("dtheta", dtheta);
            debugBuf = new ComputeBuffer(world.GetNrOfTargets() * sourceParams.ntheta * modelParams.INTEGRATIONSTEPS, 3 * sizeof(float));
            debugger = new float3[world.GetNrOfTargets() * sourceParams.ntheta * modelParams.INTEGRATIONSTEPS];            
            SetComputeBuffer("debugBuf", debugBuf, modelParams.RTMODEL);
        }
        if (modelParams.MAXNRSURFACEHITS != oldMaxSurfaceHits)
        {
            oldMaxSurfaceHits = modelParams.MAXNRSURFACEHITS;
            computeShader.SetInt("_MAXSURFACEHITS", modelParams.MAXNRSURFACEHITS);
        }
        if (modelParams.MAXNRBOTTOMHITS != oldMaxBottomHits)
        {
            oldMaxBottomHits = modelParams.MAXNRBOTTOMHITS;
            computeShader.SetInt("_MAXBOTTOMHITS", modelParams.MAXNRBOTTOMHITS);
        }        

        if (world.WorldHasChanged() || modelParams.HasChanged(oldRTModelParams) || _STLFileReader.BathymetryFileHasChanged())
        {
            oldRTModelParams = modelParams.ToStruct();            
            BuildWorld();
            world.AckChangeInWorld();
            _STLFileReader.AckBathymetryFileHasChanged();
            rebuildRTAS = true;           
        }
        SetComputeBuffer("thetaData", alphaData, modelParams.RTMODEL);
        return BuffersAndArraysSuccess && SSPReadSuccessfully;
    }

    /// <summary>
    /// check if the user has changed the SSP-file using the settings panel
    /// if so, read the new file and its content
    /// </summary>
    /// <returns>bool</returns>
    bool SSPFileCheck()
    {
        bool success = true;
        RTModelParams modelParams = RTModel.GetComponent<RTModelParams>();
        if (_SSPFileReader.SSPFileHasChanged())
        {
            try
            {
                _SSPFileReader.AckSSPFileHasChanged();
                SSP = _SSPFileReader.GetSSPData();

                if (SSPBuffer != null)
                {
                    SSPBuffer.Release();
                }

                SSPBuffer = new ComputeBuffer(SSP.Count, sizeof(float) * 4); // SSP_data struct consists of 4 floats
                SSPBuffer.SetData(SSP.ToArray(), 0, 0, SSP.Count);                
                SetComputeBuffer("_SSPBuffer", SSPBuffer, modelParams.RTMODEL);
                World world = worldManager.GetComponent<World>();
                world.SetWaterDepth(SSP.Last().depth);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                Debug.Log("SSP not created successfully, raytracing won't be available until SSP is created successfully");
                success = false;
            }
        }
        else if (modelParams.HasChanged(oldRTModelParams))
        {
            SetComputeBuffer("_SSPBuffer", SSPBuffer, modelParams.RTMODEL);
        }
        return success;
    }

    void BellhopComputeEigenRays()
    {
        for (int i = 0; i < contributingRays.Count - 1; i++) // TODO: här blir det problem eftersom sista strålen kan skippas
        {
            // find pairs of rays
            if (contributingRays[i + 1].theta < contributingRays[i].theta + 1.5 * dtheta && contributingRays[i].phi == contributingRays[i + 1].phi)
            {
                float n1 = contributingRays[i].xn;
                float n2 = contributingRays[i + 1].xn;
                float tot = contributingRays[i].beta + contributingRays[i + 1].beta;

                float2 angles;
                if (n1 * n2 <= 0 && tot > 0.9 && tot < 1.1)
                {
                    float w = n2 / (n2 - n1);
                    float theta = w * contributingRays[i].theta + (1 - w) * contributingRays[i + 1].theta;
                    float phi = contributingRays[i].phi;
                    angles.x = theta;
                    angles.y = phi;
                    contributingAngles.Add(angles);
                    isEigenRay.Add(true);
                    rayTargets.Add(contributingRays[i].target);
                    i++;
                }
                else
                {
                    angles.x = contributingRays[i].theta;
                    angles.y = contributingRays[i].phi;
                    contributingAngles.Add(angles);
                    isEigenRay.Add(false);
                    rayTargets.Add(contributingRays[i].target);
                }
            }
        }
    }

    void BellhopTraceContributingRays()
    {
        debugBuf = new ComputeBuffer(contributingAngles.Count, 3 * sizeof(float));
        debugger = new float3[contributingAngles.Count];
        computeShader.SetBuffer(BellhopTraceContributingRaysKernelIdx, "debugBuf", debugBuf);

        PerContributingRayData = new PerRayData[contributingAngles.Count];

        ContributingAnglesBuffer = new ComputeBuffer(contributingAngles.Count, sizeof(float) * 2);

        ContributingAnglesBuffer.SetData(contributingAngles); // fill buffer of contributing angles/values
        computeShader.SetBuffer(BellhopTraceContributingRaysKernelIdx, "ContributingAnglesData", ContributingAnglesBuffer);

        // init return data buffer
        PerContributingRayDataBuffer = new ComputeBuffer(contributingAngles.Count, perraydataByteSize);
        computeShader.SetBuffer(BellhopTraceContributingRaysKernelIdx, "ContributingRayData", PerContributingRayDataBuffer);

        computeShader.SetBuffer(BellhopTraceContributingRaysKernelIdx, "_SSPBuffer", SSPBuffer);

        RayTargetsBuffer = new ComputeBuffer(contributingRays.Count, sizeof(uint));
        RayTargetsBuffer.SetData(rayTargets.ToArray());
        computeShader.SetBuffer(BellhopTraceContributingRaysKernelIdx, "rayTargets", RayTargetsBuffer);

        freqsdamps = new float2[1];
        freqsdamps[0].x = 150000;
        freqsdamps[0].y = 0.015f / 8.6858896f;

        FreqDampBuffer = new ComputeBuffer(freqsdamps.Length, sizeof(float) * 2);
        FreqDampBuffer.SetData(freqsdamps);
        computeShader.SetBuffer(BellhopTraceContributingRaysKernelIdx, "FreqsAndDampData", FreqDampBuffer);
        computeShader.SetInt("freqsdamps", freqsdamps.Length);

        computeShader.SetBuffer(BellhopTraceContributingRaysKernelIdx, "targetBuffer", targetBuffer);

        int threadGroupsX = Mathf.FloorToInt(1);
        int threadGroupsY = Mathf.FloorToInt(contributingAngles.Count);

        // send eigenrays
        computeShader.Dispatch(BellhopTraceContributingRaysKernelIdx, threadGroupsX, threadGroupsY, 1);

        PerContributingRayDataBuffer.GetData(PerContributingRayData);

        Debug.Log("    theta     phi     T   B   C         TL          dist         delay     beta     eig");           
        for (int i = 0; i < PerContributingRayData.Length; i++)
        {            
            float theta_deg = PerContributingRayData[i].theta * 180 / MathF.PI;
            float phi_deg = PerContributingRayData[i].phi * 180 / MathF.PI;

            string data = theta_deg.ToString("F6") + " " + phi_deg.ToString("F6") + " " + PerContributingRayData[i].ntop + " " + PerContributingRayData[i].nbot + " " + PerContributingRayData[i].ncaust + " " +
                            PerContributingRayData[i].TL.ToString("F6") + " " + PerContributingRayData[i].curve.ToString("F6") + " " + PerContributingRayData[i].delay.ToString("F6") + " " +
                            PerContributingRayData[i].beta.ToString("F6") + " " + isEigenRay[i];
            Debug.Log(data);
        }
    }

    void HovemComputeEigenRays()
    {        
        // find eigenray pairs
        for (int i = 1; i < rayData.Length; i++)
        {
            if (rayData[i - 1].contributing == 0) // contributing == eigenray for now in the hovem case
            {
                if (rayData[i].ntop == rayData[i - 1].ntop && rayData[i].nbot == rayData[i - 1].nbot && rayData[i].target == rayData[i - 1].target)
                {     
                    if (rayData[i].xn * rayData[i - 1].xn <= 0)
                    {
                        rayData[i].contributing = 1;
                        rayData[i - 1].contributing = 1;
                        // add to list
                        contributingRays.Add(rayData[i - 1]);
                        contributingRays.Add(rayData[i]);                        
                        float theta = (rayData[i].theta + rayData[i - 1].theta) / 2;
                        float phi = rayData[i].phi;                        
                        contributingAngles.Add(new float2(theta, phi));
                    }
                }
            }
        }
    }

    void HovemTraceContributingRays()
    {
        debugBuf = new ComputeBuffer(contributingAngles.Count, 3 * sizeof(float));
        debugger = new float3[contributingAngles.Count];
        computeShader.SetBuffer(HovemTraceContributingRaysKernelIdx, "debugBuf", debugBuf);

        PerContributingRayData = new PerRayData[contributingRays.Count];

        ContributingAnglesBuffer = new ComputeBuffer(contributingAngles.Count, sizeof(float) * 2);

        ContributingAnglesBuffer.SetData(contributingAngles); // fill buffer of contributing angles/values
        computeShader.SetBuffer(HovemTraceContributingRaysKernelIdx, "ContributingAnglesData", ContributingAnglesBuffer);

        // init return data buffer
        PerContributingRayDataBuffer = new ComputeBuffer(contributingRays.Count, perraydataByteSize);
        PerContributingRayDataBuffer.SetData(contributingRays);
        computeShader.SetBuffer(HovemTraceContributingRaysKernelIdx, "ContributingRayData", PerContributingRayDataBuffer);

        computeShader.SetBuffer(HovemTraceContributingRaysKernelIdx, "_SSPBuffer", SSPBuffer);

        RayTargetsBuffer = new ComputeBuffer(contributingRays.Count, sizeof(uint));
        RayTargetsBuffer.SetData(rayTargets.ToArray());
        computeShader.SetBuffer(HovemTraceContributingRaysKernelIdx, "rayTargets", RayTargetsBuffer);

        freqsdamps = new float2[1];
        freqsdamps[0].x = 150000;
        freqsdamps[0].y = 0.015f / 8.6858896f;

        FreqDampBuffer = new ComputeBuffer(freqsdamps.Length, sizeof(float) * 2);
        FreqDampBuffer.SetData(freqsdamps);
        computeShader.SetBuffer(HovemTraceContributingRaysKernelIdx, "FreqsAndDampData", FreqDampBuffer);
        computeShader.SetInt("freqsdamps", freqsdamps.Length);

        computeShader.SetBuffer(HovemTraceContributingRaysKernelIdx, "targetBuffer", targetBuffer);

        int threadGroupsX = Mathf.FloorToInt(1);
        int threadGroupsY = Mathf.FloorToInt(contributingAngles.Count);

        // send eigenrays
        computeShader.Dispatch(HovemTraceContributingRaysKernelIdx, threadGroupsX, threadGroupsY, 1);

        PerContributingRayDataBuffer.GetData(PerContributingRayData);

        Debug.Log("    theta     phi     T   B   C         TL          dist         delay");           
        for (int i = 0; i < PerContributingRayData.Length; i++)
        {            
            if (PerContributingRayData[i].curve > 0) // skip the empty rays
            {
                float theta_deg = PerContributingRayData[i].theta * 180 / MathF.PI;
                float phi_deg = PerContributingRayData[i].phi * 180 / MathF.PI;

                string data = theta_deg.ToString("F6") + " " + phi_deg.ToString("F6") + " " + PerContributingRayData[i].ntop + " " + PerContributingRayData[i].nbot + " " + PerContributingRayData[i].ncaust + " " +
                                PerContributingRayData[i].TL.ToString("F6") + " " + PerContributingRayData[i].curve.ToString("F6") + " " + PerContributingRayData[i].delay.ToString("F6");
                Debug.Log(data);
            }            
        }
    }

    // update is run every frame, it continously checks for changes in the settings available to the user, it also waits for the user to activate the raytracing
    void Update()
    {
        //
        // CHECK FOR UPDATES
        //

        errorFree = MaintainBuffersArraysSSPAndWorld();               

        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();        
        if (oldVisualiseRays != sourceParams.visualizeRays || oldVisualiseContributingRays != sourceParams.showContributingRaysOnly) // if the user toggles the visualization options after raytracing has been done
        {
            oldVisualiseRays = sourceParams.visualizeRays;
            oldVisualiseContributingRays = sourceParams.showContributingRaysOnly;

            if (rayPositionDataAvail && sourceParams.visualizeRays)
            {
                DestroyLines();
                Plot();
            }
        }

        if (Input.GetKey(KeyCode.C)){ // user input
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

        if (((!lockRayTracing && doRayTracing) || sourceParams.sendRaysContinously) && errorFree) // do raytracing if the user has pressed key C. only do it once though. or do it continously
        {
            RTModelParams modelParams = RTModel.GetComponent<RTModelParams>();

            rayPositionDataAvail = false;
            DestroyLines();

            lockRayTracing = true; // disable raytracing being done several times during one keypress            

            DateTime time1 = DateTime.Now; // measure time to do raytracing

            CreateResources();            

            SetShaderParameters();            
            
            int threadGroupsX = Mathf.FloorToInt(worldManager.GetComponent<World>().GetNrOfTargets());
            int threadGroupsY = Mathf.FloorToInt(sourceParams.ntheta / 8.0f);           

            //send rays
            if (modelParams.RTMODEL == RTModelParams.RT_Model.Bellhop)
            {
                computeShader.Dispatch(BellhopTraceRaysKernelIdx, threadGroupsX, threadGroupsY, 1);
            }
            else if (modelParams.RTMODEL == RTModelParams.RT_Model.Hovem)
            {                
                computeShader.Dispatch(HovemTraceRaysKernelIdx, threadGroupsX, threadGroupsY, 1);
            }
            else if (modelParams.RTMODEL == RTModelParams.RT_Model.HovemRTAS)
            {
                if (rebuildRTAS)
                {
                    BuildRTAS();
                    rebuildRTAS = false;
                }
                computeShader.Dispatch(HovemRTASTraceRaysKernelIdx, threadGroupsX, threadGroupsY, 1);
            }

            // read results from buffers into arrays
            RayPositionsBuffer.GetData(rayPositions);
            rayPositionDataAvail = true;
            PerRayDataBuffer.GetData(rayData);

            //debugBuf.GetData(debugger);
            //Debug.Log("------------------------------------------------------------------------------");
            //for (int i = 17 * modelParams.INTEGRATIONSTEPS; i < 18 * modelParams.INTEGRATIONSTEPS/*debugger.Length*/; i++)
            //{
            //    Debug.Log("i: " + i + " x: " + debugger[i].x + " y: " + debugger[i].y + " z: " + debugger[i].z);
            //}
            //Debug.Log("------------------------------------------------------------------------------");

            if (modelParams.RTMODEL == RTModelParams.RT_Model.Bellhop)
            {                
                // keep contributing rays only            
                for (int i = 0; i < rayData.Length; i++)
                {
                    if (rayData[i].contributing == 1)
                    {
                        contributingRays.Add(rayData[i]);
                    }
                }

                // check if a pair of contributing rays can be combined into an eigenray
                BellhopComputeEigenRays();

                if (contributingAngles.Count > 0)
                {
                    BellhopTraceContributingRays();               
                }

            }
            else if (modelParams.RTMODEL == RTModelParams.RT_Model.Hovem)
            {                
                HovemComputeEigenRays();

                // trace the contributing rays again
                if (contributingRays.Count > 0)
                {                    
                    HovemTraceContributingRays();

                    //debugBuf.GetData(debugger);
                    //Debug.Log("------------------------------------------------------------------------------");
                    //for (int i = 0; i < debugger.Length; i++)
                    //{
                    //    Debug.Log("i: " + i + " x: " + debugger[i].x + " y: " + debugger[i].y + " z: " + debugger[i].z);
                    //}
                    //Debug.Log("------------------------------------------------------------------------------");
                }
            }

            // Communicate the rays with the api
            if (api.enabled)
            {

                World world = worldManager.GetComponent<World>();
                // Create a ray collection
                List<List<Vector3>> rays = new List<List<Vector3>>();

                for (int iphi = 0; iphi < world.GetNrOfTargets(); iphi++){
                    for (int itheta = 0; itheta < sourceParams.ntheta; itheta++)
                    {

                        rays.Add(RayLine(iphi, itheta, rayPositions));

                    }
                }

                api.Rays(rays);
                // Debug.Log(rays[0][0].ToString());
            }           

            DateTime time2 = DateTime.Now;

            TimeSpan ts = time2 - time1;            

            Debug.Log("Time elapsed: " + ts.TotalMilliseconds + "ms");

            if (sourceParams.visualizeRays)
            {
                Plot();
            }
        }

        if (!sourceParams.visualizeRays)
        {
            DestroyLines();
        }
        contributingRays.Clear();
        contributingAngles.Clear();
        isEigenRay.Clear();
        rayTargets.Clear();

    }


    // For calls from the API
    public void TraceNow() {


        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        Debug.Log("HEJ moltas");


        CreateResources();
        

        SetShaderParameters();

        World world = worldManager.GetComponent<World>();

        int threadGroupsX = Mathf.FloorToInt(world.GetNrOfTargets());
        int threadGroupsY = Mathf.FloorToInt(sourceParams.ntheta / 8.0f);


        
        computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        Debug.Log("Dispatching done");

        RayPositionsBuffer.GetData(rayPositions);

        // Create a ray collection
        List<List<Vector3>> rays = new List<List<Vector3>>();

        // API should really be enabled if trace has been called. Just to be certain. 
        if (api.enabled) {

            for (int iphi = 0; iphi < world.GetNrOfTargets(); iphi++){
                for (int itheta = 0; itheta < sourceParams.ntheta; itheta++)
                {

                    rays.Add(RayLine(iphi, itheta, rayPositions));

                }
            }

            api.Rays(rays);
        }
        
        
    }

    void BuildRTAS()
    {
        World world = worldManager.GetComponent<World>();

        rtas.ClearInstances();

        GameObject surface = world.GetSurface();
        GameObject bottom = world.GetBottom();

        // add surface
        Mesh surfaceMesh = surface.GetComponent<MeshFilter>().mesh;
        Material surfaceMaterial = surface.GetComponent<MeshRenderer>().material;
        RayTracingMeshInstanceConfig surfaceConfig = new RayTracingMeshInstanceConfig(surfaceMesh, 0, surfaceMaterial);
        rtas.AddInstances(surfaceConfig, surfaceInstanceData.matrices, id: 1); // add config to rtas with id, id is used to determine what object has been hit in raytracing

        // add bottom
        Mesh bottomMesh = bottom.GetComponent<MeshFilter>().mesh;
        Material bottomMaterial = bottom.GetComponent<MeshRenderer>().material;
        RayTracingMeshInstanceConfig bottomConfig = new RayTracingMeshInstanceConfig(bottomMesh, 0, bottomMaterial);
        rtas.AddInstances(bottomConfig, bottomInstanceData.matrices, id: 2);       

        rtas.Build();        

        computeShader.SetRayTracingAccelerationStructure(HovemRTASTraceRaysKernelIdx, "g_AccelStruct", rtas);
    }
}

// TODOS:

// 3: skapa ett knippe av strålar för 3D-hovem