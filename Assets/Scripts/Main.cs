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
    private int oldMaxSurfaceHits = 0;
    private int oldMaxBottomHits = 0;    

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

    private ComputeBuffer rayPositionsBuffer;
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
    }
    private int perraydataByteSize = sizeof(uint) * 4 + sizeof(float) * 7;

    struct PerRayData2
    {
        public PerRayData prd;
        public bool isEig;
    }

    private ComputeBuffer PerRayDataBuffer;
    private PerRayData[] rayData = null;
    private float dtheta = 0;
    private List<PerRayData> contributingRays = new List<PerRayData>();
    private List<PerRayData2> contributingRays2 = new List<PerRayData2>();
    //private bool[] isEigen = null;
    //private ComputeBuffer alphaData;

    /*private float[] alphas = new float[128] { -0.52359879f, -0.51535314f, -0.50710750f, -0.49886185f, -0.49061620f, -0.48237056f, -0.47412491f, -0.46587926f, -0.45763358f, -0.44938794f, -0.44114229f,
                                            -0.43289664f, -0.42465100f, -0.41640535f, -0.40815970f, -0.39991406f, -0.39166838f, -0.38342273f, -0.37517709f, -0.36693144f, -0.35868579f, -0.35044014f,
                                            -0.34219450f, -0.33394885f, -0.32570317f, -0.31745753f, -0.30921188f, -0.30096623f, -0.29272059f, -0.28447494f, -0.27622926f, -0.26798365f, -0.25973800f,
                                            -0.25149235f, -0.24324667f, -0.23500103f, -0.22675538f, -0.21850973f, -0.21026407f, -0.20201842f, -0.19377278f, -0.18552713f, -0.17728147f, -0.16903582f,
        -0.16079018f, -0.15254453f, -0.14429887f, -0.13605322f, -0.12780759f, -0.11956193f, -0.11131628f, -0.10307062f, -0.094824977f, -0.086579323f, -0.078333676f, -0.070088021f, -0.061842378f, -0.053596724f,
        -0.045351077f, -0.037105422f, -0.028859776f, -0.020614125f, -0.012368475f, -0.0041228253f, 0.0041228253f, 0.012368475f, 0.020614125f, 0.028859776f, 0.037105422f, 0.045351077f, 0.053596724f, 0.061842378f,
        0.070088021f, 0.078333676f, 0.086579323f, 0.094824977f, 0.10307062f, 0.11131628f, 0.11956193f, 0.12780759f, 0.13605322f, 0.14429887f, 0.15254453f, 0.16079018f, 0.16903582f, 0.17728147f, 0.18552713f,
        0.19377278f, 0.20201842f, 0.21026407f, 0.21850973f, 0.22675538f, 0.23500103f, 0.24324667f, 0.25149235f, 0.25973800f, 0.26798365f, 0.27622926f, 0.28447494f, 0.29272059f, 0.30096623f, 0.30921188f,
        0.31745753f, 0.32570317f, 0.33394885f, 0.34219450f, 0.35044014f, 0.35868579f, 0.36693144f, 0.37517709f, 0.38342273f, 0.39166838f, 0.39991406f, 0.40815970f, 0.41640535f, 0.42465100f, 0.43289664f,
        0.44114229f, 0.44938794f, 0.45763358f, 0.46587926f, 0.47412491f, 0.48237056f, 0.49061620f, 0.49886185f, 0.50710750f, 0.51535314f, 0.52359879f };*/

    private ComputeBuffer eigenAnglesBuffer;
    private float2[] eigenangles = null;
    private ComputeBuffer PerEigenRayDataBuffer;
    private PerRayData[] PerEigenRayData = null;
    
    private ComputeBuffer debugBuf;
    private float3[] debugger;
    
    //TODO: troligtvis kommer koden för hur eigenrays räknas ut behöva skrivas om lite när man skickar rays i fler phi-vinklar, fundera över det, (kom ihåg att se över shader-kod som just nu bara skickar
    //rays i en phi-riktning.

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
        
        _SSPBuffer?.Release();
        rayPositionsBuffer?.Release();
        PerRayDataBuffer?.Release();
        //alphaData?.Release();
        //debugBuf?.Release();
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

        if (rayPositionsBuffer == null)
        {
            rayPositionsBuffer = new ComputeBuffer(bellhopParams.BELLHOPINTEGRATIONSTEPS * sourceParams.nphi * sourceParams.ntheta, 3 * sizeof(float));
            SetComputeBuffer("rayPositionsBuffer", rayPositionsBuffer);
        }

        if (rayPositions == null)
        {
            rayPositions = new float3[bellhopParams.BELLHOPINTEGRATIONSTEPS * sourceParams.nphi * sourceParams.ntheta];
            rayPositionDataAvail = false;
        }

        if (PerRayDataBuffer == null)
        {
            PerRayDataBuffer = new ComputeBuffer(sourceParams.nphi * sourceParams.ntheta, perraydataByteSize);
            SetComputeBuffer("PerRayDataBuffer", PerRayDataBuffer);
        }

        if (rayData == null)
        {
            rayData = new PerRayData[sourceParams.nphi * sourceParams.ntheta];
        }

        if (surfaceInstanceData == null)
        {            
            surfaceInstanceData = new SurfaceAndSeafloorInstanceData();
        }        
        if (seafloorInstanceData == null)
        {            
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

        //rtas = new RayTracingAccelerationStructure();
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

        /*alphaData = new ComputeBuffer(128, sizeof(float));
        alphaData.SetData(alphas);*/
    }

    int GetStartIndexBellhop(int idx, int idy)
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();

        return (idy * sourceParams.nphi + idx) * bellhopParams.BELLHOPINTEGRATIONSTEPS;
    }

    void PlotBellhop(int idx, int idy)
    {
        // det kan eventuellt vara så att alla errors angående "invalid aabb" kan ha med att linjer ritas mellan alla punkter för en ray och vissa
        // punkter ligger väldigt nära varandra, kan vara värt att undersöka att ta bort punkter som ligger för nära föregående och se om det löser
        // problemet, för visualiseringens skulle borde det inte påverka något negativt eftersom de små små linjerna ändå inte går att se, plus att
        // det blir färre linjer vilket borde minska minnesanvädningen
        // Testade att bara lägga till punkter som har ett större avstånd mellan sig, blir fortfarande enormt många fel av oklar anledning, läst på om
        // felet "object is too large or too far away from the origin" och det var någon som sa att man inte ska ha object mer än 5000 enheter från origo,
        // men våra rays rör sig i detta fall max 300 enheter bort så det känns jätteskumt det som händer
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        
        int rayIdx = idy * sourceParams.nphi + idx;

        if (sourceParams.showContributingRaysOnly && rayData[rayIdx].contributing != 1)
        {
            return;
        }

        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();

        int offset = GetStartIndexBellhop(idx, idy);

        line = new GameObject("Line").AddComponent<LineRenderer>();
        line.startWidth = 0.03f;
        line.endWidth = 0.03f;
        line.useWorldSpace = true;

        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < bellhopParams.BELLHOPINTEGRATIONSTEPS; i++)
        {            
            /*if (i == 0) // add the first position
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
            }*/            
            
            if (rayPositions[offset + i].y <= 0f )
            {
                positions.Add(new Vector3(rayPositions[offset + i].x, rayPositions[offset + i].y, rayPositions[offset + i].z));
            }
            else
            {
                break; // faulty position, break loop
            }
        }

        line.positionCount = positions.Count;
        line.SetPositions(positions.ToArray());

        lines.Add(line);        
    }

    void Update()
    {        
        //
        // CHECK FOR UPDATES
        //
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();        

        if (sourceParams.HasChanged(oldSourceParams) || bellhopParams.HasChanged(oldBellhopParams))
        {
            rayPositions = new float3[bellhopParams.BELLHOPINTEGRATIONSTEPS * sourceParams.nphi * sourceParams.ntheta];
            rayPositionDataAvail = false;
            rayData = new PerRayData[sourceParams.nphi * sourceParams.ntheta];
            oldSourceParams = sourceParams.ToStruct();
            oldBellhopParams = bellhopParams.ToStruct();

            if (rayPositionsBuffer != null)
            {
                rayPositionsBuffer.Release();
                rayPositionsBuffer = null;
            }

            if (PerRayDataBuffer != null)
            {
                PerRayDataBuffer.Release();
                PerRayDataBuffer = null;
            }

            // update values in shader
            computeShader.SetInt("theta", sourceParams.theta);
            computeShader.SetInt("ntheta", sourceParams.ntheta);
            computeShader.SetInt("phi", sourceParams.phi);
            computeShader.SetInt("nphi", sourceParams.nphi);

            computeShader.SetInt("_BELLHOPSIZE", bellhopParams.BELLHOPINTEGRATIONSTEPS);
            computeShader.SetFloat("deltas", bellhopParams.BELLHOPSTEPSIZE);

            //computeShader.SetBuffer(0, "alphaData", alphaData);

            dtheta = (float)sourceParams.theta / (float)(sourceParams.ntheta + 1);            
            dtheta = dtheta * MathF.PI / 180; // to radians
            computeShader.SetFloat("dalpha", dtheta);
            debugBuf = new ComputeBuffer(sourceParams.nphi * sourceParams.ntheta, 3 * sizeof(float));
            debugger = new float3[sourceParams.nphi * sourceParams.ntheta];
            computeShader.SetBuffer(0, "debugBuf", debugBuf);
        }
        if(bellhopParams.MAXNRSURFACEHITS != oldMaxSurfaceHits)
        {
            oldMaxSurfaceHits = bellhopParams.MAXNRSURFACEHITS;
            computeShader.SetInt("_MAXSURFACEHITS", bellhopParams.MAXNRSURFACEHITS);            
        }
        if (bellhopParams.MAXNRBOTTOMHITS != oldMaxBottomHits)
        {
            oldMaxBottomHits = bellhopParams.MAXNRBOTTOMHITS;
            computeShader.SetInt("_MAXBOTTOMHITS", bellhopParams.MAXNRBOTTOMHITS);            
        }
        if (oldVisualiseRays != sourceParams.visualizeRays || oldVisualiseContributingRays != sourceParams.showContributingRaysOnly)
        {
            oldVisualiseRays = sourceParams.visualizeRays;
            oldVisualiseContributingRays = sourceParams.showContributingRaysOnly;

            if (rayPositionDataAvail && sourceParams.visualizeRays)
            {
                Debug.Log("ray data available, plot rays");
                foreach (LineRenderer line in lines)
                {
                    Destroy(line.gameObject);
                }
                lines.Clear();

                for (int itheta = 0; itheta < sourceParams.ntheta; itheta++)
                {                    
                    PlotBellhop((int)sourceParams.nphi / 2, itheta);
                }
            }
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
            DateTime time1 = DateTime.Now; // measure time to do raytracing
            rayPositionDataAvail = false;
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

            //int threadGroupsX = Mathf.FloorToInt(sourceParams.nphi / 8.0f);
            int threadGroupsX = Mathf.FloorToInt(sourceParams.nphi);
            //int threadGroupsY = Mathf.FloorToInt(1);
            int threadGroupsY = Mathf.FloorToInt(sourceParams.ntheta / 8.0f);           

            //send rays
            computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            // read results from buffers into arrays
            rayPositionsBuffer.GetData(rayPositions);
            rayPositionDataAvail = true;
            PerRayDataBuffer.GetData(rayData);
            debugBuf.GetData(debugger);

            /*for (int i = 0; i < debugger.Length; i++)
            {
                Debug.Log("phi " + debugger[i].x + ", alpha " + debugger[i].y + ", 1234 " + debugger[i].z);
            } */           

            //int steplength = sourceParams.nphi;

            // keep contributing rays only
            //for (int i = sourceParams.nphi / 2; i < rayData.Length; i += steplength)
            for (int i = 0; i < rayData.Length; i++)
            {
                if (rayData[i].contributing == 1)
                {
                    contributingRays.Add(rayData[i]);
                }
            }

            Debug.Log(contributingRays.Count);
            

            // compute eigenrays
            for (int i = 0; i < contributingRays.Count - 1; i++) // nu blir den här fucked eftersom rays kan ligga blandat
            {
                Debug.Log(i+": " + contributingRays[i].phi);
                // find pairs of rays
                if (contributingRays[i + 1].theta < contributingRays[i].theta + 1.5 * dtheta && contributingRays[i].phi == contributingRays[i+1].phi)
                {
                    float n1 = contributingRays[i].xn;
                    float n2 = contributingRays[i + 1].xn;
                    float tot = contributingRays[i].beta + contributingRays[i + 1].beta;

                    if (n1 * n2 <= 0 && tot > 0.9 && tot < 1.1)
                    {
                        float w = n2 / (n2 - n1);
                        // create eigenray from the two rays
                        PerRayData eigenray;
                        eigenray.contributing = 1;
                        eigenray.ntop = contributingRays[i].ntop;
                        eigenray.nbot = contributingRays[i].nbot;
                        eigenray.ncaust = contributingRays[i].ncaust;
                        eigenray.curve = contributingRays[i].curve;
                        eigenray.delay = contributingRays[i].delay;
                        eigenray.qi = contributingRays[i].qi;
                        eigenray.xn = contributingRays[i].xn;
                        eigenray.beta = contributingRays[i].beta;
                        eigenray.theta = w * contributingRays[i].theta + (1 - w) * contributingRays[i + 1].theta;
                        eigenray.phi = contributingRays[i].phi;

                        PerRayData2 eigRay;
                        eigRay.prd = eigenray;
                        eigRay.isEig = true;


                        contributingRays2.Add(eigRay);

                        i++;
                    }
                    else
                    {
                        PerRayData2 notEigRay;
                        notEigRay.prd = contributingRays[i];
                        notEigRay.isEig = false;
                        contributingRays2.Add(notEigRay);
                    }
                }
            }

            if (contributingRays2.Count > 0)
            {
                // trace the eigenrays
                PerEigenRayData = new PerRayData[contributingRays2.Count];

                eigenangles = new float2[contributingRays2.Count]; // ändra det här sen till nåt bättre
                for (int i = 0; i < contributingRays2.Count; i++)
                {
                    eigenangles[i].x = contributingRays2[i].prd.theta;
                    eigenangles[i].y = contributingRays2[i].prd.phi;
                }

                eigenAnglesBuffer = new ComputeBuffer(contributingRays2.Count, sizeof(float)*2);

                eigenAnglesBuffer.SetData(eigenangles); // fill buffer of alpha values
                computeShader.SetBuffer(1, "eigenAnglesData", eigenAnglesBuffer);

                // init return data buffer
                PerEigenRayDataBuffer = new ComputeBuffer(contributingRays2.Count, perraydataByteSize);
                computeShader.SetBuffer(1, "EigenRayData", PerEigenRayDataBuffer);

                computeShader.SetBuffer(1, "_SSPBuffer", _SSPBuffer);

                threadGroupsX = Mathf.FloorToInt(1);
                threadGroupsY = Mathf.FloorToInt(contributingRays2.Count);

                // send eigenrays
                computeShader.Dispatch(1, threadGroupsX, threadGroupsY, 1);

                PerEigenRayDataBuffer.GetData(PerEigenRayData);

               // compute transmission loss

                Debug.Log("    theta     phi     T   B   C         TL          dist         delay     beta     eig");

                float[] freqs = new float[1] { 150000 };
                float[] damp = new float[1] { 0.015f / 8.6858896f };
                // bottom properties
                float cp = 1600; // m/s
                float rho = 1.8f; // rho/rho0
                float bottom_alpha = 0.025f; // dB/m

                // sound speed: source, receiver            
                float cs = LayerSpeed(SSP, sourceCamera.transform.position.y, 0);
                float cr = LayerSpeed(SSP, targetSphere.transform.position.y, 0);
                float cwater = LayerSpeed(SSP, world.GetWaterDepth(), 0);

                float[,] Amp = new float[contributingRays2.Count, freqs.Length];
                float[,] Phase = new float[contributingRays2.Count, freqs.Length];

                float xdiff = targetSphere.transform.position.x - sourceCamera.transform.position.x;
                float zdiff = targetSphere.transform.position.z - sourceCamera.transform.position.z;

                float targetSphereR = MathF.Sqrt(MathF.Pow(xdiff, 2) + MathF.Pow(zdiff, 2));

                for (int i = 0; i < PerEigenRayData.Length; i++)
                {
                    // amplitudes
                    float Arms = 0;
                    float Amp0 = Mathf.Sqrt(Mathf.Cos(PerEigenRayData[i].theta) * cr / MathF.Abs(PerEigenRayData[i].qi) / targetSphereR);

                    // ray tangebt in r-direction
                    float Tg = Mathf.Cos(PerEigenRayData[i].theta) / cs;

                    for (int j = 0; j < freqs.Length; j++)
                    {
                        float Rfa, gamma;
                        // bottom reflection coefficient
                        if (PerEigenRayData[i].nbot > 0)
                        {
                            float omega = 2 * MathF.PI * freqs[j];
                            float2 RfaGamma = bottom_reflection(cwater, cp, rho, bottom_alpha, Tg, omega, damp[j]);
                            Rfa = RfaGamma.x;
                            gamma = RfaGamma.y;
                        }
                        else
                        {
                            Rfa = 1;
                            gamma = 0;
                        }

                        // amplitude and phase
                        Amp[i, j] = Amp0 * MathF.Pow(Rfa, PerEigenRayData[i].nbot) * MathF.Exp(-damp[j] * PerEigenRayData[i].curve);
                        gamma = MathF.PI * PerEigenRayData[i].ntop + gamma * PerEigenRayData[i].nbot + MathF.PI / 2 * PerEigenRayData[i].ncaust;
                        Phase[i, j] = (gamma + MathF.PI) % (2 * MathF.PI) - MathF.PI;

                        // RMS amplitude
                        Arms += MathF.Pow(Amp[i, j], 2);

                        // weighted amplitude
                        if (!contributingRays2[i].isEig)
                        {
                            Amp[i, j] *= (1 - PerEigenRayData[i].beta);
                        }
                    }

                    // transmission loss
                    rho = 1;
                    float I1 = 1 / cs / rho;
                    float I2 = (Arms / freqs.Length) / cr / rho;
                    float TL = 10 * MathF.Log10(I1 / I2);

                    float theta_deg = PerEigenRayData[i].theta * 180 / MathF.PI;
                    float phi_deg = PerEigenRayData[i].phi * 180 / MathF.PI;

                    string data = theta_deg.ToString("F6") + " " + phi_deg.ToString("F6") + " " + PerEigenRayData[i].ntop + " " + PerEigenRayData[i].nbot + " " + PerEigenRayData[i].ncaust + " " +
                                    TL.ToString("F6") + " " + PerEigenRayData[i].curve.ToString("F6") + " " + PerEigenRayData[i].delay.ToString("F6") + " " + PerEigenRayData[i].beta.ToString("F6") +
                                    " " + contributingRays2[i].isEig;


                    Debug.Log(data); // blir inte jättesnyggt, men det är samma resultat som matlab iallafall
                }
            }       

            DateTime time2 = DateTime.Now;

            TimeSpan ts = time2 - time1;            

            Debug.Log("Time elapsed: " + ts.TotalMilliseconds + "ms");

            if (sourceParams.visualizeRays)
            {
                for (int itheta = 0; itheta < sourceParams.ntheta; itheta++) {                    
                    PlotBellhop((int)sourceParams.nphi/2, itheta);
                }                
            }            

            /*PerEigenRayDataBuffer.Dispose();
            eigenAlphaBuffer.Dispose();*/
        }

        if (!sourceParams.visualizeRays)
        {
            foreach (LineRenderer line in lines)
            {
                Destroy(line.gameObject);
            }
            lines.Clear();
        }
        contributingRays.Clear();
        contributingRays2.Clear();
    }

    float2 bottom_reflection(float cwater, float cp, float rho, float bottom_alpha, float Tg, float omega, float alphaT)
    {
        // imaginary part of cp
        float alpha = bottom_alpha / 8.6858896f + alphaT;
        float ci = alpha * MathF.Pow(cp, 2) / omega;

        float g1 = MathF.Pow(Tg, 2) - 1 / MathF.Pow(cwater, 2);
        float h1 = 0;

        float cp2 = cp * cp + ci * ci;
        float x = cp / cp2;
        float y = -ci / cp2;
        float g2 = MathF.Pow(Tg, 2) - x * x + y * y;
        float h2 = -2 * x * y;

        float2 g1h1 = complexsqrt(g1, h1);
        float2 g2h2 = complexsqrt(g2, h2);

        float A = rho * g1h1.x - g2h2.x;
        float B = rho * g1h1.y - g2h2.y;
        float C = rho * g1h1.x + g2h2.x;
        float D = rho * g1h1.y + g2h2.y;
        float R = (A * C + B * D) / (C * C + D * D);
        float Q = (B * C - A * D) / (C * C + D * D);

        float Rfa = MathF.Sqrt(R * R + Q * Q);
        float gamma = MathF.Atan2(Q, R);

        return new float2(Rfa, gamma);
    }

    float2 complexsqrt(float a, float b)
    {
        float x, y;
        if (a >= 0)
        {
            x = MathF.Sqrt((MathF.Sqrt(a * a + b * b) + a) / 2);
            if (x > 0)
            {
                y = b / x / 2;
            }
            else
            {
                y = 0;
            }
        }
        else
        {
            y = MathF.Sqrt((MathF.Sqrt(a * a + b * b) - a) / 2);
            if (b < 0)
            {
                y = -y;
            }
            x = b / y / 2;
        }

        return new float2(x, y);
    }

    float LayerSpeed(List<SSPFileReader.SSP_Data> SSP, float depth, int Layer=0)
    {        
        while(Layer < SSP.Count - 1 && depth <= SSP[Layer + 1].depth)
        {
            Layer += 1;
        }
        while (depth > SSP[Layer].depth && Layer > 0)
        {
            Layer -= 1;
        }

        float w = depth - SSP[Layer].depth;

        // linear interpolation for now
        float c = SSP[Layer].velocity + w * SSP[Layer].derivative1;

        return c;
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


// tanke: skicka alla rays, lägg till intressanta rays i lista som det görs nu, spara både theta och phi vinklar för att veta vilken riktning stråle gick. då bör koden som redan är
//      skriven för transmission loss funka som tidigare utan problem, sen behöver man lägga till ett extra fält i printen.
//      fortfarande oklart hur shader-koden ska hantera saker
//      eventuellt kan man kanske byta på hur buffern fylls alltså att närliggande rays i buffern skiljer sig i theta-led (förutom edge cases då)
// tanke: eventuellt kanske man ska skapa en ny buffer som har redan på positionerna för rays i den andra sändningen för att då kunna toggla mellan att rita alla rays och eigenrays,
//          man byter då vilken buffer/array man ritar ifrån