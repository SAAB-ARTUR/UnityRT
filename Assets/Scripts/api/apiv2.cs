using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Google.FlatBuffers;
using SAAB.Artur;
using UnityEngine;

public class apiv2 : MonoBehaviour
{

    bool processreadyForData = false;

    Vector3? msg = null;

    Main main = null;

    Process process = null;
    Thread thread1 = null;

    // Start is called before the first frame update
    void Start()
    {

        main = this.GetComponent<Main>();

        // Setup async read and write to std. 
        ProcessStartInfo startInfo = RunExternal();



        process = Process.Start(startInfo);

        process.OutputDataReceived += Process_OutputDataReceived;
        process.ErrorDataReceived += Process_ErrorDataReceived;

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        UnityEngine.Debug.Log("Has started...");

        ThreadStart threadStart = new ThreadStart(() =>
        {

            while (true)
            {



                if (processreadyForData && msg != null)
                {


                    byte[] bmsg = CreateOutputMessage((Vector3)msg);



                    // process.StandardInput.WriteLine(bmsg.Length.ToString());
                    //process.StandardInput.Flush();

                   

                    byte[] blength = BitConverter.GetBytes(bmsg.Length);
                    UnityEngine.Debug.Log("blength: " + blength.Length.ToString());
                    UnityEngine.Debug.Log("bmsglength: " + bmsg.Length.ToString());
                    process.StandardInput.BaseStream.Write(blength.Concat(bmsg).ToArray(), 0, blength.Length + bmsg.Length); 
                    process.StandardInput.BaseStream.Flush();

                    //process.StandardInput.BaseStream.Write(bmsg, 0, bmsg.Length);
                    //process.StandardInput.BaseStream.Flush();
                    // process.StandardInput.Flush();


                    processreadyForData = false;

                }

            }

        });

        thread1 = new Thread(threadStart);
        thread1.Start();


    }

    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Error:" + e.Data);
    }

    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("Python: " + e.Data);
        processreadyForData = true;
    }

    ProcessStartInfo RunExternal()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        UnityEngine.Debug.Log("Home is " + home);
        startInfo.FileName = home + "\\" + ".conda\\envs\\unity_interface\\python.exe";



        startInfo.Arguments = "simple_test.py";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.RedirectStandardInput = true;
        // Process p = Process.Start(startInfo);
        //process = p;
        return startInfo;
    }



    // Update is called once per frame
    private void FixedUpdate()
    {

        msg = main.srcSphere.transform.position;
    }

    private void OnDestroy()
    {
        process.Close();
        thread1.Abort();
    }

    byte[] CreateOutputMessage(Vector3 position)
    {

        // Create flatbuffer class
        FlatBufferBuilder fbb = new FlatBufferBuilder(1);

        Vector3 p = position;

        Offset<Vec3> v = Vec3.CreateVec3(fbb, (double)p.x, (double)p.y, (double)p.z);

        //
        Sender.StartSender(fbb);
        Sender.AddPosition(fbb, v);
        
        
        Offset<Sender> s = Sender.EndSender(fbb);



        SAAB.Artur.World.StartWorld(fbb);
        SAAB.Artur.World.AddSender(fbb, s);

        Offset<SAAB.Artur.World> w = SAAB.Artur.World.EndWorld(fbb);

        SAAB.Artur.World.FinishWorldBuffer(fbb, w);


        return fbb.DataBuffer.ToArray(fbb.DataBuffer.Position, fbb.Offset);
    }
}
