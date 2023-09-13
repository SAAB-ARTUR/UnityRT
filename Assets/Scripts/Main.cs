using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Main : MonoBehaviour
{    
    public ComputeShader computeShaderTest = null;

    [SerializeField] GameObject srcSphere = null;
    [SerializeField] GameObject targetSphere = null;
    [SerializeField] GameObject surfaceCombo = null;
    [SerializeField] GameObject bottomCombo = null;
    [SerializeField] GameObject waterLayerCombo = null;
    [SerializeField] Camera secondCamera = null;
    [SerializeField] bool sendRaysContinuosly = false;
    [SerializeField] bool visualizeRays = false;
    [SerializeField] float lineLength = 1;
    [SerializeField] Material lineMaterial = null;
    [SerializeField] GameObject world_manager = null;
    

    private SourceParams.Properties? oldSourceParams = null;

    uint cameraWidth = 0;
    uint cameraHeight = 0;

    private List<GameObject> waterplanes = new List<GameObject>();
    public static bool _meshObjectsNeedRebuilding = false;
    private static List<RayTracingObject> _rayTracingObjects = new List<RayTracingObject>();
    private static List<MeshObject> _meshObjects = new List<MeshObject>();
    private static List<Vector3> _vertices = new List<Vector3>();
    private static List<int> _indices = new List<int>();
    private ComputeBuffer _meshObjectBuffer;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _indexBuffer;

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

    const float PI = 3.14159265f;

    struct MeshObject
    {
        public Matrix4x4 localToWorldMatrix;
        public int indices_offset;
        public int indices_count;
        public MeshObjectType meshObjectType;
    }

    struct RayData
    {
        public Vector3 origin;
        public int set;
    };
    private int raydatabytesize = 16;

    private void ReleaseResources()
    {
        cameraHeight = 0;
        cameraWidth = 0;

        _meshObjectBuffer?.Release();
        _vertexBuffer?.Release();
        _indexBuffer?.Release();
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

        if (cameraWidth != Camera.allCameras[1].pixelWidth || cameraHeight != Camera.allCameras[1].pixelHeight)
        {
            cameraWidth = (uint)Camera.allCameras[1].pixelWidth;
            cameraHeight = (uint)Camera.allCameras[1].pixelHeight;
        }

        if (_rayPointsBuffer == null)
        {
            _rayPointsBuffer = new ComputeBuffer(sourceParams.ntheta*sourceParams.nphi*sourceParams.MAXINTERACTIONS, raydatabytesize);
        }

        if (rds == null)
        {
            rds = new RayData[sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS];
        }
    }
    


    
    #region MeshObjects
    
    private void RebuildMeshObjectBuffers()
    {
        if (!_meshObjectsNeedRebuilding)
        {
            return;
        }

        Debug.Log("Rebuilding Mesh buffers...");

        _meshObjectsNeedRebuilding = false;        

        // Clear all lists
        _meshObjects.Clear();
        _vertices.Clear();
        _indices.Clear();        

        // Loop over all objects and gather their data
        foreach (RayTracingObject obj in _rayTracingObjects)
        {

            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            // Add vertex data
            int firstVertex = _vertices.Count;
            
            // Note: There are objects, that may have the Meshfilter object,
            // but that do not have a mesh assigned to them yet. This checks for this issue. 
            if (mesh != null) {
                _vertices.AddRange(mesh.vertices);

                // Add index data - if the vertex buffer wasn't empty before, the
                // indices need to be offset
                int firstIndex = _indices.Count;
                var indices = mesh.GetIndices(0);
                _indices.AddRange(indices.Select(index => index + firstVertex));

                // Add the object itself
                _meshObjects.Add(new MeshObject()
                {
                    localToWorldMatrix = obj.transform.localToWorldMatrix,
                    indices_offset = firstIndex,
                    indices_count = indices.Length,
                    meshObjectType = obj.meshObjectType
                });

            }
            
        }        

        CreateComputeBuffer(ref _meshObjectBuffer, _meshObjects, 76);
        CreateComputeBuffer(ref _vertexBuffer, _vertices, 12);
        CreateComputeBuffer(ref _indexBuffer, _indices, 4);        
    }
    

    public static void RegisterObject(RayTracingObject obj)
    {
        _rayTracingObjects.Add(obj);
        _meshObjectsNeedRebuilding = true;
    }
    public static void UnregisterObject(RayTracingObject obj)
    {
        _rayTracingObjects.Remove(obj);
        _meshObjectsNeedRebuilding = true;
    }
    #endregion

    private static void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride)
        where T : struct
    {
        // Do we already have a compute buffer?
        if (buffer != null)
        {
            // If no data or buffer doesn't match the given criteria, release it
            if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride)
            {
                buffer.Release();
                buffer = null;
            }
        }

        if (data.Count != 0)
        {
            // If the buffer has been released or wasn't there to
            // begin with, create it
            if (buffer == null)
            {
                buffer = new ComputeBuffer(data.Count, stride);
            }

            // Set data on the buffer
            buffer.SetData(data);
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
        computeShaderTest.SetMatrix("_CameraToWorld", Camera.allCameras[1].cameraToWorldMatrix);
        computeShaderTest.SetMatrix("_CameraInverseProjection", Camera.allCameras[1].projectionMatrix.inverse);
        computeShaderTest.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
        computeShaderTest.SetFloat("_Seed", UnityEngine.Random.value);

        SetComputeBuffer("_MeshObjects", _meshObjectBuffer);
        SetComputeBuffer("_Vertices", _vertexBuffer);
        SetComputeBuffer("_Indices", _indexBuffer);
        SetComputeBuffer("_RayPoints", _rayPointsBuffer);


        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();

        computeShaderTest.SetInt("theta", sourceParams.theta);
        computeShaderTest.SetInt("ntheta", sourceParams.ntheta);
        computeShaderTest.SetInt("phi", sourceParams.phi);
        computeShaderTest.SetInt("nphi", sourceParams.nphi);
        computeShaderTest.SetVector("srcDirection", srcSphere.transform.forward);

        computeShaderTest.SetInt("_MAXINTERACTIONS", sourceParams.MAXINTERACTIONS);
    }

    private void InitRenderTexture(SourceParams sourceParams)
    {
        if (_target == null || _target.width != Screen.width/8 || _target.height != Screen.height/8)
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
        world.AddSurface(surfaceCombo);
        world.AddBottom(bottomCombo);
        world.AddWaterLayers(waterLayerCombo);


    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        if (secondCamera != null)
        {
            secondCameraScript = secondCamera.GetComponent<RayTracingVisualization>();
        }


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

        
        BuildWorld();   
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

        
        if (sourceParams.HasChanged(oldSourceParams))
            {
            Debug.Log("Reeinit raybuffer");
            // reinit rds arrau
            rds = new RayData[sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS];
            // reinit raydatabuffer
            if (_rayPointsBuffer != null)
            {
                _rayPointsBuffer.Release();
            }
            _rayPointsBuffer = new ComputeBuffer(sourceParams.ntheta * sourceParams.nphi * sourceParams.MAXINTERACTIONS, raydatabytesize);            
            
            oldSourceParams = sourceParams.ToStruct();
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
            RebuildMeshObjectBuffers();
            SetShaderParameters();

            InitRenderTexture(sourceParams);
            
            computeShaderTest.SetTexture(0, "Result", _target);

            int threadGroupsY = Mathf.FloorToInt(sourceParams.ntheta / 8.0f);
            int threadGroupsX = Mathf.FloorToInt(sourceParams.nphi / 8.0f);

            Debug.Log(sourceParams.ntheta);

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
}