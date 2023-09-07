using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class World : MonoBehaviour
{
    
    [SerializeField] int waterDepth = 0;
    [SerializeField] int sourceDepth = 0;
    [SerializeField] int range = 0;
    [SerializeField] int nrOfWaterPlanes = 0;

    private class State {

        public int depth;
        public int range;
        public int nrOfWaterPlanes;
        public Vector3 position;

    }

    private State state0;
    private State state;

    private DualPlane surface;
    private DualPlane bottom;


    private GameObject sourceSphere;

    // Start is called before the first frame update
    void Start()
    {
        state = new State();
        // Set initial state
        state.depth = waterDepth;
        state.range = range;
        state.nrOfWaterPlanes = nrOfWaterPlanes;
        state.position = Vector3.zero;

       

    }

    public void AddSource(GameObject test) {
        //Debug.Log("Added source");
        this.sourceSphere = test;
        this.sourceSphere.transform.parent = this.transform;

        this.sourceSphere.transform.localPosition = Vector3.down * sourceDepth;

        //test.transform.parent = this.gameObject.transform;
        //Debug.Log(test.transform.position.ToString());
        //this.sourceSphere.transform.position = Vector3.zero + Vector3.up * test.transform.position.y;
        //Debug.Log(test.transform.position.ToString());
        //Debug.Log(test.ToString());
        //state.position = this.transform.position;
    }

    public void AddSurface(GameObject _surface) {
        // TODO
        surface = _surface.GetComponent<DualPlane>();
        Mesh m1 = PlaneMesh(Vector3.zero, false);
        Mesh m2 = PlaneMesh(Vector3.zero, true);

        surface.topMesh.GetComponent<MeshFilter>().mesh = m1;
        surface.bottomMesh.GetComponent<MeshFilter>().mesh = m2;

        surface.transform.parent = this.transform;
    }
    public void AddBottom(GameObject _bottom) { 
        bottom = _bottom.GetComponent<DualPlane>();
        Vector3 center = state.position + Vector3.down * waterDepth;
        Mesh m1 = PlaneMesh(Vector3.zero, false);
        Mesh m2 = PlaneMesh(Vector3.zero, true);

        bottom.topMesh.GetComponent<MeshFilter>().mesh = m1;
        bottom.bottomMesh.GetComponent<MeshFilter>().mesh = m2;
        bottom.transform.parent = this.transform;
        bottom.transform.localPosition = center;
    }

    private Vector3 mean(Vector3[] vectors) { 
        
        int n_vec = vectors.Length;

        Vector3 sum = Vector3.zero;

        foreach (Vector3 vec in vectors) { 
            sum += vec;
        }
        
        return sum / n_vec;

    }

    private Mesh PlaneMesh(Vector3 center, bool flipped = false)
    {
        Mesh surfaceMesh = new Mesh()
        {
            name = "Plane Mesh"
        };



        Vector3[] square = new Vector3[] {
            Vector3.zero, new Vector3(range, 0f, 0f), new Vector3(0f, 0f, range), new Vector3(range, 0f, range)
        };
        Vector3 center_local = mean(square);
        square = square.Select(x => { return x - center_local + center; }).ToArray();

        surfaceMesh.vertices = square;

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

    // Update is called once per framuie
    void Update()
    {
        
    }
}
