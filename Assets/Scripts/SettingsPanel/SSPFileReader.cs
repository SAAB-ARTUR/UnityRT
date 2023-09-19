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
    private List<float> SSP = null;    

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
        SSP = new List<float>();
        string line;

        try
        {
            StreamReader sr = new StreamReader(filepath);
            line = sr.ReadLine();

            while (line != null) // read lines until the end and add the values to the SSP-list
            {                
                string velocity = line.Split().Last();

                bool isNumber = float.TryParse(velocity, out float numericValue);

                if (isNumber && numericValue > 0)
                {
                    SSP.Add(numericValue);
                }
                else // incorrect format, delete list
                {
                    SSP.Clear();                    
                    break;
                }
                line = sr.ReadLine();
            }
            sr.Close(); // close file
        }
        catch(Exception e)
        {
            Debug.Log("Exception: " + e.Message);
        }
    }

    public List<float> GetSSPData()
    {
        return SSP;
    }
}
