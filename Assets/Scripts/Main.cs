using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Main : MonoBehaviour
{    
    public ComputeShader computeShaderTest = null;

    [SerializeField] GameObject srcSphere = null;
    [SerializeField] GameObject targetSphere = null;
    [SerializeField] GameObject seafloor = null;
    [SerializeField] GameObject seafloor2 = null;
    [SerializeField] GameObject surface = null;
    [SerializeField] GameObject surface2 = null;
    [SerializeField] GameObject waterplane = null;
    [SerializeField] GameObject waterplane2 = null;
    [SerializeField] int depth = 0;
    [SerializeField] int range = 0;
    [SerializeField] int width = 0;
    [SerializeField] int nrOfWaterPlanes = 0;    
    [SerializeField] Camera secondCamera = null;
    [SerializeField] bool sendRaysContinuosly = false;
    [SerializeField] bool visualizeRays = false;
    [SerializeField] float lineLength = 1;
    [SerializeField] Material lineMaterial = null;
    [SerializeField] int theta = 0;
    [SerializeField] int ntheta = 0;
    [SerializeField] int phi = 0;
    [SerializeField] int nphi = 0;

    private Mesh waterplaneMesh = null;
    private Mesh waterplaneMesh2 = null;
    private Mesh surfaceMesh = null;
    private Mesh surfaceMesh2 = null;
    private Mesh seafloorMesh = null;
    private Mesh seafloorMesh2 = null;

    private int oldtheta = 0;
    private int oldntheta = 0;
    private int oldphi = 0;
    private int oldnphi = 0;
    private int oldDepth = 0;
    private int oldRange = 0;
    private int oldWidth = 0;
    private int oldNrOfWaterPlanes = 0;

    uint cameraWidth = 0;
    uint cameraHeight = 0;

    private List<GameObject> waterplanes = new List<GameObject>();
    private static bool _meshObjectsNeedRebuilding = false;
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

    //private int threadGroupsDivisionX = 64;
    //private int threadGroupsDivisionY = 64;
    //private int threadGroupInternalSizeX = 8;
    //private int threadGroupInternalSizeY = 8;

    private bool doRayTracing = false;
    private bool lockRayTracing = false;

    private const int MAXINTERACTIONS = 1;

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
        if (cameraWidth != Camera.allCameras[1].pixelWidth || cameraHeight != Camera.allCameras[1].pixelHeight)
        {
            cameraWidth = (uint)Camera.allCameras[1].pixelWidth;
            cameraHeight = (uint)Camera.allCameras[1].pixelHeight;
        }

        if (_rayPointsBuffer == null)
        {
            _rayPointsBuffer = new ComputeBuffer(16384*MAXINTERACTIONS, raydatabytesize);
        }
    }

    #region SceneStuff
    private Mesh CreateSurfaceMesh(bool flipped=false)
    {
        Mesh surfaceMesh = new Mesh()
        {
            name = "Surface Mesh"
        };

        surfaceMesh.vertices = new Vector3[] {
            Vector3.zero, new Vector3(range, 0f, 0f), new Vector3(0f, 0f, width), new Vector3(range, 0f, width)
        };

        surfaceMesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back
        };

        surfaceMesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f)
        };

        if (flipped)
        {
            surfaceMesh.triangles = new int[] {
                1, 2, 0, 3, 2, 1
            };
        }
        else
        {
            surfaceMesh.triangles = new int[] {
                0, 2, 1, 1, 2, 3
            };
        }

        return surfaceMesh;
    }

    private Mesh CreateSeafloorMesh(bool flipped = false)
    {
        Mesh seafloorMesh = new Mesh()
        {
            name = "Seafloor Mesh"
        };

        seafloorMesh.vertices = new Vector3[] {
            new Vector3(0f, -depth, 0f), new Vector3(range, -depth, 0f), new Vector3(0f, -depth, width), new Vector3(range, -depth, width)
        };

        seafloorMesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back
        };

        seafloorMesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f)
        };

        if (flipped)
        {
            seafloorMesh.triangles = new int[] {
                1, 2, 0, 3, 2, 1
            };
        }
        else
        {
            seafloorMesh.triangles = new int[] {
                0, 2, 1, 1, 2, 3
            };
        }

        return seafloorMesh;
    }

    private Mesh CreateWaterPlaneMesh(bool flipped=false)
    {        
        Mesh waterPlaneMesh = new Mesh()
        {
            name = "Waterplane Mesh"
        };

        float delta = (float)depth / (float)(nrOfWaterPlanes + 1);

        waterPlaneMesh.vertices = new Vector3[] {
            new Vector3(0f, -delta, 0f), new Vector3(range, -delta, 0f), new Vector3(0f, -delta, width), new Vector3(range, -delta, width)
        };

        waterPlaneMesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back
        };

        waterPlaneMesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f)
        };

        if (flipped)
        {
            waterPlaneMesh.triangles = new int[] {                
                1, 2, 0, 3, 2, 1
            };
        }
        else
        {
            waterPlaneMesh.triangles = new int[] {
                0, 2, 1, 1, 2, 3
            };
        }
                

        return waterPlaneMesh;
    }    

    private void SetUpScene()
    {        
        // setup the scene, unity meshes seem to be one-sided, so all planes are created from two meshes, facing in opposite direction to ensure that rays can hit the planes from either side
        // SURFACE // 
        surfaceMesh = CreateSurfaceMesh();
        MeshFilter surfaceMF = (MeshFilter)surface.GetComponent("MeshFilter");
        surfaceMF.mesh = surfaceMesh;

        surfaceMesh2 = CreateSurfaceMesh(true);
        MeshFilter surfaceMF2 = (MeshFilter)surface2.GetComponent("MeshFilter");
        surfaceMF2.mesh = surfaceMesh2;

        // WATER PLANE
        if (nrOfWaterPlanes > 0) // if >0, create one waterplane
        {
            waterplaneMesh = CreateWaterPlaneMesh();
            MeshFilter waterplaneMF = (MeshFilter)waterplane.GetComponent("MeshFilter");
            waterplaneMF.mesh = waterplaneMesh;

            waterplaneMesh2 = CreateWaterPlaneMesh(true);
            MeshFilter waterplaneMF2 = (MeshFilter)waterplane2.GetComponent("MeshFilter");
            waterplaneMF2.mesh = waterplaneMesh2;

            if (nrOfWaterPlanes > 1) // create copies of the original waterplane at new positions relative to the original to divide the volume between the seafloor and surface evenly
            {
                float delta = (float)depth / (float)(nrOfWaterPlanes + 1);

                for (int i = 0; i < nrOfWaterPlanes; i++)
                {
                    GameObject gmobj = Instantiate(waterplane, new Vector3(0, -delta*i, 0), Quaternion.identity);
                    waterplanes.Add(gmobj);

                    GameObject gmobj2 = Instantiate(waterplane2, new Vector3(0, -delta * i, 0), Quaternion.identity);
                    waterplanes.Add(gmobj2);
                }                
            }
        }        

        // SEAFLOOR //
        seafloorMesh = CreateSeafloorMesh();
        MeshFilter seafloorMF = (MeshFilter)seafloor.GetComponent("MeshFilter");
        seafloorMF.mesh = seafloorMesh;

        seafloorMesh2 = CreateSeafloorMesh(true);
        MeshFilter seafloorMF2 = (MeshFilter)seafloor2.GetComponent("MeshFilter");
        seafloorMF2.mesh = seafloorMesh2;

    }
    #endregion

    private void RebuildMeshObjectBuffers()
    {
        if (!_meshObjectsNeedRebuilding)
        {
            return;
        }

        _meshObjectsNeedRebuilding = false;        

        // Clear all lists
        _meshObjects.Clear();
        _vertices.Clear();
        _indices.Clear();        

        // Loop over all objects and gather their data
        foreach (RayTracingObject obj in _rayTracingObjects)
        {
            if (nrOfWaterPlanes <= 0 && (obj.meshObjectType == MeshObjectType.WATERPLANE_BOTTOM || obj.meshObjectType == MeshObjectType.WATERPLANE_TOP))
            {
                continue;
            }            
            Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            // Add vertex data
            int firstVertex = _vertices.Count;
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
            }) ;
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

        computeShaderTest.SetInt("theta", theta);
        computeShaderTest.SetInt("ntheta", ntheta);
        computeShaderTest.SetInt("phi", phi);
        computeShaderTest.SetInt("nphi", nphi);
        computeShaderTest.SetVector("srcDirection", srcSphere.transform.forward);
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width/8 || _target.height != Screen.height/8)
        {
            // Release render texture if we already have one
            if (_target != null)
            {
                _target.Release();                
            }

            // Get a render target for Ray Tracing
            _target = new RenderTexture(ntheta, nphi, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();

            //int thgx = Mathf.FloorToInt(Screen.width / threadGroupsDivisionX);
            //int thgy = Mathf.FloorToInt(Screen.height / threadGroupsDivisionY);
            //_target = new RenderTexture(thgx * threadGroupInternalSizeX, thgy * threadGroupInternalSizeY, 0,
            //    RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            //_target.enableRandomWrite = true;
            //_target.Create();

            //Debug.Log(_target.width);
            //Debug.Log(_target.height);
            //Debug.Log("Width: " + Screen.width);
            //Debug.Log("Height: " + Screen.height);
        }
    }


    private void OnEnable()
    {
        Debug.Log("OnEnable");
        if (secondCamera != null)
        {
            secondCameraScript = secondCamera.GetComponent<RayTracingVisualization>();
        }

        // setup the scene
        SetUpScene();
        rds = new RayData[ntheta * nphi * MAXINTERACTIONS];
    }

    // Start is called before the first frame update
    void Start()
    {        
        Renderer srcRenderer = srcSphere.GetComponent<Renderer>();
        srcRenderer.material.SetColor("_Color", Color.green);
        Renderer targetRenderer = targetSphere.GetComponent<Renderer>();
        targetRenderer.material.SetColor("_Color", Color.red);
        Debug.Log("Start");

        srcDirectionLine = new GameObject("SourceDirectionLine").AddComponent<LineRenderer>();

        srcDirectionLine.startColor = Color.black;
        srcDirectionLine.endColor = Color.black;

        srcDirectionLine.startWidth = 0.01f;
        srcDirectionLine.endWidth = 0.01f;
        srcDirectionLine.positionCount = 2;
        srcDirectionLine.useWorldSpace = true;

        srcDirectionLine.SetPosition(0, srcSphere.transform.position);
        srcDirectionLine.SetPosition(1, srcSphere.transform.position + srcSphere.transform.forward * lineLength);

        srcDirectionLine.material = lineMaterial;
        srcDirectionLine.material.color = Color.black;

        Debug.Log(srcSphere.transform.forward);
        Debug.Log(srcSphere.transform.forward.normalized);

        Vector3[] viewLines = DirectionLines();

        // line1
        srcViewLine1 = new GameObject("View line1").AddComponent<LineRenderer>();
        srcViewLine1.startColor = Color.black;
        srcViewLine1.endColor = Color.black;

        srcViewLine1.startWidth = 0.01f;
        srcViewLine1.endWidth = 0.01f;
        srcViewLine1.positionCount = 2;
        srcViewLine1.useWorldSpace = true;

        srcViewLine1.SetPosition(0, srcSphere.transform.position);
        srcViewLine1.SetPosition(1, srcSphere.transform.position + viewLines[0] * lineLength);

        srcViewLine1.material = lineMaterial;
        srcViewLine1.material.color = Color.black;

        // line2
        srcViewLine2 = new GameObject("View line2").AddComponent<LineRenderer>();
        srcViewLine2.startColor = Color.black;
        srcViewLine2.endColor = Color.black;

        srcViewLine2.startWidth = 0.01f;
        srcViewLine2.endWidth = 0.01f;
        srcViewLine2.positionCount = 2;
        srcViewLine2.useWorldSpace = true;

        srcViewLine2.SetPosition(0, srcSphere.transform.position);
        srcViewLine2.SetPosition(1, srcSphere.transform.position + viewLines[1] * lineLength);

        srcViewLine2.material = lineMaterial;
        srcViewLine2.material.color = Color.black;

        // line3
        srcViewLine3 = new GameObject("View line3").AddComponent<LineRenderer>();
        srcViewLine3.startColor = Color.black;
        srcViewLine3.endColor = Color.black;

        srcViewLine3.startWidth = 0.01f;
        srcViewLine3.endWidth = 0.01f;
        srcViewLine3.positionCount = 2;
        srcViewLine3.useWorldSpace = true;

        srcViewLine3.SetPosition(0, srcSphere.transform.position);
        srcViewLine3.SetPosition(1, srcSphere.transform.position + viewLines[2] * lineLength);

        srcViewLine3.material = lineMaterial;
        srcViewLine3.material.color = Color.black;

        // line4
        srcViewLine4 = new GameObject("View line4").AddComponent<LineRenderer>();
        srcViewLine4.startColor = Color.black;
        srcViewLine4.endColor = Color.black;

        srcViewLine4.startWidth = 0.01f;
        srcViewLine4.endWidth = 0.01f;
        srcViewLine4.positionCount = 2;
        srcViewLine4.useWorldSpace = true;

        srcViewLine4.SetPosition(0, srcSphere.transform.position);
        srcViewLine4.SetPosition(1, srcSphere.transform.position + viewLines[3] * lineLength);

        srcViewLine4.material = lineMaterial;
        srcViewLine4.material.color = Color.black;

    }

    private void UpdateSourceViewLines()
    {
        Vector3[] viewLines = DirectionLines();

        srcViewLine1.SetPosition(0, srcSphere.transform.position);
        srcViewLine1.SetPosition(1, srcSphere.transform.position + viewLines[0] * lineLength);

        srcViewLine2.SetPosition(0, srcSphere.transform.position);
        srcViewLine2.SetPosition(1, srcSphere.transform.position + viewLines[1] * lineLength);

        srcViewLine3.SetPosition(0, srcSphere.transform.position);
        srcViewLine3.SetPosition(1, srcSphere.transform.position + viewLines[2] * lineLength);

        srcViewLine4.SetPosition(0, srcSphere.transform.position);
        srcViewLine4.SetPosition(1, srcSphere.transform.position + viewLines[3] * lineLength);
    }

    Vector3[] DirectionLines()
    {
        float theta_rad = theta * PI / 180; //convert to radians
        float phi_rad = phi * PI / 180;

        float dtheta = theta_rad / ntheta; // resolution in theta
        float dphi = phi_rad / nphi; // resolution in phi

        Vector2[] offsets = new Vector2[4];

        // calculate the angular offset for the lines compared to the forward vector of the source
        int k = 0;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                float offset_theta = -theta_rad / 2 + i * theta_rad;
                float offset_phi = -phi_rad / 2 + j * phi_rad;
                offsets[k] = new Vector2(offset_theta, offset_phi);
                k++;
            }            
        }        
        
        // calculate angles of the source's forward vector
        float origin_theta = (float)Math.Acos(srcSphere.transform.forward.y);
        float origin_phi = Math.Sign(srcSphere.transform.forward.z) * (float)Math.Acos(srcSphere.transform.forward.x / Math.Sqrt(Math.Pow(srcSphere.transform.forward.x, 2) + Math.Pow(srcSphere.transform.forward.z, 2)));

        // add the angular offset
        float[] theta_results = new float[4];
        float[] phi_results = new float[4];

        for (int i = 0; i < 4; i++) 
        {
            theta_results[i] = origin_theta + offsets[i].x;
            phi_results[i] = origin_phi + offsets[i].y;
        }

        Vector3[] viewLines = new Vector3[4] 
        {
            new Vector3((float)(Math.Sin(theta_results[0]) * Math.Cos(phi_results[0])), (float)Math.Cos(theta_results[0]), (float)(Math.Sin(theta_results[0]) * Math.Sin(phi_results[0]))),
            new Vector3((float)(Math.Sin(theta_results[1]) * Math.Cos(phi_results[1])), (float)Math.Cos(theta_results[1]), (float)(Math.Sin(theta_results[1]) * Math.Sin(phi_results[1]))),
            new Vector3((float)(Math.Sin(theta_results[2]) * Math.Cos(phi_results[2])), (float)Math.Cos(theta_results[2]), (float)(Math.Sin(theta_results[2]) * Math.Sin(phi_results[2]))),
            new Vector3((float)(Math.Sin(theta_results[3]) * Math.Cos(phi_results[3])), (float)Math.Cos(theta_results[3]), (float)(Math.Sin(theta_results[3]) * Math.Sin(phi_results[3])))
        };

        return viewLines;

        //float theta_res = origin_theta + offset_theta;
        //float phi_res = origin_phi + offset_phi;

        // calculate new direction (r is assumed to be one since srcDirection should be normalized)
        //Vector3 direction = new Vector3
        //    (
        //        Math.Sin(theta_res) * cos(phi_res), //x
        //        cos(theta_res),                     //y
        //        sin(theta_res) * sin(phi_res)       //z
        //    );
    }

    // Update is called once per frame
    void Update()
    {
        if (srcSphere == null)
        {
            Debug.Log("No source sphere!");
        }

        if (targetSphere == null)
        {
            Debug.Log("No target sphere!");
        }

        if (surface == null)
        {
            Debug.Log("No surface!");
        }

        if (seafloor == null)
        {
            Debug.Log("No seafloor!");
        }

        if (srcDirectionLine != null)
        {
            srcDirectionLine.SetPosition(0, srcSphere.transform.position);
            srcDirectionLine.SetPosition(1, srcSphere.transform.position + srcSphere.transform.forward * lineLength);

            UpdateSourceViewLines();

            Debug.Log(srcSphere.transform.forward);
            Debug.Log(srcSphere.transform.forward.normalized);
        }

        if (oldtheta != theta || oldntheta != ntheta || oldphi != phi || oldnphi != nphi)
        {
            // do stuff
            // reinit rds arrau
            rds = new RayData[ntheta * nphi * MAXINTERACTIONS];
            // reinit raydatabuffer
            if (_rayPointsBuffer != null)
            {
                _rayPointsBuffer.Release();
            }
            _rayPointsBuffer = new ComputeBuffer(ntheta * nphi * MAXINTERACTIONS, raydatabytesize);

            oldtheta = theta;
            oldntheta = ntheta;
            oldphi = phi;
            oldnphi = nphi;
        }

        if (oldWidth != width || oldRange != range || oldDepth != depth || oldNrOfWaterPlanes != nrOfWaterPlanes)
        { // change has happened to the scene, update (create new) meshes for the surface, seafloor and water planes
            
            foreach(GameObject obj in waterplanes)
            {
                Destroy(obj);
            }
            waterplanes.Clear();

            SetUpScene();         

            // update values
            oldDepth = depth;
            oldRange = range;
            oldWidth = width;
            oldNrOfWaterPlanes = nrOfWaterPlanes; 
            _meshObjectsNeedRebuilding = true;
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

            InitRenderTexture();

            computeShaderTest.SetTexture(0, "Result", _target);
            //int threadGroupsX = Mathf.FloorToInt(Screen.width / threadGroupsDivisionX);
            //int threadGroupsY = Mathf.FloorToInt(Screen.height / threadGroupsDivisionY);

            int threadGroupsX = Mathf.FloorToInt(ntheta / 8.0f);
            int threadGroupsY = Mathf.FloorToInt(nphi / 8.0f);

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
                        int mod = i % MAXINTERACTIONS;
                        int step = MAXINTERACTIONS - mod - 1;
                        i += step;
                        continue;
                    }

                    line = new GameObject("Line").AddComponent<LineRenderer>();

                    line.startColor = Color.black;
                    line.endColor = Color.black;

                    line.startWidth = 0.01f;
                    line.endWidth = 0.01f;
                    line.positionCount = 2;
                    line.useWorldSpace = true;

                    // add material to lines to make them another color than magenta, however this heavily worsens fps when there are many rays
                    // line.material = lineMaterial;
                    // line.material.color = Color.black;

                    if (i % MAXINTERACTIONS == 0) // first interaction for a line, draw line from source to first interaction
                    {
                        line.SetPosition(0, srcOrigin);
                    }
                    else //
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

                    line.startColor = Color.black;
                    line.endColor = Color.black;

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

    //[ImageEffectOpaque]
    //private void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{        
        //CreateResources();
        //RebuildMeshObjectBuffers();
        //SetShaderParameters();

        //InitRenderTexture();
        //computeShaderTest.SetTexture(0, "Result", _target);
        //int threadGroupsX = Mathf.FloorToInt(Screen.width / threadGroupsDivisionX);
        //int threadGroupsY = Mathf.FloorToInt(Screen.height / threadGroupsDivisionY);
        //Debug.Log(threadGroupsX);
        //Debug.Log(threadGroupsY);

        //Debug.Log("ThreadGroups: " + threadGroupsX + ", " + threadGroupsY);
        //computeShaderTest.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        //Graphics.Blit(source, destination);        

        //RayData[] rds = new RayData[16384];
        //_rayPointsBuffer.GetData(rds);

        //for (int i = 0; i < rds.Length; i++)
        //{
        //    Debug.Log("i : " + i);
        //    Debug.Log("Origin: " + rds[i].origin);
        //    Debug.Log("Set : " + rds[i].set);
        //}
    //}    
}