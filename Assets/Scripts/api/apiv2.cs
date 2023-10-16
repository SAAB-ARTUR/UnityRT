using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using Google.FlatBuffers;
using SAAB.Artur;

// using SAAB.Artur.Control;
using UnityEngine;

public class apiv2 : MonoBehaviour
{

    bool processreadyForData = true;

    string command = "";

    byte[]? msg = null;

    Main main = null;

    Process process = null;
    Thread thread1 = null;

    List<List<Vector3>> rays = new List<List<Vector3>>();

    Queue<SAAB.Artur.Control.Message> messageQueue = new Queue<SAAB.Artur.Control.Message>();

    bool run = false;

    // Start is called before the first frame update
    void OnEnable()
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


                    
                    // process.StandardInput.WriteLine(bmsg.Length.ToString());
                    //process.StandardInput.Flush();

                   

                    byte[] blength = BitConverter.GetBytes(msg.Length);
                    //UnityEngine.Debug.Log("blength: " + blength.Length.ToString());
                    //UnityEngine.Debug.Log("bmsglength: " + bmsg.Length.ToString());
                    byte[] total_message = blength.Concat(msg).ToArray();
                    process.StandardInput.BaseStream.Write(total_message, 0, total_message.Length); 
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
        


        byte[] bb = Base64Decode(e.Data);


        // Test read
        SAAB.Artur.Control.Message m = SAAB.Artur.Control.Message.GetRootAsMessage(new ByteBuffer(bb));

        messageQueue.Enqueue(m);

