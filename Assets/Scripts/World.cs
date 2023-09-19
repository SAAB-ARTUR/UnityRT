using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityTemplateProjects;

public class World : MonoBehaviour
{
    [SerializeField] float sourceDepth = 0;

    public int range
    {
        get { return _range; }
        set
        {
            if (value > 0) { _range = value; }
            else { _range = 0; }
        }
    }

    [SerializeField]
    private int _range;

    public int waterDepth
    {
        get { return _waterDepth; }
        set
        {
            if (value > 0) { _waterDepth = value; }
            else { _waterDepth = 0; }
        }
    }

    [SerializeField]
    private int _waterDepth;

    public int nrOfWaterplanes
    {
        get { return _nrOfWaterplanes; }
        set
        {
            if (value > 0) { _nrOfWaterplanes = value; }
            else { _nrOfWaterplanes = 0; }
        }
    }

    [SerializeField]
    private int _nrOfWaterplanes;

    private class State 
    {
        public float depth;
        public float range;
        public int nrOfWaterplanes;
        public Vector3 position;        
    }

    private State state0 = null;
    private State state;

    private Camera sourceSphere;
    private GameObject surface;
    private GameObject bottom;
    //private List<GameObject> waterLayers;

    // Start is called before the first frame update
    void Start()
    {
        state = new State();
        // Set initial state
        state.depth = waterDepth;
        state.range = range;
        state.nrOfWaterplanes = nrOfWaterplanes;
        state.position = Vector3.zero;
    }

    State getCurrentState() 
    {
        state = new State();
        // Set initial state
        state.depth = waterDepth;
        state.range = range;
        state.nrOfWaterplanes = nrOfWaterplanes;
        state.position = this.transform.position;

        return state;
    }

    public bool StateChanged() {

        if (state0 is null) {
            return true;
        }

        if (state.depth != state0.depth || state.range != state0.range || state.nrOfWaterplanes != state0.nrOfWaterplanes || ! state.position.Equals(state0.position)) {
            Debug.Log("Change in world");
            return true;
        }

        return false;
    }

    public void AddSource(Camera test) {
        //Debug.Log("Added source");
        this.sourceSphere = test;

        Debug.Log("Applying. Please wait");

        SimpleSourceController controller = test.GetComponent<SimpleSourceController>();    
        
        controller.upper_limit_x = range/2;
        controller.lower_limit_x = -range/2;
        controller.upper_limit_y = 0;
        controller.lower_limit_y = -waterDepth;
        controller.upper_limit_z = range/2;
        controller.lower_limit_z = -range/2;
        controller.JumpTo(Vector3.down * sourceDepth);
    }

    public void AddSurface(GameObject _surface) 
    {
        Mesh m1 = DoublePlaneMesh(Vector3.zero);

        _surface.GetComponent<MeshFilter>().mesh = m1;
        
        this.surface = _surface;
    }
    public void AddBottom(GameObject bottom) 
    {
        Vector3 center = Vector3.down * waterDepth;
        Mesh m1 = DoublePlaneMesh(center);

        bottom.GetComponent<MeshFilter>().mesh = m1;
        
        this.bottom = bottom;
    }

    public void AddWaterplane(GameObject waterplane) 
    {
        // Create one waterplane, more waterplanes (if wanted) are created in main when the waterplane is added to the raytracing acceleration structure
        float dx = waterDepth / (nrOfWaterplanes + 1);
                
        Vector3 center = Vector3.down * dx;
        Mesh m1 = SinglePlaneMesh(center);

        waterplane.GetComponent<MeshFilter>().mesh = m1;

        // make plane invisible in the normal scene
        Color temp = waterplane.GetComponent<MeshRenderer>().material.color;
        temp.a = 0;
        waterplane.GetComponent<MeshRenderer>().material.color = temp;
    }

    private Vector3 mean(Vector3[] vectors) 
    {        
        int n_vec = vectors.Length;

        Vector3 sum = Vector3.zero;

        foreach (Vector3 vec in vectors) { 
            sum += vec;
        }
        
        return sum / n_vec;
    }

    private Mesh DoublePlaneMesh(Vector3 center)
    {
        Mesh surfaceMesh = new Mesh()
        {
            name = "Plane Mesh"
        };

        Vector3[] square = new Vector3[] {
            Vector3.zero, new Vector3(range, 0f, 0f), new Vector3(0f, 0f, range), new Vector3(range, 0f, range), // Upper plane
            Vector3.zero, new Vector3(range, 0f, 0f), new Vector3(0f, 0f, range), new Vector3(range, 0f, range) // lower plane
        };
        Vector3 center_local = mean(square);
        square = square.Select(x => { return x - center_local + center; }).ToArray();

        surfaceMesh.vertices = square;

        surfaceMesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back,

            Vector3.forward, 
            Vector3.forward, 
            Vector3.forward, 
            Vector3.forward,
        };

        surfaceMesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f)
        };
        
        surfaceMesh.triangles = new int[] {
            0, 2, 1, 1, 2, 3,
            4, 5, 6, 5, 7, 6,
        };

        return surfaceMesh;
    }

    private Mesh SinglePlaneMesh(Vector3 center)
    {
        Mesh surfaceMesh = new Mesh()
        {
            name = "Plane Mesh"
        };

        Vector3[] square = new Vector3[] {
            Vector3.zero, new Vector3(range, 0f, 0f), new Vector3(0f, 0f, range), new Vector3(range, 0f, range), // Upper plane
        };
        Vector3 center_local = mean(square);
        square = square.Select(x => { return x - center_local + center; }).ToArray();

        surfaceMesh.vertices = square;

        surfaceMesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back,
        };

        surfaceMesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
        };

        surfaceMesh.triangles = new int[] {
            0, 2, 1, 1, 2, 3,
        };

        return surfaceMesh;
    }

    void Update()
    {
        state0 = state;
        state = getCurrentState();        

        Vector3 worldPos = this.transform.position;     
        


        // Ensure planes stay at the same location
        //SetPlaneDepthStationary(this.surface, 0);
        //SetPlaneDepthStationary(this.bottom, this.waterDepth);



        //Vector3 newPos = this.transform.position;
        //newPos.y = worldPos.y;
        //this.transform.position = newPos;



        //Debug.Log(sourceDept2);

        //SetPlaneDepthStationary(surface, 0);
        //for (int i = 0; i<waterLayers.Length; i++)
        //{

        //  SetPlaneDepthStationary(waterLayers[i], waterLayerDepths[i]);

        //}
        //SetPlaneDepthStationary(bottom, waterDepth);

        /*
        if (sourceDept2 > 0 )
        {

            Vector3 p = sourceSphere.transform.position;
            p.y = 0;
            sourceSphere.transform.position = p;
        //    this.transform.position = p;
        }

        if (sourceDept2 < -waterDepth) {

            Vector3 p = sourceSphere.transform.position;
            p.y = -waterDepth;
            sourceSphere.transform.position = p;
          //  this.transform.position = p;
        }        
        */
    }

    public int GetNrOfWaterplanes()
    {
        return nrOfWaterplanes;
    }

    public float GetWaterDepth()
    {
        return waterDepth;
    }
}
