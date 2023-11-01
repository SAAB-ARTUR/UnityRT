using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AnotherFileBrowser.Windows; // https://github.com/SrejonKhan/AnotherFileBrowser Used to open file explorer to be able to select a file.


public class SSPFileReader : MonoBehaviour
{    
    private bool filepathHasChanged = false;
    private List<SSP_Data> SSP = null;

    public struct SSP_Data
    {
        public float depth;
        public float velocity;
        public float derivative1;
        public float derivative2;

        public SSP_Data(float depth, float velocity, float derivative1, float derivative2)
        {
            this.depth = depth;
            this.velocity = velocity;
            this.derivative1 = derivative1;
            this.derivative2 = derivative2;
        }
    }

    public void OnSSPButtonPress()
    {
        BrowserProperties bp = new BrowserProperties();        
        bp.initialDir = Directory.GetCurrentDirectory() + "\\Assets\\SSP";
        bp.filter = "txt files (*.txt)|*.txt|All Files (*.*)|*.*";
        bp.filterIndex = 0;
        new FileBrowser().OpenFileBrowser(bp, path =>
        {            
            if (path != "")
            {
                string filename = path.Split("\\").Last(); // print only filename and extension on the button, not the entire path

                GameObject.Find("Button - Filepicker_SSP").GetComponentInChildren<Text>().text = filename;
                filepathHasChanged = true;
                ReadSSPFromFile(path);
            }
        });        
    }

    public bool SSPFileHasChanged()
    {
        return filepathHasChanged;
    }

    public void AckSSPFileHasChanged() // Main.cs calls this function to acknowledge the change
    {
        if (filepathHasChanged) // only update if a change has happened
        {
            filepathHasChanged = false;
        }            
    }

    private void ReadSSPFromFile(string filepath)
    {
        SSP = new List<SSP_Data>();
        string line;

        try
        {
            StreamReader sr = new StreamReader(filepath);
            line = sr.ReadLine();

            while (line != null) // read lines until the end and add the values to the SSP-list
            {                
                string[] items = line.Split();
                SSP_Data data = new SSP_Data();
                int i = 0;
                foreach (string item in items)
                {                    
                    bool isNumber = float.TryParse(item, out float numericValue);
                    if (isNumber)
                    {
                        switch (i)
                        {
                            case 0:
                                if (numericValue > 0)
                                {
                                    numericValue = -numericValue; // depth needs to be negative
                                }
                                data.depth = numericValue;
                                break;
                            case 1:
                                data.velocity = numericValue;                                
                                break;
                            default:
                                break;
                        }
                        i++;
                    }
                }
                SSP.Add(data);

                line = sr.ReadLine();
            }
            sr.Close(); // close file

            // calculate derivate and second derivate

            // Estimate derivatives
            float[] derivatives1 = new float[SSP.Count];
            float[] b = new float[SSP.Count];
            for (int j = 0; j < SSP.Count - 1; j++) {         

                derivatives1[j] = (SSP[j+1].velocity - SSP[j].velocity) / (SSP[j + 1].depth - SSP[j].depth);
                if (j > 0)
                {
                    b[j] = (derivatives1[j] - derivatives1[j - 1]) / (SSP[j + 1].depth - SSP[j - 1].depth) / 2;
                    b[j - 1] = b[j - 1] + b[j];
                }

                // Set coefficients for interpolation
                float depth = SSP[j].depth;
                float velocity = SSP[j].velocity;
                float derivative1 = derivatives1[j]; // - b[j] * (SSP[j + 1].depth - SSP[j].depth);
                float derivative2 = 0.0f; // b[j];

                SSP[j] = new SSP_Data(depth, velocity, derivative1, derivative2);

                
                //SSP[j, 2] = a[j] - b[j] * (z[j + 1] - z[j]);
                //SSP[j, 3] = b[j];
            }

        }
        catch(Exception e)
        {
            Debug.Log("Exception: " + e.Message);
        }
    }

    public List<SSP_Data> GetSSPData()
    {
        return SSP;
    }
}
