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
    
    [SerializeField] float waterDepth = 0;
    [SerializeField] float sourceDepth = 0;
    [SerializeField] float range = 0;
    [SerializeField] int nrOfWaterPlanes = 0;
    //[SerializeField] Material waterplaneMaterial;

    private class State {

        public float depth;
        public float range;
        public int nrOfWaterPlanes;
        public Vector3 position;
    }

    private State state0 = null;
    private State state;
    
    private float[] waterLayerDepths = { };




    private Camera sourceSphere;
    private GameObject surface;
    private GameObject bottom;
    private List<GameObject> waterLayers;

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

    public void AddSource(Camera test) {
        //Debug.Log("Added source");
        this.sourceSphere = test;

        Debug.Log("Applying. Please wait");

        SimpleSourceController controller = test.GetComponent<SimpleSourceController>();    
        controller.upper_limit_y = 0;
        controller.lower_limit_y = -waterDepth;
        //this.sourceSphere.transform.parent = this.transform;

        //this.sourceSphere.transform.localPosition = Vector3.down * sourceDepth;
    }

    public void AddSurface(GameObject _surface) {
        // TODO
        Mesh m1 = PlaneMesh(Vector3.zero);

        _surface.GetComponent<MeshFilter>().mesh = m1;

        //_surface.transform.parent = this.transform;
        this.surface = _surface;
    }
    public void AddBottom(GameObject bottom) {
        Vector3 center = Vector3.down * waterDepth;
        Mesh m1 = PlaneMesh(center);

        bottom.GetComponent<MeshFilter>().mesh = m1;
        //bottom.transform.parent = this.transform;    
        //bottom.transform.localPosition = center;
        
        this.bottom = bottom;
    }

    public void AddWaterplane(GameObject waterplane) {

        // Figure out the depths of the layers
        float dx = waterDepth / (nrOfWaterPlanes + 1);
        //List<float> waterPlanesDepths = new List<float>();
                
        Vector3 center = Vector3.down * dx;
        Mesh m1 = PlaneMesh(center);

        waterplane.GetComponent<MeshFilter>().mesh = m1;
        //waterplane.transform.parent = this.transform;

        Color temp = waterplane.GetComponent<MeshRenderer>().material.color;
        temp.a = 0;
        waterplane.GetComponent<MeshRenderer>().material.color = temp;

        //GameObject wl = Instantiate(_waterLayerParent);
        //DualPlane dp = wl.GetComponent<DualPlane>();
        //dp.topMesh.GetComponent<MeshFilter>().mesh = m1;
        //dp.bottomMesh.GetComponent<MeshFilter>().mesh = m2;

        //dp.transform.parent = this.transform;
        //dp.transform.position = center;

        //waterPlanes.Add(dp);
        //waterPlanesDepths.Add(dx * i);

        //waterLayers = waterPlanes.ToArray();
        //waterLayerDepths = waterPlanesDepths.ToArray();

        this.waterLayers.Add(waterplane);

    }

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
   
    // Update is called once per framuie

    /*
    void SetPlaneDepthStationary(GameObject dp, float depth) {
        Vector3 pos = sourceSphere.transform.position;
        pos.y = -depth;
        dp.transform.position = pos;
        dp.transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    */

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
        return nrOfWaterPlanes;
    }

    public float GetWaterDepth()
    {
        return waterDepth;
    }
}
