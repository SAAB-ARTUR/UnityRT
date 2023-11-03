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
using UnityEngine.UI;
using UnityTemplateProjects;


public class PerTarget {

    public List<List<Vector3>> cart_rays;
    public List<List<Vector3>>? cyl_rays = null;
    public List<Main.PerRayData> raydatas;

}

public class apiv2 : MonoBehaviour
{

    [SerializeField] InputField ntheta;
    [SerializeField] InputField theta;
    [SerializeField] GameObject world;

    bool processreadyForData = true;
    bool processAlive = false;

    string command = "";

    byte[]? msg = null;

    Main main = null;

    Process process = null;
    Thread thread1 = null;

    // Enable this if you want to save the binary message to be sent to over the 
    // api in a file
    // For debugging purposes. Should be false for maximum performance.
    bool save = false;

    List<List<Vector3>> rays = new List<List<Vector3>>();

    List<PerTarget> rayData = new List<PerTarget>();

    Queue<SAAB.Artur.Control.Message> messageQueue = new Queue<SAAB.Artur.Control.Message>();

    bool run = false;

    // Start is called before the first frame update
    void OnEnable()
    {

        main = this.GetComponent<Main>();

        // Setup async read and write to std. 
        ProcessStartInfo startInfo = RunExternal();



        process = Process.Start(startInfo);
        processAlive = true;

        process.OutputDataReceived += Process_OutputDataReceived;
        process.ErrorDataReceived += Process_ErrorDataReceived;

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();


        ThreadStart threadStart = new ThreadStart(() =>
        {



            while (!process.HasExited)
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
       


        byte[] bb = Base64Decode(e.Data);


        // Test read
        SAAB.Artur.Control.Message m = SAAB.Artur.Control.Message.GetRootAsMessage(new ByteBuffer(bb));



        messageQueue.Enqueue(m);


        
    }

    void Control(SAAB.Artur.Control.ControlMessage m) {

       

        /*

        if (m.Sender != null) {
            if (m.Sender.Value.Position != null)
            {

                SAAB.Artur.Control.Vec3 ps = m.Sender.Value.Position.Value;

                main.sourceCamera.GetComponent<SimpleSourceController>().DirectJumpTo(new Vector3((float)ps.X, (float)ps.Z, (float)ps.Y));


            }

        }
        */
        SAAB.Artur.Control.Vec3? ps = m.Sender?.Position; 

        if (ps != null)
        {

            main.sourceCamera.GetComponent<SimpleSourceController>().DirectJumpTo(new Vector3((float)ps?.X, (float)ps?.Z, (float)ps?.Y));

        }


        SAAB.Artur.Control.Vec3? pr = m.Reciever?.Position;
        if (pr != null)
        {
            

            List<float> targetList = new List<float>();
            targetList.Add((float)pr?.X);
            targetList.Add((float)pr?.Y); 
            targetList.Add((float)pr?.Z);

            world.GetComponent<World>().CreateTargets(targetList);

        }

        SAAB.Artur.Control.AngleSpan ? angleSpan = m.Sender?.AngleSpan;

        if (angleSpan != null) {

            ntheta.text = angleSpan.Value.NTheta.ToString();
            theta.text = angleSpan.Value.ThetaSpan.ToString();

        }

        SAAB.Artur.Control.SphericalDir? lookAt = m.Sender?.LookAt;


        

        //Quaternion.LookRotation();

        SimpleSourceController c = main.sourceCamera.GetComponent<SimpleSourceController>();
        
        //c.DirectJumpTo(new Vector3(((float)ps?.X), ((float)ps?.Y), ((float)ps?.Z)));
        //c.DirectLookAt();


    }

    void ChangeSetup(SAAB.Artur.Control.SetupMessage m) { 
        
        // TODO

    }

    void Trace() {



        main.TraceNow();
        // Remove the rayData, while we are waiting for a new set of data points 

    }

    ProcessStartInfo RunExternal()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

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
    private void Update()
    {

        msg = CreateOutputMessage();
        if (save)
        {
            File.WriteAllBytes("apimsg.bin", msg);
        }


        if (messageQueue.Count > 0) {

            SAAB.Artur.Control.Message m = messageQueue.Dequeue();

            SAAB.Artur.Control.MessageType tt = m.MessageType;
            
            // UnityEngine.Debug.Log(tt.ToString());

            switch (tt)
            {
                case SAAB.Artur.Control.MessageType.ResponseHandled:
                    processreadyForData = true;
                    break;

                case SAAB.Artur.Control.MessageType.ControlMessage:
                    Control(m.Message_AsControlMessage());
                    break;

                case SAAB.Artur.Control.MessageType.TraceNow:
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

        if (!process.HasExited) {
            process.Kill();
            thread1.Abort();
        }


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
        int theta_width = sourceParams.theta; 



        



        // START Creation of the message

        
        // Create flatbuffer class
        FlatBufferBuilder fbb = new FlatBufferBuilder(1);


        // Sender
        Offset<Vec3> posSrcOffset = Vec3.CreateVec3(fbb, (double)psrc.x, (double)psrc.y, (double)psrc.z);
        Sender.StartSender(fbb);
        Sender.AddPosition(fbb, posSrcOffset);
        Offset<Sender> s = Sender.EndSender(fbb);

        // Reciever
        // For now, take the first target position and send. 
        Vector3 prec = world.GetComponent<World>().GetMainTargetPosition();
        Offset<Vec3> posRecOffset = Vec3.CreateVec3(fbb, (double)prec.x, (double)prec.y, (double)prec.z);
        Reciever.StartReciever(fbb);
        Reciever.AddPosition(fbb, posRecOffset);
        Offset<Reciever> r = Reciever.EndReciever(fbb);

        // Ray collections
        Offset<RayCollection>[] rayCollectionCol = new Offset<RayCollection>[rayData.Count];


        // TODO: Iterate over every combination of sender and recieve
        for (int ii = 0; ii < rayData.Count; ii++) {
            rayCollectionCol[ii] = AddRays(fbb, s, r, ii);
        }


        VectorOffset rayCollectionsOffset = fbb.CreateVectorOfTables(rayCollectionCol);
        

        // World
        SAAB.Artur.World.StartWorld(fbb);
        SAAB.Artur.World.AddSender(fbb, s);
        SAAB.Artur.World.AddReciever(fbb, r);

        if (rayData.Count > 0)
        {
            SAAB.Artur.World.AddCr(fbb, rayData[0].raydatas[0].cr);
            SAAB.Artur.World.AddCs(fbb, rayData[0].raydatas[0].cs);
        }




        SAAB.Artur.World.AddRayCollections(fbb, rayCollectionsOffset);



        Offset<SAAB.Artur.World> w = SAAB.Artur.World.EndWorld(fbb);




        SAAB.Artur.World.FinishWorldBuffer(fbb, w);
       

        return fbb.DataBuffer.ToArray(fbb.DataBuffer.Position, fbb.Offset);
    }

    // Interface to recieve rays from main.
    public void SetData(List<PerTarget> data) {
        rayData = data;
    }
    public void SetRays(List<List<Vector3>> _rays) {
        rays = _rays;
    }



    Offset<RayCollection> AddRays(FlatBufferBuilder fbb, Offset<Sender> s, Offset<Reciever> r, int collectionIndex) {


        
        Offset<SAAB.Artur.Ray>[] raycol;
        if (rayData != null) {
            raycol = new Offset<SAAB.Artur.Ray>[rayData[collectionIndex].cart_rays.Count] ;
        }
        else
        {
            raycol = new Offset<SAAB.Artur.Ray>[0];
        }

        if (rayData != null) {

            

            for (int rayii = 0; rayii < rayData[collectionIndex].cart_rays.Count; rayii++){
                //SAAB.Artur.Ray.StartXCartesianVector(fbb, rays.Count);
                List<Vector3> ray = rayData[collectionIndex].cart_rays[rayii];

                // Offset<Vec3>[] positions = new Offset<Vec3>[ray.Count];

                SAAB.Artur.Ray.StartXCartesianVector(fbb, ray.Count);

                for (int i = ray.Count -1 ; i >= 0; i--) {
                    SAAB.Artur.Vec3.CreateVec3(fbb, ray[i].x, ray[i].y, ray[i].z);    
                }
                VectorOffset posoffset = fbb.EndVector();


                SAAB.Artur.Ray.StartRay(fbb);
                SAAB.Artur.Ray.AddXCartesian(fbb, posoffset);

                /*
                SAAB.Artur.Ray.StartXCylindricalVector(fbb, rays[rayii].Count);

                for (int i = ray.Count - 1; i >= 0; i--)
                {   
                    float phi = 
                    ray[i].x, ray[i].y, ray[i].z
                }

                // TODO: Add cylindrical coordinates representation
                // SAAB.Artur.Ray.AddXCylindrical(fbb, ...);
                */

                Main.PerRayData d = rayData[collectionIndex].raydatas[rayii];
                SAAB.Artur.Ray.AddBeta(fbb, d.beta);
                SAAB.Artur.Ray.AddNtop(fbb,  d.ntop);
                SAAB.Artur.Ray.AddNbot(fbb, d.nbot);
                SAAB.Artur.Ray.AddNcaust(fbb, d.ncaust);
                SAAB.Artur.Ray.AddDelay(fbb, d.delay);
                SAAB.Artur.Ray.AddCurve(fbb, d.curve);
                SAAB.Artur.Ray.AddDistanceToTarget(fbb, d.xn);
                SAAB.Artur.Ray.AddQi(fbb, d.qi);
                SAAB.Artur.Ray.AddTheta(fbb, d.theta);
                SAAB.Artur.Ray.AddPhi(fbb, d.phi);
                SAAB.Artur.Ray.AddContributing(fbb, d.contributing);
                SAAB.Artur.Ray.AddTransmissionLoss(fbb, d.TL);
                SAAB.Artur.Ray.AddTargetIndex(fbb, d.target);
                

                raycol[rayii] = SAAB.Artur.Ray.EndRay(fbb);
                
            
            }


        }
        // VectorOffset raysOffset = fbb.CreateVectorOfTables(raycol);
        VectorOffset vv = SAAB.Artur.RayCollection.CreateRaysVector(fbb, raycol);

        SAAB.Artur.RayCollection.StartRayCollection(fbb);
        
        SAAB.Artur.RayCollection.AddRays(fbb, vv);
        SAAB.Artur.RayCollection.AddSender(fbb, s);
        SAAB.Artur.RayCollection.AddReciever(fbb, r);

        //SAAB.Artur.RayCollection.AddSender(fbb, s);
        //SAAB.Artur.RayCollection.AddReciever(fbb, r);
        return SAAB.Artur.RayCollection.EndRayCollection(fbb);
        
        
    }

    public static byte[] Base64Decode(string base64EncodedData)
    {

        return Convert.FromBase64String(base64EncodedData);
    }
}


