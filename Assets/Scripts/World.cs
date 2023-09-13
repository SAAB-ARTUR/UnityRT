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

    private State state0 = null;
    private State state;
    
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

    State getCurrentState() 
    {
        state = new State();
        // Set initial state
        state.depth = waterDepth;
        state.range = range;
        state.nrOfWaterPlanes = nrOfWaterPlanes;
        state.position = this.transform.position;

        return state;
    }

    public bool StateChanged() {

        if (state0 is null) {
            return true;
        }

        if (state.depth != state0.depth || state.range != state0.range || state.nrOfWaterPlanes != state0.nrOfWaterPlanes || ! state.position.Equals(state0.position)) {
            Debug.Log("Change in world");
            return true;

        }

        return false;
    }

    public void AddSource(GameObject test) {
        //Debug.Log("Added source");
        this.sourceSphere = test;
        this.sourceSphere.transform.parent = this.transform;

        this.sourceSphere.transform.localPosition = Vector3.down * sourceDepth;
    }

    public void AddSurface(GameObject surface) {
        // TODO
        Mesh m1 = PlaneMesh(Vector3.zero);

        surface.GetComponent<MeshFilter>().mesh = m1;

        surface.transform.parent = this.transform;
    }
    public void AddBottom(GameObject bottom) {
        Vector3 center = state.position + Vector3.down * waterDepth;
        Mesh m1 = PlaneMesh(center);

        bottom.GetComponent<MeshFilter>().mesh = m1;
        bottom.transform.parent = this.transform;
        bottom.transform.localPosition = center;
    }

    /*public void AddWaterLayers(GameObject _waterLayerParent) {

        // Figure out the depths of the layers
        float dx = waterDepth / nrOfWaterPlanes;
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

    }*/

    private Vector3 mean(Vector3[] vectors) { 
        
        int n_vec = vectors.Length;

        Vector3 sum = Vector3.zero;

        foreach (Vector3 vec in vectors) { 
            sum += vec;
        }
        
        return sum / n_vec;
    }

    private Mesh PlaneMesh(Vector3 center)
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
        
        surfaceMesh.triangles = new int[] {
            0, 2, 1, 1, 2, 3
        };

        return surfaceMesh;
    }

    private Mesh SeafloorPlaneMesh(Vector3 center)
    {
        Mesh surfaceMesh = new Mesh()
        {
            name = "Plane Mesh"
        };

        Vector3[] square = new Vector3[] {
            new Vector3(0, -waterDepth, 0), new Vector3(range, -waterDepth, 0f), new Vector3(0f, -waterDepth, range), new Vector3(range, -waterDepth, range)
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

        surfaceMesh.triangles = new int[] {
            0, 2, 1, 1, 2, 3
        };

        return surfaceMesh;
    }

    // Update is called once per framuie

    /*void SetPlaneDepthStationary(DualPlane dp, float depth) {
        Vector3 pos = dp.transform.position;
        pos.y = -depth;
        dp.transform.position = pos;
        dp.transform.rotation = Quaternion.Euler(0, 0, 0);
    }*/

    void Update()
    {
        state0 = state;
        state = getCurrentState();        

        Vector3 worldPos = this.transform.position;

        this.transform.position = sourceSphere.transform.position;
        Vector3 newPos = this.transform.position;
        newPos.y = worldPos.y;
        this.transform.position = newPos;


        float sourceDept2 = sourceSphere.transform.position.y;
        //Debug.Log(sourceDept2);

        //SetPlaneDepthStationary(surface, 0);
        //for (int i = 0; i<waterLayers.Length; i++)
        //{

          //  SetPlaneDepthStationary(waterLayers[i], waterLayerDepths[i]);

        //}
        //SetPlaneDepthStationary(bottom, waterDepth);

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
    }
}
