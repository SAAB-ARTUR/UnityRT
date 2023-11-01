using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTemplateProjects;

public class World : MonoBehaviour
{
    [SerializeField] Material lineMaterial = null;
    [SerializeField] float sourceDepth = 0;

    [SerializeField] GameObject surface = null;
    [SerializeField] GameObject bottom = null;
    public Vector3 pyramidTop = Vector3.zero;
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

    public void AddCustomBottom(Mesh mesh)
    {
        foreach (LineRenderer line in pyramidlines) // delete lines from previous runs
        {
            Destroy(line.gameObject);
        }
        pyramidlines.Clear();

        // set the mesh
        bottom.GetComponent<MeshFilter>().mesh = mesh;

        //shift the custom mesh down to the correct depth
        Vector3[] vertices = mesh.vertices;
        Vector3 downshift = Vector3.up * waterDepth;
        vertices = vertices.Select(x => { return x + downshift; }).ToArray();
        bottom.GetComponent<MeshFilter>().mesh.vertices = vertices;

        Mesh customMesh = bottom.GetComponent<MeshFilter>().mesh;

        for (int i = 2; i < mesh.triangles.Length; i += 3)
        {
            line = new GameObject("Line").AddComponent<LineRenderer>();
            line.startWidth = 0.03f;
            line.endWidth = 0.03f;
            line.useWorldSpace = true;

            Vector3[] positions = new Vector3[4] { customMesh.vertices[customMesh.triangles[i]], customMesh.vertices[customMesh.triangles[i - 1]], customMesh.vertices[customMesh.triangles[i - 2]], customMesh.vertices[customMesh.triangles[i]] };
            line.positionCount = 4;
            line.SetPositions(positions);

            line.material = lineMaterial;
            line.material.color = Color.black;

            pyramidlines.Add(line);
        }
    }

    public void AddCustomSurface() // not actually a custom surface, it just matches the size of bottom and creates a flat plane from the dimensions of the bottom
    {
        Mesh mesh = new Mesh()
        {
            name = "Plane Mesh"
        };

        // take the final 6 vertices and use them to create a surface of the same dimensions (definition of the bottom vertices is in STLFileReader.cs)
        Vector3[] vertices = bottom.GetComponent<MeshFilter>().mesh.vertices.Skip(bottom.GetComponent<MeshFilter>().mesh.vertexCount - 6).Take(6).ToArray();
        Vector3 upshift = Vector3.up * waterDepth;

        // shift the vertices up to 0 depth
        vertices = vertices.Select(x => { return x - upshift; }).ToArray();

        mesh.vertices = vertices;
        mesh.triangles = Enumerable.Range(0, vertices.Length).ToArray();

        mesh.normals = Enumerable.Repeat(Vector3.back, mesh.vertexCount).ToArray();
        mesh.tangents = Enumerable.Repeat(new Vector4(1f, 0f, 0f, -1f), mesh.vertexCount).ToArray();
    }


    public void AddPlaneBottom() 
    {
        foreach (LineRenderer line in pyramidlines) // delete lines from previous runs
        {
            Destroy(line.gameObject);
        }
        pyramidlines.Clear();
        Vector3 center = Vector3.up * waterDepth;        

        Mesh m1 = DoublePlaneMesh(center);
        bottom.GetComponent<MeshFilter>().mesh = m1;
    }

    public void AddPlaneSurface()
    {
        // surface is flat for now, but it should be the same size as bottom

        Mesh m1 = DoublePlaneMesh(Vector3.zero);

        surface.GetComponent<MeshFilter>().mesh = m1;

        //surface = _surface;
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

    public GameObject GetSurface()
    {
        return surface;
    }

    public GameObject GetBottom()
    {
        return bottom;
    }
}
