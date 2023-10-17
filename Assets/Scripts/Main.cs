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
    public GameObject targetSphere = null;
    [SerializeField] GameObject surface = null;
    [SerializeField] GameObject seafloor = null;
    [SerializeField] GameObject waterplane = null;
    public Camera sourceCamera = null; 
    [SerializeField] GameObject worldManager = null;
    [SerializeField] GameObject btnFilePicker = null;
    [SerializeField] GameObject bellhop = null;    

    apiv2 api = null;

    private SourceParams.Properties? oldSourceParams = null;
    private BellhopParams.Properties? oldBellhopParams = null;
    private int oldMaxSurfaceHits = 0;
    private int oldMaxBottomHits = 0;    

    //private RayTracingVisualization sourceCameraScript = null;    

    public bool doRayTracing = false;
    private bool lockRayTracing = false;
    private bool errorFree = true;

    private LineRenderer line = null;
    private List<LineRenderer> lines = new List<LineRenderer>();

    private RayTracingAccelerationStructure rtas = null;
    private bool rebuildRTAS = false;

    /*private SurfaceAndSeafloorInstanceData surfaceInstanceData = null;
    private SurfaceAndSeafloorInstanceData seafloorInstanceData = null;
    private WaterplaneInstanceData waterplaneInstanceData = null;
    private TargetInstanceData targetInstanceData = null;*/    

    private SSPFileReader _SSPFileReader = null;
    private List<SSPFileReader.SSP_Data> SSP = null;
    private ComputeBuffer SSPBuffer;

    private ComputeBuffer RayPositionsBuffer;
    private float3[] rayPositions = null;
    private bool rayPositionDataAvail = false;

    private bool oldVisualiseRays = false;
    private bool oldVisualiseContributingRays = false;

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
    private List<uint> rayTargets = new List<uint>();

    /*private ComputeBuffer debugBuf;
    private float3[] debugger;*/

    private ComputeBuffer FreqDampBuffer;
    private float2[] freqsdamps;

    private ComputeBuffer targetBuffer;    
    private int oldNrOfTargets = 0;
    
    private void ReleaseResources()
    {
        if (rtas != null)
        {
            rtas.Release();
            rtas = null;
        }        

        /*if (surfaceInstanceData != null)
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
        }*/
        
        SSPBuffer?.Release();
        SSPBuffer = null;
        RayPositionsBuffer?.Release();
        RayPositionsBuffer = null;
        PerRayDataBuffer?.Release();
        PerRayDataBuffer = null;
        FreqDampBuffer?.Release();
        FreqDampBuffer = null;
        //debugBuf?.Release();
        //debugBuf = null;
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
        World world = worldManager.GetComponent<World>();        

        if (RayPositionsBuffer == null)
        {
            RayPositionsBuffer = new ComputeBuffer(bellhopParams.BELLHOPINTEGRATIONSTEPS * world.GetNrOfTargets() * sourceParams.ntheta, 3 * sizeof(float));
            SetComputeBuffer("RayPositionsBuffer", RayPositionsBuffer);
        }

        if (rayPositions == null)
        {
            rayPositions = new float3[bellhopParams.BELLHOPINTEGRATIONSTEPS * world.GetNrOfTargets() * sourceParams.ntheta];
            rayPositionDataAvail = false;
        }

        if (PerRayDataBuffer == null)
        {
            PerRayDataBuffer = new ComputeBuffer(world.GetNrOfTargets() * sourceParams.ntheta, perraydataByteSize);
            SetComputeBuffer("PerRayDataBuffer", PerRayDataBuffer);
        }

        if (rayData == null)
        {
            rayData = new PerRayData[world.GetNrOfTargets() * sourceParams.ntheta];
        }

        /*if (surfaceInstanceData == null)
        {            
            surfaceInstanceData = new SurfaceAndSeafloorInstanceData();
        }        
        if (seafloorInstanceData == null)
        {            
            seafloorInstanceData = new SurfaceAndSeafloorInstanceData();
        }*/

        /*World world = world_manager.GetComponent<World>();
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
        }*/
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
        computeShader.SetVector("srcDirection", srcSphere.transform.forward);        
        computeShader.SetVector("srcPosition", srcSphere.transform.position);        
    }

    private void BuildWorld() {
        World world = worldManager.GetComponent<World>();
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
        /*if (sourceCamera != null)
        {
            sourceCameraScript = sourceCamera.GetComponent<RayTracingVisualization>();
        }*/

        //rtas = new RayTracingAccelerationStructure();
    }

    // Start is called before the first frame update
    void Start()
    {        
        Renderer srcRenderer = srcSphere.GetComponent<Renderer>();
        srcRenderer.material.SetColor("_Color", Color.green);

        BuildWorld();        
        rebuildRTAS = true;        

        _SSPFileReader = btnFilePicker.GetComponent<SSPFileReader>();

        api = GetComponent<apiv2>();
    }

    int GetStartIndexBellhop(int idx, int idy)
    {        
        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();        
        
        return (idy + idx * sourceParams.ntheta) * bellhopParams.BELLHOPINTEGRATIONSTEPS;
    }
    
    void Plot()
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        World world = worldManager.GetComponent<World>();
        for (int iphi = 0; iphi < world.GetNrOfTargets(); iphi++)
        {
            for (int itheta = 0; itheta < sourceParams.ntheta; itheta++)
            {
                PlotBellhop(iphi, itheta, rayPositions);
            }
        }
    }

    List<Vector3> BellhopLine(int idx, int idy, float3[] bds) {

        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();

        int offset = GetStartIndexBellhop(idx, idy);

        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < bellhopParams.BELLHOPINTEGRATIONSTEPS; i++)
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
                if (distance < 1) // if points are too close, don't add the current point
                {
                    continue;
                }
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

    void PlotBellhop(int idx, int idy, float3[] bds)
    {
        // det kan eventuellt vara så att alla errors angående "invalid aabb" kan ha med att linjer ritas mellan alla punkter för en ray och vissa
        // punkter ligger väldigt nära varandra, kan vara värt att undersöka att ta bort punkter som ligger för nära föregående och se om det löser
        // problemet, för visualiseringens skulle borde det inte påverka något negativt eftersom de små små linjerna ändå inte går att se, plus att
        // det blir färre linjer vilket borde minska minnesanvädningen
        // Testade att bara lägga till punkter som har ett större avstånd mellan sig, blir fortfarande enormt många fel av oklar anledning, läst på om
        // felet "object is too large or too far away from the origin" och det var någon som sa att man inte ska ha object mer än 5000 enheter från origo,
        // men våra rays rör sig i detta fall max 300 enheter bort så det känns jätteskumt det som händer
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        
        int rayIdx = idy + sourceParams.ntheta * idx;

        if (sourceParams.showContributingRaysOnly && rayData[rayIdx].contributing != 1)
        {
            return;
        }

        List<Vector3> positions = BellhopLine(idx, idy, bds);   

        line = new GameObject("Line").AddComponent<LineRenderer>();
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;
        line.useWorldSpace = true;

        line.positionCount = positions.Count;
        line.SetPositions(positions.ToArray());

        lines.Add(line);        
    }

    void DestryBellhopLines()
    {
        foreach (LineRenderer line in lines) // delete lines from previous runs
        {
            Destroy(line.gameObject);
        }
        lines.Clear();
    }


    /// <summary>
    /// maintain data structures that are related to the gpu, update them if necessary and return the status of the operations
    /// </summary>
    /// <returns>bool</returns>
    bool MaintainBuffersArraysSSPAndWorld()
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
        World world = worldManager.GetComponent<World>();
        SimpleSourceController sourceController = sourceCamera.GetComponent<SimpleSourceController>();
        bool BuffersAndArraysSuccess = true;

        if (world.TargetHasChanged() || sourceController.HasMoved())
        {
            // either nr of targets has changed, positions of targets has changed or the source has moved, get the targets and recalculate their respective angles from the source            
            sourceController.AckMovement();
            oldNrOfTargets = world.GetNrOfTargets();
            targetBuffer = new ComputeBuffer(oldNrOfTargets, world.GetDataSizeOfTarget());
            targetBuffer.SetData(world.GetTargets(srcSphere.transform.position.x, srcSphere.transform.position.z).ToArray());
            computeShader.SetBuffer(0, "targetBuffer", targetBuffer);
        }

        if (sourceParams.HasChanged(oldSourceParams) || bellhopParams.HasChanged(oldBellhopParams) || world.TargetHasChanged()) // this could probably be written a bit nicer, some unecessary updates are done when a field is changed, but many of these are connected in some way
        {
            world.AckTargetChange();
            try
            {
                rayPositions = new float3[bellhopParams.BELLHOPINTEGRATIONSTEPS * world.GetNrOfTargets() * sourceParams.ntheta];
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
            oldBellhopParams = bellhopParams.ToStruct();

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

            computeShader.SetInt("_BELLHOPSIZE", bellhopParams.BELLHOPINTEGRATIONSTEPS);
            computeShader.SetFloat("deltas", bellhopParams.BELLHOPSTEPSIZE);

            dtheta = (float)sourceParams.theta / (float)(sourceParams.ntheta + 1); //TODO: Lista ut hur vinklar ska hanteras. Gör som i matlab, och sen lös det på nåt sätt
            dtheta = dtheta * MathF.PI / 180; // to radians
            computeShader.SetFloat("dtheta", dtheta);
            /*debugBuf = new ComputeBuffer(nrOfTargets * sourceParams.ntheta, 3 * sizeof(float));
            debugger = new float3[nrOfTargets * sourceParams.ntheta];
            computeShader.SetBuffer(0, "debugBuf", debugBuf);*/
        }
        if (bellhopParams.MAXNRSURFACEHITS != oldMaxSurfaceHits)
        {
            oldMaxSurfaceHits = bellhopParams.MAXNRSURFACEHITS;
            computeShader.SetInt("_MAXSURFACEHITS", bellhopParams.MAXNRSURFACEHITS);
        }
        if (bellhopParams.MAXNRBOTTOMHITS != oldMaxBottomHits)
        {
            oldMaxBottomHits = bellhopParams.MAXNRBOTTOMHITS;
            computeShader.SetInt("_MAXBOTTOMHITS", bellhopParams.MAXNRBOTTOMHITS);
        }

        bool SSPReadSuccessfully = SSPFileCheck();

        if (world.WorldHasChanged())
        {
            BuildWorld();
            world.AckChangeInWorld();
            rebuildRTAS = true;
        }

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
                SetComputeBuffer("_SSPBuffer", SSPBuffer);
                World world = worldManager.GetComponent<World>();
                world.SetNrOfWaterplanes(SSP.Count - 2);
                world.SetWaterDepth(SSP.Last().depth);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                Debug.Log("SSP not created successfully, raytracing won't be available until SSP is created successfully");
                success = false;
            }
        }
        return success;
    }

    void ComputeEigenRays()
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

    void TraceContributingRays()
    {
        PerContributingRayData = new PerRayData[contributingAngles.Count];

        ContributingAnglesBuffer = new ComputeBuffer(contributingAngles.Count, sizeof(float) * 2);

        ContributingAnglesBuffer.SetData(contributingAngles); // fill buffer of contributing angles/values
        computeShader.SetBuffer(1, "ContributingAnglesData", ContributingAnglesBuffer);

        // init return data buffer
        PerContributingRayDataBuffer = new ComputeBuffer(contributingAngles.Count, perraydataByteSize);
        computeShader.SetBuffer(1, "ContributingRayData", PerContributingRayDataBuffer);

        computeShader.SetBuffer(1, "_SSPBuffer", SSPBuffer);

        RayTargetsBuffer = new ComputeBuffer(contributingRays.Count, sizeof(uint));
        RayTargetsBuffer.SetData(rayTargets.ToArray());
        computeShader.SetBuffer(1, "rayTargets", RayTargetsBuffer);

        freqsdamps = new float2[1];
        freqsdamps[0].x = 150000;
        freqsdamps[0].y = 0.015f / 8.6858896f;

        FreqDampBuffer = new ComputeBuffer(freqsdamps.Length, sizeof(float) * 2);
        FreqDampBuffer.SetData(freqsdamps);
        computeShader.SetBuffer(1, "FreqsAndDampData", FreqDampBuffer);
        computeShader.SetInt("freqsdamps", freqsdamps.Length);

        computeShader.SetBuffer(1, "targetBuffer", targetBuffer);

        int threadGroupsX = Mathf.FloorToInt(1);
        int threadGroupsY = Mathf.FloorToInt(contributingAngles.Count);

        // send eigenrays
        computeShader.Dispatch(1, threadGroupsX, threadGroupsY, 1);

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
                DestryBellhopLines();
                Plot();
            }
        }

        if (Input.GetKey(KeyCode.C)){ // user input
            doRayTracing = true;            
        }
        else
        {
            //doRayTracing = false;
            lockRayTracing = false;
        }

        //
        // CHECK FOR UPDATES OVER //
        //

        if (((!lockRayTracing && doRayTracing) || sourceParams.sendRaysContinously) && errorFree) // do raytracing if the user has pressed key C. only do it once though. or do it continously
        {            
            rayPositionDataAvail = false;
            DestryBellhopLines();

            lockRayTracing = true; // disable raytracing being done several times during one keypress            

            DateTime time1 = DateTime.Now; // measure time to do raytracing

            CreateResources();

            if (rebuildRTAS)
            {
                BuildRTAS();
                rebuildRTAS = false;
            }

            SetShaderParameters();            
            
            int threadGroupsX = Mathf.FloorToInt(worldManager.GetComponent<World>().GetNrOfTargets());
            int threadGroupsY = Mathf.FloorToInt(sourceParams.ntheta / 8.0f);           

            //send rays
            computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            // read results from buffers into arrays
            RayPositionsBuffer.GetData(rayPositions);
            rayPositionDataAvail = true;
            PerRayDataBuffer.GetData(rayData);

            // keep contributing rays only            
            for (int i = 0; i < rayData.Length; i++)
            {
                if (rayData[i].contributing == 1)
                {
                    contributingRays.Add(rayData[i]);
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

                        rays.Add(BellhopLine(iphi, itheta, rayPositions));

                    }
                }

                api.Rays(rays);
                // Debug.Log(rays[0][0].ToString());
            }

            // check if a pair of contributing rays can be combined into an eigenray
            ComputeEigenRays();

            if (contributingAngles.Count > 0)
            {
                TraceContributingRays();               
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
            DestryBellhopLines();
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

                    rays.Add(BellhopLine(iphi, itheta, rayPositions));

                }
            }

            api.Rays(rays);
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