using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
    }

    public void OnSSPButtonPress()
    {
        string filepath = EditorUtility.OpenFilePanel("Select File", "Assets/SSP", "txt");
        Debug.Log(filepath);
        if (filepath != "")
        {
            Debug.Log("File has been picked");
            string filename = filepath.Split("/").Last(); // print only filename and extension on the button, not the entire path

            GameObject.Find("Button - Filepicker").GetComponentInChildren<Text>().text = filename;
            
            filepathHasChanged = true;
            ReadSSPFromFile(filepath);
        }
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
                                data.depth = numericValue;
                                break;
                            case 1:
                                data.velocity = numericValue;
                                break;
                            case 2:
                                data.derivative1 = numericValue;
                                break;
                            case 3:
                                data.derivative2 = numericValue;
                                break;
                            default:
                                break;
                        }
                    }
                    i++;
                }
                SSP.Add(data);

                line = sr.ReadLine();
            }
            sr.Close(); // close file
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