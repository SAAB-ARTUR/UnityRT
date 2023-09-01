using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
using UnityEngine.UI;
using System.IO;

public class Main : MonoBehaviour
{
    public RayTracingShader rayTracingShader = null;

    [SerializeField] GameObject srcSphere = null;
    [SerializeField] GameObject targetSphere = null;
    [SerializeField] GameObject seafloor = null;
    [SerializeField] GameObject surface = null;
    [SerializeField] int depth = 0;
    [SerializeField] int range = 0;
    [SerializeField] int width = 0;
    [SerializeField] int nrOfWaterPlanes = 0;
    [SerializeField] Material waterPlaneMaterial = null;
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

    RayTracingAccelerationStructure rtas = null;

    RayTracingVisualization secondCameraScript = null;
    RenderTexture rayTracingOutput = null;

    SurfaceAndSeafloorInstanceData surfaceInstanceData = null;
    SurfaceAndSeafloorInstanceData seafloorInstanceData = null;
    WaterPlaneInstanceData waterPlanesInstanceData = null;

    private void ReleaseResources()
    {
        if (rtas != null)
        {
            rtas.Release();
            rtas = null;
        }

        if (rayTracingOutput)
        {
            rayTracingOutput.Release();
            rayTracingOutput = null;
        }

        cameraHeight = 0;
        cameraWidth = 0;

        if (surfaceInstanceData != null)
        {
            surfaceInstanceData.Dispose();
            surfaceInstanceData = null;
        }
        if (waterPlanesInstanceData != null)
        {
            waterPlanesInstanceData.Dispose();
            waterPlanesInstanceData = null;
        }
        if (seafloorInstanceData != null)
        {
            seafloorInstanceData.Dispose();
            seafloorInstanceData = null;
        }
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
            if (rayTracingOutput)
                rayTracingOutput.Release();

            rayTracingOutput = new RenderTexture(Camera.allCameras[1].pixelWidth, Camera.allCameras[1].pixelHeight, 0, RenderTextureFormat.ARGBFloat);
            rayTracingOutput.enableRandomWrite = true;
            rayTracingOutput.Create();

            cameraWidth = (uint)Camera.allCameras[1].pixelWidth;
            cameraHeight = (uint)Camera.allCameras[1].pixelHeight;
        }

        if (surfaceInstanceData == null)
        {           
            surfaceInstanceData = new SurfaceAndSeafloorInstanceData();
        }  
        if (seafloorInstanceData == null)
        {
            seafloorInstanceData = new SurfaceAndSeafloorInstanceData();
        }

