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
    [SerializeField] GameObject surfaceCombo = null;
    [SerializeField] GameObject bottomCombo = null;
    [SerializeField] GameObject waterLayerCombo = null;
    [SerializeField] int depth = 0;
    [SerializeField] int range = 0;
    [SerializeField] int width = 0;
    [SerializeField] int nrOfWaterPlanes = 0;    
    [SerializeField] Camera secondCamera = null;
    [SerializeField] bool sendRaysContinuosly = false;
    [SerializeField] bool visualizeRays = false;
    [SerializeField] float lineLength = 1;
    [SerializeField] Material lineMaterial = null;
    [SerializeField] GameObject world_manager = null;

    [SerializeField] int theta = 0;
    [SerializeField] int ntheta = 0;
    [SerializeField] int phi = 0;
    [SerializeField] int nphi = 0;
    [SerializeField] int MAXINTERACTIONS = 0;

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
    private int oldMAXINTERACTIONS = 0;
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
        if (cameraWidth != Camera.allCameras[1].pixelWidth || cameraHeight != Camera.allCameras[1].pixelHeight)
        {
            cameraWidth = (uint)Camera.allCameras[1].pixelWidth;
            cameraHeight = (uint)Camera.allCameras[1].pixelHeight;
        }

        if (_rayPointsBuffer == null)
        {
            _rayPointsBuffer = new ComputeBuffer(ntheta*nphi*MAXINTERACTIONS, raydatabytesize);
        }

        if (rds == null)
        {
            rds = new RayData[ntheta * nphi * MAXINTERACTIONS];
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

    #endregion
    

    #region MeshObjects
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

        computeShaderTest.SetInt("theta", theta);
        computeShaderTest.SetInt("ntheta", ntheta);
        computeShaderTest.SetInt("phi", phi);
        computeShaderTest.SetInt("nphi", nphi);
        computeShaderTest.SetVector("srcDirection", srcSphere.transform.forward);

        computeShaderTest.SetInt("_MAXINTERACTIONS", MAXINTERACTIONS);
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
        }
    }

    private void BuildWorld() {

        Debug.Log("Building world. Please wait...");

        World world = world_manager.GetComponent<World>();
        world.AddSource(srcSphere);
        world.AddSurface(surfaceCombo);
        world.AddBottom(bottomCombo);
        world.AddWaterLayers(waterLayerCombo);

        Debug.Log("World build completed.");

    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        if (secondCamera != null)
        {
            secondCameraScript = secondCamera.GetComponent<RayTracingVisualization>();
        }

        // setup the scene
        // SetUpScene();
        
        //rds = new RayData[ntheta * nphi * MAXINTERACTIONS];
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
        float origin_theta = (float)Math.Acos(srcSphere.transform.forward.y);
        float origin_phi = (float)Math.Atan2(srcSphere.transform.forward.z, srcSphere.transform.forward.x);

        float theta_rad = theta * PI / 180; //convert to radians
        float phi_rad = phi * PI / 180;

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
        }

        if (oldtheta != theta || oldntheta != ntheta || oldphi != phi || oldnphi != nphi || oldMAXINTERACTIONS != MAXINTERACTIONS)
        {            
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
            oldMAXINTERACTIONS = MAXINTERACTIONS;
        }

        if (oldWidth != width || oldRange != range || oldDepth != depth || oldNrOfWaterPlanes != nrOfWaterPlanes)
        { // change has happened to the scene, update (create new) meshes for the surface, seafloor and water planes            
            foreach(GameObject obj in waterplanes)
            {
                Destroy(obj);
            }
            waterplanes.Clear();

            // SetUpScene();         

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
                    line.startWidth = 0.01f;
                    line.endWidth = 0.01f;
                    line.positionCount = 2;
                    line.useWorldSpace = true;

                    if (i % MAXINTERACTIONS == 0) // first interaction for a line, draw line from source to first interaction
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