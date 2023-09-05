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
    [SerializeField] GameObject surface = null;
    [SerializeField] int depth = 0;
    [SerializeField] int range = 0;
    [SerializeField] int width = 0;
    [SerializeField] int nrOfWaterPlanes = 0;    
    [SerializeField] Camera secondCamera = null;    
    
    private Mesh waterPlane = null;
    private Mesh surfaceMesh = null;
    private Mesh seafloorMesh = null;    

    private int oldDepth = 0;
    private int oldRange = 0;
    private int oldWidth = 0;
    private int oldNrOfWaterPlanes = 0;

    uint cameraWidth = 0;
    uint cameraHeight = 0;
    
    private static bool _meshObjectsNeedRebuilding = false;
    private static List<RayTracingObject> _rayTracingObjects = new List<RayTracingObject>();
    private static List<MeshObject> _meshObjects = new List<MeshObject>();
    private static List<Vector3> _vertices = new List<Vector3>();
    private static List<int> _indices = new List<int>();
    private ComputeBuffer _meshObjectBuffer;
    private ComputeBuffer _vertexBuffer;
    private ComputeBuffer _indexBuffer;

    private ComputeBuffer _rayPointsBuffer;

    //RayTracingVisualization secondCameraScript = null;
    private RenderTexture _target;
    RenderTexture rayTracingOutput = null;

    
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

    private void ReleaseResources()
    {

        if (rayTracingOutput)
        {
            rayTracingOutput.Release();
            rayTracingOutput = null;
        }

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
        if (cameraWidth != Camera.main.pixelWidth || cameraHeight != Camera.main.pixelHeight)
        {
            if (rayTracingOutput)
                rayTracingOutput.Release();

            rayTracingOutput = new RenderTexture(Camera.main.pixelWidth, Camera.main.pixelHeight, 0, RenderTextureFormat.ARGBFloat);
            rayTracingOutput.enableRandomWrite = true;
            rayTracingOutput.Create();

            cameraWidth = (uint)Camera.main.pixelWidth;
            cameraHeight = (uint)Camera.main.pixelHeight;
        }

        if (_rayPointsBuffer == null)
        {
            _rayPointsBuffer = new ComputeBuffer(10000, 16);
        }
    }

    private Mesh CreateSurfaceMesh()
    {
        Mesh surface = new Mesh()
        {
            name = "Surface Mesh"
        };

        surface.vertices = new Vector3[] {
            Vector3.zero, new Vector3(range, 0f, 0f), new Vector3(0f, 0f, width), new Vector3(range, 0f, width)
        };

        surface.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back
        };

        surface.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f)
        };

        surface.triangles = new int[] {
            0, 2, 1, 1, 2, 3
        };

        return surface;
    }

    private Mesh CreateSeafloorMesh()
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

        seafloorMesh.triangles = new int[] {
            0, 2, 1, 1, 2, 3
        };

        return seafloorMesh;
    }

    private Mesh CreateWaterPlaneMesh()
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

        waterPlaneMesh.triangles = new int[] {
            0, 2, 1, 1, 2, 3
        };        

        return waterPlaneMesh;
    }

    private void SetUpScene()
    {
        List<Mesh> planes = new List<Mesh>();
        // setup the scene
        // SURFACE // 
        surfaceMesh = CreateSurfaceMesh();
        MeshFilter surfaceMF = (MeshFilter)surface.GetComponent("MeshFilter");
        surfaceMF.mesh = surfaceMesh;
        planes.Add(surfaceMesh);

        // WATER PLANE
        if (nrOfWaterPlanes > 0) // if >0, create one waterplane, waterPlanesInstanceData handles the remaining waterplanes to be created
        {
            waterPlane = CreateWaterPlaneMesh();
        }

        // SEAFLOOR //
        seafloorMesh = CreateSeafloorMesh();
        MeshFilter seafloorMF = (MeshFilter)seafloor.GetComponent("MeshFilter");
        seafloorMF.mesh = seafloorMesh;
        planes.Add(seafloorMesh);   
    }

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
            Debug.Log("Hejdej");
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
        computeShaderTest.SetMatrix("_CameraToWorld", Camera.main.cameraToWorldMatrix);
        computeShaderTest.SetMatrix("_CameraInverseProjection", Camera.main.projectionMatrix.inverse);
        computeShaderTest.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
        computeShaderTest.SetFloat("_Seed", UnityEngine.Random.value);        

        SetComputeBuffer("_MeshObjects", _meshObjectBuffer);
        SetComputeBuffer("_Vertices", _vertexBuffer);
        SetComputeBuffer("_Indices", _indexBuffer);
        SetComputeBuffer("_RayPoints", _rayPointsBuffer);
        
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
            {
                _target.Release();                
            }

            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();            
        }
    }


    private void OnEnable()
    {
        // setup the scene
        SetUpScene();

    }

    // Start is called before the first frame update
    void Start()
    {        
        Renderer srcRenderer = srcSphere.GetComponent<Renderer>();
        srcRenderer.material.SetColor("_Color", Color.green);
        Renderer targetRenderer = targetSphere.GetComponent<Renderer>();
        targetRenderer.material.SetColor("_Color", Color.red);        
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

        if (oldWidth != width || oldRange != range || oldDepth != depth || oldNrOfWaterPlanes != nrOfWaterPlanes)
        { // change has happened to the scene, update (create new) meshes for the surface, seafloor and water planes

            SetUpScene();         

            // update values
            oldDepth = depth;
            oldRange = range;
            oldWidth = width;
            oldNrOfWaterPlanes = nrOfWaterPlanes;
            _meshObjectsNeedRebuilding = true;
        }
    }    

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        CreateResources();
        RebuildMeshObjectBuffers();
        SetShaderParameters();

        InitRenderTexture();
        computeShaderTest.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        computeShaderTest.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        Graphics.Blit(_target, destination);

        RayData[] vec = new RayData[10000];
        _rayPointsBuffer.GetData(vec);
        Debug.Log(vec[0].origin);
        Debug.Log(vec[0].set);
        Debug.Log(vec[1].origin);
        Debug.Log(vec[1].set);
    }
}
