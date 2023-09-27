using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System;

public class api : MonoBehaviour
{

    string endpoint = "C:\\Users\\Daniel\\.conda\\envs\\unity_interface\\python.exe test.py";
    // Start is called before the first frame update
    void Start()
    {

        Process p = RunExternal();

       

        StreamReader reader = p.StandardOutput;
        string output = reader.ReadToEnd();

        UnityEngine.Debug.Log(output);


        reader = p.StandardError;
        output = reader.ReadToEnd();

        UnityEngine.Debug.Log(output);

    }

    Process RunExternal()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        UnityEngine.Debug.Log("Home is " + home);
        startInfo.FileName = home + "\\.conda\\envs\\unity_interface\\python.exe";
        //startInfo.FileName = "cd";
        startInfo.Arguments = "test.py";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        Process p = Process.Start(startInfo);
        return p;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
