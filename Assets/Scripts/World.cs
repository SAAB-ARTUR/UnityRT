using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class World : MonoBehaviour
{
    
    [SerializeField] float waterDepth = 0;
    [SerializeField] float sourceDepth = 0;
    [SerializeField] float range = 0;
    [SerializeField] int nrOfWaterPlanes = 0;

    private class State {

        public float depth;
        public float range;
        public int nrOfWaterPlanes;
        public Vector3 position;

    }

    private State state0;
    private State state;

    private DualPlane surface;
    private DualPlane bottom;
    private DualPlane[] waterLayers = { };
    private float[] waterLayerDepths = { };


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

    public void AddWaterLayers(GameObject _waterLayerParent) {

        // Figure out the depths of the layers
        float dx = waterDepth / nrOfWaterPlanes;
        DualPlane parentWaterLayer = _waterLayerParent.GetComponent<DualPlane>();

        List<DualPlane> waterPlanes = new List<DualPlane>();
        List<float> waterPlanesDepths = new List<float>();

        for (int i = 0; i < nrOfWaterPlanes; i++)
        {

            Vector3 center = state.position + Vector3.down * dx * i;
            Mesh m1 = PlaneMesh(Vector3.zero, false);
            Mesh m2 = PlaneMesh(Vector3.zero, true);

            GameObject wl = Instantiate(_waterLayerParent);
            DualPlane dp = wl.GetComponent<DualPlane>();
            dp.topMesh.GetComponent<MeshFilter>().mesh = m1;
            dp.bottomMesh.GetComponent<MeshFilter>().mesh = m2;

            dp.transform.parent = this.transform;
            dp.transform.position = center;

            waterPlanes.Add(dp);
            waterPlanesDepths.Add(dx * i);

        }

        waterLayers = waterPlanes.ToArray();
        waterLayerDepths = waterPlanesDepths.ToArray();

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

    void SetPlaneDepthStationary(DualPlane dp, float depth) {
        Vector3 pos = dp.transform.position;
        pos.y = -depth;
        dp.transform.position = pos;
        dp.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void Update()
    {

        this.transform.position = sourceSphere.transform.position;
        float sourceDept2 = this.transform.position.y;

        SetPlaneDepthStationary(surface, 0);
        for (int i = 0; i<waterLayers.Length; i++)
        {

            SetPlaneDepthStationary(waterLayers[i], waterLayerDepths[i]);

        }
        SetPlaneDepthStationary(bottom, waterDepth);
        




        if (sourceDept2 > 0 )
        {

            Vector3 p = sourceSphere.transform.position;
            p.y = 0;
            sourceSphere.transform.position = p;
        }

        if (sourceDept2 < -waterDepth) {

            Vector3 p = sourceSphere.transform.position;
            p.y = -waterDepth;
            sourceSphere.transform.position = p;

        }


        
    }
}
