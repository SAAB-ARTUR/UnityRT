using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AnotherFileBrowser.Windows; // https://github.com/SrejonKhan/AnotherFileBrowser Used to open file explorer to be able to select a file.

public class STLFileReader : MonoBehaviour
{
    private bool filepathHasChanged = false;
    private Mesh bottomMesh = null;
    private List<Vector3> meshNormals = null;

    public void OnBathymetryButtonPress()
    {
        BrowserProperties bp = new BrowserProperties();
        bp.initialDir = Directory.GetCurrentDirectory() + "\\Assets\\Bathymetry";
        bp.filter = "stl files (*.stl)|*.stl|All Files (*.*)|*.*";
        bp.filterIndex = 0;
        new FileBrowser().OpenFileBrowser(bp, path =>
        {
            if (path != "")
            {
                string filename = path.Split("\\").Last(); // print only filename and extension on the button, not the entire path

                GameObject.Find("Button - Filepicker_Bathymetry").GetComponentInChildren<Text>().text = filename;
                filepathHasChanged = true;
                ReadSTLFile(path);
            }
        });
    }

    private void ReadSTLFile(string filepath)
    {
        try
        {
            StreamReader sr = new StreamReader(filepath);
            string line = sr.ReadToEnd(); // really inefficient way to find out if the file is in binary or ascii format
            if (line != null)
            {
                if (line.Contains("facet"))
                {
                    // ascii file
                    ReadAsciiSTLFile(filepath);
                }
                else
                {
                    //binary file
                    ReadBinarySTLFile(filepath);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Exception: " + e.Message);
        }
    }

    private void ReadBinarySTLFile(string filepath)
    {
        bottomMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        meshNormals = new List<Vector3>();
        
        try
        {
            // In order to read a binary format, a BinaryReader must be used. BinaryReader itself
            // is not thread safe. To make it so, a locker object and lock() must be used.
            object locker = new object();
            lock (locker)
            {
                using (BinaryReader br = new BinaryReader(File.Open(filepath, FileMode.Open)))
                {
                    // Read header info
                    byte[] header = br.ReadBytes(80);
                    byte[] length = br.ReadBytes(4);
                    int numberOfSurfaces = BitConverter.ToInt32(length, 0);
                    string headerInfo = System.Text.Encoding.UTF8.GetString(header, 0, header.Length).Trim();                    

                    // Read Data
                    byte[] block;
                    int surfCount = 0;

                    // Read from the file until either there is no data left or 
                    // the number of surfaces read is equal to the number of surfaces in the
                    // file. This can prevent reading a partial block at the end and getting
                    // out of range execptions.
                    while ((block = br.ReadBytes(50)) != null && surfCount++ < numberOfSurfaces)
                    {
                        // Declare temp containers                        
                        List<Vector3> verts = new List<Vector3>();
                        byte[] xdata = new byte[4];
                        byte[] ydata = new byte[4];
                        byte[] zdata = new byte[4];

                        // Parse data block
                        for (int i = 0; i < 4; i++)
                        {
                            for (int k = 0; k < 12; k++)
                            {
                                int index = k + i * 12;

                                if (k < 4)
                                {
                                    // xComp
                                    xdata[k] = block[index];
                                }
                                else if (k < 8)
                                {
                                    // yComp
                                    ydata[k - 4] = block[index];
                                }
                                else
                                {
                                    // zComp
                                    zdata[k - 8] = block[index];
                                }
                            }
                            // Convert data to useable structures
                            float x = BitConverter.ToSingle(xdata, 0);
                            float y = BitConverter.ToSingle(ydata, 0);
                            float z = BitConverter.ToSingle(zdata, 0);                            

                            if (i == 0)
                            {
                                // This is a normal                                
                                meshNormals.Add(new Vector3(x, y, z));
                            }
                            else
                            {
                                // This is a vertex
                                vertices.Add(new Vector3(x, y, z));
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)  // This is too general to be the only catch statement.
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);        
        }

        bottomMesh.vertices = vertices.ToArray();
        bottomMesh.triangles = Enumerable.Range(0, vertices.Count).ToArray();
    }

    private void ReadAsciiSTLFile(string filepath)
    {
        // based on https://en.wikipedia.org/wiki/STL_(file_format)        
        bottomMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();        
        meshNormals = new List<Vector3>();

        string line;

        // keep track of these
        float smallestX = 0;
        float largestX = 0;
        float smallestZ = 0;
        float largestZ = 0;

        try
        {
            StreamReader sr = new StreamReader(filepath);
            line = sr.ReadLine();
            

            while (line != null) // read lines until the end and add the values to the SSP-list
            {                
                //do shit
                if (line.Contains("solid") || line.Contains("outer loop") || line.Contains("endloop") || line.Contains("endfacet"))
                {
                    line = sr.ReadLine(); // read new line
                    continue;
                }
                if (line.Contains("endsolid"))
                {
                    break; // end of file
                }
                if (line.Contains("facet") && !line.Contains("endfacet")) // new triangle, normal is defined on this line 
                {
                    string[] items = line.Split();
                    float[] n = new float[3];
                    for (int i = 2; i < items.Length; i++) // line format: facet normal n_i n_j n_k
                    {
                        bool isNumber = float.TryParse(items[i], out float numericValue);
                        if (isNumber)
                        {
                            n[i - 2] = numericValue;
                        }                        
                    }
                    // create normal
                    meshNormals.Add(new Vector3(n[0], n[1], n[2]));

                }
                else if (line.Contains("vertex")) // start of vertex definitions
                {
                    string[] items = line.Split();
                    float[] v = new float[3];
                    int j = 0;
                    for (int i = 1; i < items.Length; i++) // line format: facet normal n_i n_j n_k
                    {        
                        bool isNumber = float.TryParse(items[i], out float numericValue);
                        if (isNumber)
                        {
                            v[j] = numericValue;
                            j++;
                        }
                    }
                    // create vertex
                    vertices.Add(new Vector3(v[0], v[1], v[2]));
                    if (v[0] < smallestX)
                    {
                        smallestX = v[0];
                    }
                    else if(v[0] > largestX)
                    {
                        largestX = v[0];
                    }
                    if (v[2] < smallestZ)
                    {
                        smallestZ = v[2];
                    }
                    else if (v[2] > largestZ)
                    {
                        largestZ = v[2];
                    }
                }

                line = sr.ReadLine();
            }
            sr.Close(); // close file
        }
        catch (Exception e)
        {
            Debug.Log("Exception: " + e.Message);
        }

        Debug.Log(meshNormals.Count);

        // create more vertices to add a bottom to the seafloor mesh (for visual purposes only), these get no normals since they are not supposed to be in the rtas
        Vector3[] bottom = new Vector3[]
        {
            new Vector3(smallestX, 0, smallestZ), new Vector3(smallestX, 0, largestZ), new Vector3(largestX, 0, smallestZ),
            new Vector3(largestX, 0, smallestZ), new Vector3(smallestX, 0, largestZ), new Vector3(largestX, 0, largestZ),

            new Vector3(smallestX, 0, smallestZ), new Vector3(largestX, 0, smallestZ), new Vector3(smallestX, 0, largestZ),
            new Vector3(largestX, 0, smallestZ), new Vector3(largestX, 0, largestZ), new Vector3(smallestX, 0, largestZ),
        };
        vertices.AddRange(bottom);       
        
        bottomMesh.vertices = vertices.ToArray();
        bottomMesh.triangles = Enumerable.Range(0, vertices.Count).ToArray();

        // normals and tangents for the mesh vertices seem to be for visual purposes
        bottomMesh.normals = Enumerable.Repeat(Vector3.back, bottomMesh.vertexCount).ToArray();
        bottomMesh.tangents = Enumerable.Repeat(new Vector4(1f, 0f, 0f, -1f), bottomMesh.vertexCount).ToArray();
    }

    public bool BathymetryFileHasChanged()
    {
        return filepathHasChanged;
    }

    public void AckBathymetryFileHasChanged() // Main.cs calls this function to acknowledge the change
    {
        if (filepathHasChanged) // only update if a change has happened
        {
            filepathHasChanged = false;
        }
    }

    public List<Vector3> GetBottomMeshNormals()
    {
        return meshNormals;
    }

    public Mesh GetBottomMesh()
    {
        return bottomMesh;
    }

}