        if (nrOfWaterPlanes > 0 && (waterPlanesInstanceData == null || waterPlanesInstanceData.layers != nrOfWaterPlanes || waterPlanesInstanceData.depth != depth))
        {
            if (waterPlanesInstanceData != null)
            {
                waterPlanesInstanceData.Dispose();
            }
            waterPlanesInstanceData = new WaterPlaneInstanceData(nrOfWaterPlanes, depth);
        }
        else if (nrOfWaterPlanes <= 0)
        {
            if (waterPlanesInstanceData != null)
            {
                waterPlanesInstanceData.Dispose();
            }
            waterPlanesInstanceData = null;
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

    private void OnEnable()
    {
        if (rtas != null)
            return;

        rtas = new RayTracingAccelerationStructure();
        
        if (secondCamera != null)
        {
            secondCameraScript = secondCamera.GetComponent<RayTracingVisualization>();
        }

        // setup the scene
        // SURFACE // 
        surfaceMesh = CreateSurfaceMesh();

        MeshFilter surfaceMF = (MeshFilter)surface.GetComponent("MeshFilter");
        surfaceMF.mesh = surfaceMesh;

        // WATER PLANE
        if (nrOfWaterPlanes > 0) // if >0, create one waterplane, waterPlanesInstanceData handles the remaining waterplanes to be created
        {
            waterPlane = CreateWaterPlaneMesh();
        }

        // SEAFLOOR //
        seafloorMesh = CreateSeafloorMesh();

        MeshFilter seafloorMF = (MeshFilter)seafloor.GetComponent("MeshFilter");
        seafloorMF.mesh = seafloorMesh;

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
          // SURFACE //            

            surfaceMesh = CreateSurfaceMesh();
            MeshFilter surfaceMF = (MeshFilter)surface.GetComponent("MeshFilter");
            surfaceMF.mesh = surfaceMesh;

            // WATER PLANES //
            if (nrOfWaterPlanes > 0) // if >0, create one waterplane, waterPlanesInstanceData handles the remaining waterplanes to be created
            {
                waterPlane = CreateWaterPlaneMesh();
            }

            // SEAFLOOR //
            seafloorMesh = CreateSeafloorMesh();
            MeshFilter seafloorMF = (MeshFilter)seafloor.GetComponent("MeshFilter");
            seafloorMF.mesh = seafloorMesh;

            // update values
            oldDepth = depth;
            oldRange = range;
            oldWidth = width;
            oldNrOfWaterPlanes = nrOfWaterPlanes;
        }
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        CreateResources();

        CommandBuffer cmdBuffer = new CommandBuffer();
        cmdBuffer.name = "RT Test";

        rtas.ClearInstances();

        try
        {
            if (surfaceInstanceData != null && seafloorInstanceData != null)
            {
                MeshFilter surfaceMF = (MeshFilter)surface.GetComponent("MeshFilter");
                Mesh surfaceMesh = surfaceMF.mesh;                

                MeshRenderer surfaceMR = (MeshRenderer)surface.GetComponent("MeshRenderer");
                Material surfaceMaterial = surfaceMR.material;
                RayTracingMeshInstanceConfig surfaceConfig = new RayTracingMeshInstanceConfig(surfaceMesh, 0, surfaceMaterial);

                MeshFilter seafloorMF = (MeshFilter)seafloor.GetComponent("MeshFilter");
                Mesh seafloorMesh = seafloorMF.mesh;

                MeshRenderer seafloorMR = (MeshRenderer)seafloor.GetComponent("MeshRenderer");
                Material seafloorMaterial = seafloorMR.material;
                RayTracingMeshInstanceConfig seafloorConfig = new RayTracingMeshInstanceConfig(seafloorMesh, 0, seafloorMaterial);                

                // add meshes and materials to rt accelereation structure
                rtas.AddInstances(surfaceConfig, surfaceInstanceData.matrices);
                rtas.AddInstances(seafloorConfig, seafloorInstanceData.matrices);

                if (waterPlanesInstanceData != null) // add water planes to rtas
                {
                    RayTracingMeshInstanceConfig waterPlaneConfig = new RayTracingMeshInstanceConfig(waterPlane, 0, waterPlaneMaterial);
                    rtas.AddInstances(waterPlaneConfig, waterPlanesInstanceData.matrices);                    
                }
                
            }
            else
            {
                Debug.Log("InstanceData is null for either the surface or the seadloor."); 
            }
        }
        catch (Exception e)
        {
            Debug.Log("An exception occurred: " + e.Message);
        }        

        cmdBuffer.BuildRayTracingAccelerationStructure(rtas);
        cmdBuffer.SetRayTracingShaderPass(rayTracingShader, "RTPass");         

        // define "shared" variables between the CPU and GPU code
        cmdBuffer.SetRayTracingAccelerationStructure(rayTracingShader, Shader.PropertyToID("g_AccelStruct"), rtas);
        cmdBuffer.SetRayTracingMatrixParam(rayTracingShader, Shader.PropertyToID("g_InvViewMatrix"), Camera.allCameras[1].cameraToWorldMatrix);
        cmdBuffer.SetRayTracingFloatParam(rayTracingShader, Shader.PropertyToID("g_Zoom"), Mathf.Tan(Mathf.Deg2Rad * Camera.allCameras[1].fieldOfView * 0.5f));

        Vector3[] points = new Vector3[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)};

        // Output
        cmdBuffer.SetRayTracingTextureParam(rayTracingShader, Shader.PropertyToID("g_Output"), rayTracingOutput);        

        cmdBuffer.DispatchRays(rayTracingShader, "MainRayGenShader", cameraWidth, cameraHeight, 1); // de sista 3 gissar jag har att göra med hur många rays som ska skickas

        Graphics.ExecuteCommandBuffer(cmdBuffer);


        cmdBuffer.Release();
        
        secondCameraScript.receiveData(rayTracingOutput); //send raytracing results to display to the other camera
        
        Graphics.Blit(source, destination);
    }
}
