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
    private int oldBellhopIterations = 0;

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

    /*struct RayData
    {
        public Vector3 origin;
        public int set;
    };*/
    //private int raydatabytesize = 16; // update this if the struct RayData is modified

    struct PerRayData
    {
        public uint iseig;
        public float beta;
        public uint ntop;
        public uint nbot;
        public uint ncaust;
        public float delay;
        public float curve;
        public float xn;
        public float qi;
        public float alpha;
        public uint contributing;
    }
    private int perraydataByteSize = sizeof(uint) * 5 + sizeof(float) * 6;

    private ComputeBuffer PerRayDataBuffer;
    private PerRayData[] rayData = null;
    private float dtheta = 0;
    private List<PerRayData> contributingRays = new List<PerRayData>();
    private bool[] isEigen = null;
    private ComputeBuffer alphaData;
    /*private float[] alphas = new float[128] { (float)-0.523598775598299, (float)-0.515353125588877, (float)-0.507107475579455, (float)-0.498861825570033, (float)-0.490616175560611, (float)-0.482370525551189, (float)-0.474124875541767,
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
                                            (float)0.507107475579455, (float)0.515353125588877, (float)0.523598775598299 };*/

    private float[] alphas = new float[128] { -0.52359879f, -0.51535314f, -0.50710750f, -0.49886185f, -0.49061620f, -0.48237056f, -0.47412491f, -0.46587926f, -0.45763358f, -0.44938794f, -0.44114229f,
                                            -0.43289664f, -0.42465100f, -0.41640535f, -0.40815970f, -0.39991406f, -0.39166838f, -0.38342273f, -0.37517709f, -0.36693144f, -0.35868579f, -0.35044014f,
                                            -0.34219450f, -0.33394885f, -0.32570317f, -0.31745753f, -0.30921188f, -0.30096623f, -0.29272059f, -0.28447494f, -0.27622926f, -0.26798365f, -0.25973800f,
                                            -0.25149235f, -0.24324667f, -0.23500103f, -0.22675538f, -0.21850973f, -0.21026407f, -0.20201842f, -0.19377278f, -0.18552713f, -0.17728147f, -0.16903582f,
        -0.16079018f, -0.15254453f, -0.14429887f, -0.13605322f, -0.12780759f, -0.11956193f, -0.11131628f, -0.10307062f, -0.094824977f, -0.086579323f, -0.078333676f, -0.070088021f, -0.061842378f, -0.053596724f,
        -0.045351077f, -0.037105422f, -0.028859776f, -0.020614125f, -0.012368475f, -0.0041228253f, 0.0041228253f, 0.012368475f, 0.020614125f, 0.028859776f, 0.037105422f, 0.045351077f, 0.053596724f, 0.061842378f,
        0.070088021f, 0.078333676f, 0.086579323f, 0.094824977f, 0.10307062f, 0.11131628f, 0.11956193f, 0.12780759f, 0.13605322f, 0.14429887f, 0.15254453f, 0.16079018f, 0.16903582f, 0.17728147f, 0.18552713f,
        0.19377278f, 0.20201842f, 0.21026407f, 0.21850973f, 0.22675538f, 0.23500103f, 0.24324667f, 0.25149235f, 0.25973800f, 0.26798365f, 0.27622926f, 0.28447494f, 0.29272059f, 0.30096623f, 0.30921188f,
        0.31745753f, 0.32570317f, 0.33394885f, 0.34219450f, 0.35044014f, 0.35868579f, 0.36693144f, 0.37517709f, 0.38342273f, 0.39166838f, 0.39991406f, 0.40815970f, 0.41640535f, 0.42465100f, 0.43289664f,
        0.44114229f, 0.44938794f, 0.45763358f, 0.46587926f, 0.47412491f, 0.48237056f, 0.49061620f, 0.49886185f, 0.50710750f, 0.51535314f, 0.52359879f };

    

    private ComputeBuffer debugBuf;
    private float3[] debugger;


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
        PerRayDataBuffer?.Release();
        alphaData?.Release();
        debugBuf?.Release();
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

        if (bds == null)
        {
            bds = new float3[bellhopParams.BELLHOPINTEGRATIONSTEPS * sourceParams.nphi * sourceParams.ntheta];
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

        /*if (rds == null)
        {
            rds = new RayData[sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS];
            Debug.Log(rds.Length);
        }*/

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

        computeShader.SetVector("srcDirection", srcSphere.transform.forward);
        Debug.Log(srcSphere.transform.forward);
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

        alphaData = new ComputeBuffer(128, sizeof(float));
        alphaData.SetData(alphas);

        //foreach(float item in alphas)
        //{
        //    Debug.Log(item);
        //}
    }

    int GetStartIndexBellhop(int idx, int idy)
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();

        return (idy * sourceParams.nphi + idx) * bellhopParams.BELLHOPINTEGRATIONSTEPS;
    }

    void PlotBellhop(int idx, int idy)
    {        
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();

        int offset = GetStartIndexBellhop(idx, idy);
        int offset2 = idy * sourceParams.nphi + idx;

        //Debug.Log(offset2);
        //Debug.Log(rayData[offset2].iseig.ToString());
        //Debug.Log(rayData[offset2].beta.ToString());
        //Debug.Log(rayData[offset2].ntop.ToString());
        //Debug.Log(rayData[offset2].nbot.ToString());
        //Debug.Log(rayData[offset2].ncaust.ToString());
        //Debug.Log(rayData[offset2].delay.ToString());
        //Debug.Log(rayData[offset2].curve.ToString());
        //Debug.Log(rayData[offset2].xn.ToString());
        //Debug.Log(rayData[offset2].qi.ToString());
        //Debug.Log(rayData[offset2].alpha.ToString());
        //Debug.Log("-----------------------------------------------");


        //if (rayData[offset2].iseig == 0)
        //{
        //    return;
        //}

        if (rayData[offset2].contributing == 0)
        {
            return;
        }

        //Debug.Log("x: " + idx);

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
        //Debug.Log(sourceParams.nphi);
        //Debug.Log(sourceParams.ntheta);

        if (sourceParams.HasChanged(oldSourceParams) || bellhopParams.HasChanged(oldBellhopParams))
        {
            Debug.Log("Reeinit raybuffer");
            // reinit rds array
            //rds = new RayData[sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS];

            // reinit raydatabuffer
            /*if (_rayPointsBuffer != null)
            {
                _rayPointsBuffer.Release();
            }
            _rayPointsBuffer = new ComputeBuffer(sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS, raydatabytesize);*/

            bds = new float3[bellhopParams.BELLHOPINTEGRATIONSTEPS * sourceParams.nphi * sourceParams.ntheta];
            rayData = new PerRayData[sourceParams.nphi * sourceParams.ntheta];
            oldSourceParams = sourceParams.ToStruct();
            oldBellhopParams = bellhopParams.ToStruct();

            if (xrayBuf != null)
            {
                xrayBuf.Release();
                xrayBuf = null;
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

            computeShader.SetBuffer(0, "alphaData", alphaData);

            dtheta = (float)sourceParams.theta / (float)(sourceParams.ntheta -1);
            Debug.Log(dtheta);
            dtheta = dtheta * MathF.PI / 180; // to radians
            Debug.Log(dtheta);
            computeShader.SetFloat("dalpha", dtheta);
            debugBuf = new ComputeBuffer(bellhopParams.BELLHOPINTEGRATIONSTEPS * sourceParams.nphi * sourceParams.ntheta, 3 * sizeof(float));
            debugger = new float3[bellhopParams.BELLHOPINTEGRATIONSTEPS * sourceParams.nphi * sourceParams.ntheta];
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
        if(bellhopParams.BELLHOPITERATIONS != oldBellhopIterations)
        {
            oldBellhopIterations = bellhopParams.BELLHOPITERATIONS;
            computeShader.SetInt("_BELLHOPITERATIONS", bellhopParams.BELLHOPITERATIONS);            
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

            //int threadGroupsX = Mathf.FloorToInt(sourceParams.nphi / 8.0f);
            int threadGroupsX = Mathf.FloorToInt(1);
            int threadGroupsY = Mathf.FloorToInt(sourceParams.ntheta / 8.0f);           

            computeShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);     

            // read results from buffers into arrays
            xrayBuf.GetData(bds);
            PerRayDataBuffer.GetData(rayData);
            debugBuf.GetData(debugger);

            Debug.Log("--------------------------------------------------------------------------");
            for (int i = 38* bellhopParams.BELLHOPINTEGRATIONSTEPS; i < 39*bellhopParams.BELLHOPINTEGRATIONSTEPS; i++) //index 38 borde vara första strålen som bidrar
            {
                Debug.Log(i - 38 * bellhopParams.BELLHOPINTEGRATIONSTEPS + ": " + debugger[i].x + " " + debugger[i].y + " " + debugger[i].z);
            }
            Debug.Log("--------------------------------------------------------------------------");
            Debug.Log(rayData[0].alpha);
            Debug.Log(rayData[0].beta);

            int steplength = sourceParams.nphi;

            // keep contributing rays only
            for (int i = sourceParams.nphi/2; i < rayData.Length; i+=steplength)
            {
                if (rayData[i].beta < 1)
                {
                    contributingRays.Add(rayData[i]);
                }
            }

            Debug.Log("Contributing rays: " + contributingRays.Count);
            Debug.Log(rayData.Length);

            isEigen = new bool[contributingRays.Count];

            /*for (int i = 0; i < contributingRays.Count; i++)
            {
                Debug.Log(contributingRays[i].alpha.ToString());
            }*/

            Debug.Log("------------------------------------------------------------------------------------------------");

            for (int i = 0; i < contributingRays.Count; i++)
            {
                //Debug.Log(rayData[i].iseig.ToString());
                Debug.Log("Ray: " + i);
                Debug.Log("alpha: " + contributingRays[i].alpha.ToString());
                Debug.Log("Beta: " + contributingRays[i].beta.ToString());
                Debug.Log("ntop: " + contributingRays[i].ntop.ToString());
                Debug.Log("nbot: " + contributingRays[i].nbot.ToString());
                Debug.Log("ncaust: " + contributingRays[i].ncaust.ToString());
                Debug.Log("delay: " + contributingRays[i].delay.ToString());
                Debug.Log("curve: " + contributingRays[i].curve.ToString());
                Debug.Log("xn: " + contributingRays[i].xn.ToString());
                Debug.Log("qi: " + contributingRays[i].qi.ToString());
                Debug.Log("------------------------------------------------------------------------------------------------");
            }




            //Debug.Log(rayData[0].iseig.ToString());
            //Debug.Log(rayData[0].beta.ToString());
            //Debug.Log(rayData[0].ntop.ToString());
            //Debug.Log(rayData[0].nbot.ToString());
            //Debug.Log(rayData[0].ncaust.ToString());
            //Debug.Log(rayData[0].delay.ToString());
            //Debug.Log(rayData[0].curve.ToString());
            //Debug.Log(rayData[0].xn.ToString());
            //Debug.Log(rayData[0].qi.ToString());
            //Debug.Log(rayData[0].alpha.ToString());



            // compute eigenrays, detta blir för nuvarande jättefel, ray0 (rakt fram, mest neråt) är på plats '4' (för 128 ntheta, 8 nphi) och nästa ray kommer vara på plats 12, därav mpste det ändras här
            /*int i = 0;
            while (i < rayData.Length - steplength)
            {
                // find pairs of rays
                if (rayData[i + steplength].xn < rayData[i].xn + 1.5 * dtheta)
                {
                    float n1 = rayData[i].xn;
                    float n2 = rayData[i+steplength].xn;
                    float tot = rayData[i].beta + rayData[i + steplength].beta;

                    if (n1 * n2 <= 0 && tot > 0.9 && tot < 1.1)
                    {
                        float w = n2 / (n2 - n1);
                        rayData[i].iseig = 1; //true
                        rayData[i].alpha = w * rayData[i].alpha + (1 - w) * rayData[i + steplength].alpha;
                        rayData[i + steplength].iseig = 0; //false
                        i++;
                        Debug.Log("kjdkdjkldjlkdd");
                    }
                }
                i++;
            }*/

            // här borde man gå igenom listan istället
            for (int i = 0; i < contributingRays.Count - 1; i++)
            {
                // find pairs of rays
                if (contributingRays[i + 1].alpha < contributingRays[i].alpha + 1.5 * dtheta)
                {
                    float n1 = contributingRays[i].xn;
                    float n2 = contributingRays[i + 1].xn;
                    float tot = contributingRays[i].beta + contributingRays[i + 1].beta;
                    Debug.Log("hejhej " + i);
                    Debug.Log("jhkjfhkjfh");
                    Debug.Log(n1 + " " + n2);
                    Debug.Log(tot);

                    if (n1 * n2 <= 0 && tot > 0.9 && tot < 1.1)
                    {
                        isEigen[i] = true;
                        //isEigen[i + 1] = false;


                        float w = n2 / (n2 - n1);
                        //contributingRays[i].iseig = 1; //true
                        //contributingRays[i].alpha = w * contributingRays[i].alpha + (1 - w) * contributingRays[i + 1].alpha;
                        //contributingRays[i + 1].iseig = 0; //false
                        i++;
                        Debug.Log("kjdkdjkldjlkdd");
                    }
                }
            }

            if (sourceParams.visualizeRays)
            {
                for (int itheta = 0; itheta < sourceParams.ntheta; itheta++) {
                    //Debug.Log("nphi: " + sourceParams.nphi/2);
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
        contributingRays.Clear();
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