        UnityEngine.Debug.Log("Queue length: "  + messageQueue.Count);


        
    }

    void Control(SAAB.Artur.Control.ControlMessage m) { 
        // TODO 
    }

    void ChangeSetup(SAAB.Artur.Control.SetupMessage m) { 
        
        // TODO

    }

    void Trace() {


        UnityEngine.Debug.Log("TRACING222");
        UnityEngine.Debug.Log(rays.Count.ToString());
        main.TraceNow();

        //main.doRayTracing = true;

        UnityEngine.Debug.Log(rays.Count.ToString());

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

        msg = CreateOutputMessage();

    }


    private void Update()
    {

        // Work on the queue

        if (messageQueue.Count > 0) {

            SAAB.Artur.Control.Message m = messageQueue.Dequeue();
            UnityEngine.Debug.Log("Queue length " + messageQueue.Count);

            SAAB.Artur.Control.MessageType tt = m.MessageType;
            UnityEngine.Debug.Log(tt.ToString());

            switch (tt)
            {
                case SAAB.Artur.Control.MessageType.ResponseHandled:
                    processreadyForData = true;
                    break;

                case SAAB.Artur.Control.MessageType.ControlMessage:
                    Control(m.Message_AsControlMessage());
                    break;

                case SAAB.Artur.Control.MessageType.TraceNow:
                    UnityEngine.Debug.Log("TRACING");
                    Trace();
                    break;
                case SAAB.Artur.Control.MessageType.SetupMessage:
                    ChangeSetup(m.Message_AsSetupMessage());
                    break;
                case SAAB.Artur.Control.MessageType.NONE: break;

            }
        }

        


    }

    private void OnDisable()
    {

        UnityEngine.Debug.Log("Disable api...");
        thread1.Interrupt();
        process.Kill();
        thread1.Abort();
       
    }

    byte[] CreateOutputMessage()
    {

        // Access data required

        // Source position
        Vector3 psrc = main.srcSphere.transform.position;

        // Source looking-at
        Vector3 lookat = main.sourceCamera.transform.localToWorldMatrix.rotation.eulerAngles;

        // Source spans
        SourceParams sourceParams = main.srcSphere.GetComponent<SourceParams>();
        int n_theta = sourceParams.ntheta;
        int n_phi = sourceParams.nphi;
        int theta_width = sourceParams.theta; 
        int phi_width = sourceParams.phi;


        // Reciever position
        Vector3 prec = main.targetSphere.transform.position;
        



        // START Creation of the message

        
        // Create flatbuffer class
        FlatBufferBuilder fbb = new FlatBufferBuilder(1);


        // Sender
        Offset<Vec3> posSrcOffset = Vec3.CreateVec3(fbb, (double)psrc.x, (double)psrc.y, (double)psrc.z);
        Sender.StartSender(fbb);
        Sender.AddPosition(fbb, posSrcOffset);
        Offset<Sender> s = Sender.EndSender(fbb);

        // Reciever
        Offset<Vec3> posRecOffset = Vec3.CreateVec3(fbb, (double)prec.x, (double)prec.y, (double)prec.z);
        Reciever.StartReciever(fbb);
        Reciever.AddPosition(fbb, posRecOffset);
        Offset<Reciever> r = Reciever.EndReciever(fbb);

        // Ray collections
        Offset<RayCollection>[] rayCollectionCol = new Offset<RayCollection>[1];

        
        // TODO: Iterate over every combination of sender and reciever
        rayCollectionCol[0] = AddRays(fbb, s, r);
        VectorOffset rayCollectionsOffset = fbb.CreateVectorOfTables(rayCollectionCol);
        

        // World
        SAAB.Artur.World.StartWorld(fbb);
        SAAB.Artur.World.AddSender(fbb, s);
        SAAB.Artur.World.AddReciever(fbb, r);




        SAAB.Artur.World.AddRayCollections(fbb, rayCollectionsOffset);



        Offset<SAAB.Artur.World> w = SAAB.Artur.World.EndWorld(fbb);




        SAAB.Artur.World.FinishWorldBuffer(fbb, w);
       

        return fbb.DataBuffer.ToArray(fbb.DataBuffer.Position, fbb.Offset);
    }

    // Interface to recieve rays from main.
    public void Rays(List<List<Vector3>> _rays) { 
        rays = _rays;
    }

    Offset<RayCollection> AddRays(FlatBufferBuilder fbb, Offset<Sender> s, Offset<Reciever> r) {


        
        Offset<SAAB.Artur.Ray>[] raycol;
        if (rays != null) {
            raycol = new Offset<SAAB.Artur.Ray>[rays.Count];
        }
        else
        {
            raycol = new Offset<SAAB.Artur.Ray>[0];
        }

        if (rays != null) {

            

            for (int rayii = 0; rayii < rays.Count; rayii++){
                //SAAB.Artur.Ray.StartXCartesianVector(fbb, rays.Count);
                List<Vector3> ray = rays[rayii];

                // Offset<Vec3>[] positions = new Offset<Vec3>[ray.Count];

                SAAB.Artur.Ray.StartXCartesianVector(fbb, rays[rayii].Count);

                for (int i = ray.Count -1 ; i >= 0; i--) {
                    SAAB.Artur.Vec3.CreateVec3(fbb, ray[i].x, ray[i].y, ray[i].z);    
                }
                VectorOffset posoffset = fbb.EndVector();
                



                SAAB.Artur.Ray.StartRay(fbb);
                SAAB.Artur.Ray.AddXCartesian(fbb, posoffset);

                // TODO: Add cylindrical coordinates representation
                // SAAB.Artur.Ray.AddXCylindrical(fbb, ...);
                
                
                raycol[rayii] = SAAB.Artur.Ray.EndRay(fbb);
                
            
            }


        }
        // VectorOffset raysOffset = fbb.CreateVectorOfTables(raycol);
        VectorOffset vv = SAAB.Artur.RayCollection.CreateRaysVector(fbb, raycol);

        SAAB.Artur.RayCollection.StartRayCollection(fbb);
        
        SAAB.Artur.RayCollection.AddRays(fbb, vv);
        

        //SAAB.Artur.RayCollection.AddSender(fbb, s);
        //SAAB.Artur.RayCollection.AddReciever(fbb, r);
        return SAAB.Artur.RayCollection.EndRayCollection(fbb);
        
        
    }

    public static byte[] Base64Decode(string base64EncodedData)
    {

        return Convert.FromBase64String(base64EncodedData);
    }
}


