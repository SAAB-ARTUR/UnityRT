using System.Linq;
using UnityEngine;
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
    
    private float waterDepth = 0.0f;

    private int nrOfWaterplanes = 0;

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
            return true;
        }

        return false;
    }

    public void AddSource(Camera test) {        
        this.sourceSphere = test;

        SimpleSourceController controller = test.GetComponent<SimpleSourceController>();    
        
        controller.upper_limit_x = range/2;
        controller.lower_limit_x = -range/2;
        controller.upper_limit_y = 0;
        controller.lower_limit_y = waterDepth;
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
        Vector3 center = Vector3.up * waterDepth;
        Mesh m1 = DoublePlaneMesh(center);

        bottom.GetComponent<MeshFilter>().mesh = m1;
        
        this.bottom = bottom;
    }

    public void AddWaterplane(GameObject waterplane) 
    {
        // Create one waterplane, more waterplanes (if wanted) are created in main when the waterplane is added to the raytracing acceleration structure
        float dx = waterDepth / (nrOfWaterplanes + 1);
                
        Vector3 center = Vector3.up * dx;
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
    }

    public int GetNrOfWaterplanes()
    {
        return nrOfWaterplanes;
    }

    public void SetNrOfWaterplanes(int value)
    {
        nrOfWaterplanes = value;
    }

    public float GetWaterDepth()
    {
        return waterDepth;
    }

    public void SetWaterDepth(float depth)
    {
        waterDepth = depth;
    }
}
