using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTemplateProjects;

public class World : MonoBehaviour
{
    [SerializeField] Material lineMaterial = null;
    [SerializeField] float sourceDepth = 0;
    public Vector3 pyramidTop = Vector3.zero;
    //[SerializeField] GameObject srcSphere = null;
    private LineRenderer line = null;
    private List<LineRenderer> pyramidlines = new List<LineRenderer>();

    public int range
    {
        get { return _range; }
        set
        {
            if (value > 0) 
            { 
                _range = value;
                changeInWorld = true;
            }
            else 
            { 
                _range = 0;
                changeInWorld = true;
            }
        }
    }

    [SerializeField]
    private int _range;    
    
    private float waterDepth = 0.0f;

    private int nrOfWaterplanes = 0;

    private Camera sourceSphere;
    private GameObject surface;
    private GameObject bottom;

    private bool changeInWorld = false;

    public struct Target
    {
        public float xpos;
        public float ypos;
        public float zpos;
        public float phi; // angle from source to target

        public Target(float xpos, float ypos, float zpos, float srcX, float srcZ)
        {
            this.xpos = xpos;
            this.ypos = ypos;
            this.zpos = zpos;

            float xdiff = xpos - srcX;
            float zdiff = zpos - srcZ;
            this.phi = MathF.Atan2(zdiff, xdiff);
        }        
    }

    private bool targetChange = false;

    public int GetDataSizeOfTarget()
    {
        return 4 * sizeof(float);
    }

    [SerializeField] GameObject target;
    private List<GameObject> targets = new List<GameObject>();
    private List<Target> targetStructs = new List<Target>();

    private int nrOfTargets = 1;

    public bool WorldHasChanged()
    {
        /*if (changeInWorld)
        {
            Debug.Log("change");
        }*/
        return changeInWorld;
    }

    public void AckChangeInWorld()
    {
        changeInWorld = false;
    }

    public void AddSource(Camera c) {
        sourceSphere = c;

        SimpleSourceController controller = c.GetComponent<SimpleSourceController>();
        
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
        
        surface = _surface;
    }
    public void AddBottom(GameObject _bottom) 
    {
        foreach (LineRenderer line in pyramidlines) // delete lines from previous runs
        {
            Destroy(line.gameObject);
        }
        pyramidlines.Clear();
        Vector3 center = Vector3.up * waterDepth;
        //Mesh m1 = DoublePlaneMesh(center);
        Mesh m1 = BottomPyramid(center, pyramidTop);

        _bottom.GetComponent<MeshFilter>().mesh = m1;
        
        bottom = _bottom;

        for (int i = 2; i < m1.triangles.Length; i += 3)
        {
            line = new GameObject("Line").AddComponent<LineRenderer>();
            line.startWidth = 0.03f;
            line.endWidth = 0.03f;
            line.useWorldSpace = true;

            Vector3[] positions = new Vector3[4] { m1.vertices[m1.triangles[i]], m1.vertices[m1.triangles[i - 1]], m1.vertices[m1.triangles[i - 2]], m1.vertices[m1.triangles[i]] };
            line.positionCount = 4;
            line.SetPositions(positions);

            line.material = lineMaterial;
            line.material.color = Color.black;

            pyramidlines.Add(line);
        }
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
        Mesh mesh = new Mesh()
        {
            name = "Plane Mesh"
        };

        Vector3[] square = new Vector3[] {
            Vector3.zero, new Vector3(range, 0f, 0f), new Vector3(0f, 0f, range), new Vector3(range, 0f, range),             
        };
        Vector3 center_local = mean(square);
        square = square.Select(x => { return x - center_local + center; }).ToArray();

        mesh.vertices = square;

        mesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back,
        };

        mesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
        };
        
        mesh.triangles = new int[] {
            0, 2, 1, 1, 2, 3, // upper plane
            0, 1, 2, 1, 3, 2, // lower plane
        };

        return mesh;
    }

    private Mesh SinglePlaneMesh(Vector3 center)
    {
        Mesh mesh = new Mesh()
        {
            name = "Plane Mesh"
        };

        Vector3[] square = new Vector3[] {
            Vector3.zero, new Vector3(range, 0f, 0f), new Vector3(0f, 0f, range), new Vector3(range, 0f, range), // Upper plane
        };
        Vector3 center_local = mean(square);
        square = square.Select(x => { return x - center_local + center; }).ToArray();

        mesh.vertices = square;

        mesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back,
        };

        mesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
        };

        mesh.triangles = new int[] {
            0, 2, 1, 1, 2, 3,
        };

        return mesh;
    }

    private Mesh BottomPyramid(Vector3 center, Vector3 topPoint)
    {
        Mesh mesh = new Mesh();
        Vector3[] pyramid = new Vector3[] // create vertices for 4 triangles and combine them into a pyramid
        {
            new Vector3(-range/2, waterDepth, -range/2), new Vector3(-range/2, waterDepth, range/2), new Vector3(range/2, waterDepth, range/2), new Vector3(range/2, waterDepth, -range/2),
            topPoint// top position
        };

        mesh.vertices = pyramid;

        mesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back, Vector3.back
        };

        mesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
        };

        mesh.triangles = new int[] {
            0, 1, 4, // pyramid
            1, 2, 4, 
            3, 4, 2,
            0, 4, 3,
            0, 2, 1, 0, 3, 2, // seafloor 
        };

        return mesh;
    }

    public int GetNrOfWaterplanes()
    {
        return nrOfWaterplanes;
    }

    public void SetNrOfWaterplanes(int value)
    {
        nrOfWaterplanes = value;
        changeInWorld = true;
    }

    public float GetWaterDepth()
    {
        return waterDepth;
    }

    public void SetWaterDepth(float depth)
    {
        waterDepth = depth;
        changeInWorld = true;
    }

    public bool CreateTargets(List<float> targetCoords)
    {         
        // try tp create the new targets, everything needs to be successful for a change to take place
        List<GameObject> tempTargets = new List<GameObject>();        

        for(int i = 3; i < targetCoords.Count; i+=3)
        {
            if (targetCoords[i] <= range / 2 && targetCoords[i] >= -range / 2 && targetCoords[i+2] <= range / 2 && targetCoords[i+2] >= -range / 2 && targetCoords[i+1] <= 0 && targetCoords[i+1] >= waterDepth)
            {
                GameObject temp = Instantiate(target, new Vector3(targetCoords[i], targetCoords[i + 1], targetCoords[i + 2]), Quaternion.identity); // create a new target
                tempTargets.Add(temp);
            }
            else
            {                
                return false; // target out of volume, abort changes and keep old targets
            }            
        }

        // first target position denotes the 'main' target
        if (targetCoords[0] <= range / 2 && targetCoords[0] >= -range / 2 && targetCoords[2] <= range / 2 && targetCoords[2] >= -range / 2 && targetCoords[1] <= 0 && targetCoords[1] >= waterDepth)
        {
            target.transform.position = new Vector3(targetCoords[0], targetCoords[1], targetCoords[2]);
        }
        else
        {
            return false; // target out of volume, abort changes and keep old targets
        }

        // target creation successful

        foreach (GameObject t in targets) // destroy old targets
        {
            Destroy(t);
        }
        targets.Clear(); // clear the list and fill it with new targets

        foreach (GameObject t in tempTargets) // add the new targets to the target list
        {
            targets.Add(t);
        }

        nrOfTargets = 1 + targets.Count;
        targetChange = true;

        return true; // target creation successful
    }

    public int GetNrOfTargets()
    {
        return nrOfTargets;
    }

    public List<Target> GetTargets(float srcX, float srcZ)
    {
        targetStructs.Clear();
        // add the original target to the list
        Target ta_orig = new Target(target.transform.position.x, target.transform.position.y, target.transform.position.z, srcX, srcZ);
        targetStructs.Add(ta_orig);

        foreach (GameObject t in targets) // create structs of the target gameobjects
        {
            Target ta = new Target(t.transform.position.x, t.transform.position.y, t.transform.position.z, srcX, srcZ);
            targetStructs.Add(ta);
        }        

        return targetStructs;
    }

    public Vector3 GetMainTargetPosition()
    {
        return target.transform.position;
    }

    public bool TargetHasChanged()
    {
        return targetChange;
    }

    public void AckTargetChange()
    {
        targetChange = false;
    }

    public GameObject GetTarget()
    {
        return target;
    }
}
