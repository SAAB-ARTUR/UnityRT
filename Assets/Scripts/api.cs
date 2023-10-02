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

    string endpoint = ".conda\\envs\\unity_interface\\python.exe async_test.py";
    Process process = null;
    StreamWriter stdin = null;



    Main main = null;


    Thread processThread = null;


    int i = 0;

    // Start is called before the first frame update
    void Start()
    {

        main = this.GetComponent<Main>();   

        RunExternal();

        process.OutputDataReceived += OutputHandler;


        ThreadStart ths = new ThreadStart(() => {
            process.Start();
            process.BeginOutputReadLine();
            
        });
        processThread = new Thread(ths);
        processThread.Start();
     

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
        

        startInfo.Arguments = "threaded_test.py";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.RedirectStandardInput = true;
        Process p = Process.Start(startInfo);
        process = p;
        return p;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        StreamWriter writer = process.StandardInput;



        DateTime currentDateTime = DateTime.Now;
        string d = currentDateTime.ToString("HH:mm:ss:ff");
        //writer.WriteLine("Current date and time: " + d);

        writer.WriteLine(main.srcSphere.transform.position.y);
        writer.Flush();

    }

    private void OnDestroy()
    {
        //stdin.Close();
        process.Kill();
    }
}
