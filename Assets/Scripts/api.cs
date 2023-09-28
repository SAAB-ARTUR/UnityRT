using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System;
using System.Threading;

public class api : MonoBehaviour
{

    string endpoint = ".conda\\envs\\unity_interface\\python.exe test.py";
    Process process = null;
    StreamWriter stdin = null; 


    int i = 0;

    // Start is called before the first frame update
    void Start()
    {

        RunExternal();

        process.OutputDataReceived += OutputHandler;

        process.Start();

        process.BeginOutputReadLine();
     

    }

    void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {

        UnityEngine.Debug.Log(outLine.Data);

    }

    Process RunExternal()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        UnityEngine.Debug.Log("Home is " + home);
        startInfo.FileName = home + "\\" + ".conda\\envs\\unity_interface\\python.exe";
        

        startInfo.Arguments = "test.py";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.RedirectStandardInput = true;
        Process p = Process.Start(startInfo);
        process = p;
        return p;
    }

    // Update is called once per frame
    void Update()
    {

        StreamWriter writer = process.StandardInput;
        writer.WriteLine((i++).ToString());
        writer.Flush();

    }

    private void OnDestroy()
    {
        stdin.Close();
        process.Close();
    }
}
