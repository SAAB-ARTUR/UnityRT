using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DualPlane : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject topMesh = null;
    public GameObject bottomMesh = null;
    void Start()
    {   
        //Mesh topMeshMesh = topMesh.GetComponent<MeshFilter>().mesh.Clone<Mesh>();
        //this.GetComponent<MeshFilter>.sharedMesh = topMeshMesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
