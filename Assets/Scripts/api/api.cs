using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System;
using System.Threading;
using Google.FlatBuffers;
using SAAB.Artur;
using UnityEngine.UIElements;
using UnityEditor;
using static UnityEngine.Awaitable;
using System.Net.Configuration;
using System.Web;

public class api : MonoBehaviour
{

    string endpoint = ".conda\\envs\\unity_interface\\python.exe async_test.py";
    Process process = null;
    StreamWriter stdin = null;



    Main main = null;

    string msg = "";
    bool new_status = false;
    bool handled = true;

    Thread processThread = null;
    bool processStarted = false;


    int i = 0;

    // Start is called before the first frame update
    void Start()
    {

        main = this.GetComponent<Main>();   

        ProcessStartInfo pi = RunExternal();
           


        //process.OutputDataReceived += OutputHandler;
        // process.ErrorDataReceived += ErrorHandler;

        // process.Start();
        // process.BeginErrorReadLine();


        ThreadStart ths = new ThreadStart(() => {
            
            //process.BeginOutputReadLine();
            

            while (true)
            {

                if (new_status && processStarted) {
                    process.StandardInput.WriteLine("5.9");
                    process.StandardInput.Flush();

                    //SendStatus(process.StandardInput);

                    new_status = false;
                    handled = false;
                    
                }

                //Thread.Sleep(100);

            }

        });
        processThread = new Thread(ths);
        processThread.Start();

        ThreadStart ths2 = new ThreadStart(() => {
            
            
            while (true)
            {


                if (processStarted) {
                    UnityEngine.Debug.Log(process.StandardOutput.ReadLine());

                }
                //Thread.Sleep(100);

            }

        });
        Thread processThread2 = new Thread(ths2);
        

        process = Process.Start(pi);
        processThread2.Start();
        processStarted = true;



        // Test saving of a flatbuffer
        //Save();
        //Load();
    }

    void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {

        UnityEngine.Debug.Log("Python:"  + outLine.Data);
        handled = true;

    }

    void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {

        UnityEngine.Debug.Log("Python err:" + outLine.Data);

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
    void FixedUpdate()
    {

        StreamWriter writer = process.StandardInput;    


        DateTime currentDateTime = DateTime.Now;
        string d = currentDateTime.ToString("HH:mm:ss:ff");
        //writer.WriteLine("Current date and time: " + d);

        

        msg = main.srcSphere.transform.position.y.ToString();
        new_status = true;
        //UnityEngine.Debug.Log("Handled?" + handled.ToString());



        //CreateOutputMessage(main.srcSphere.transform);


    }

    private void SendStatus(StreamWriter channel)
    {
        channel.WriteLine(msg);
        channel.Flush();
    }

    private void OnDestroy()
    {
        //process.StandardInput.Close();
        //process.Kill();
        process.Close();
    }
    // Update is called once per frame
    // Update is called once per frame

    

    void CreateOutputMessage(Transform senderPos) {

        // Create flatbuffer class
        FlatBufferBuilder fbb = new FlatBufferBuilder(1);

        Vector3 p = senderPos.position;

        Offset<Vec3> v = Vec3.CreateVec3(fbb, (double) p.x, (double) p.y, (double) p.z);

        //
        Sender.StartSender(fbb);
        Sender.AddPosition(fbb, v);
        Offset<Sender> s = Sender.EndSender(fbb);



        SAAB.Artur.World.StartWorld(fbb);
        SAAB.Artur.World.AddSender(fbb, s);

        Offset<SAAB.Artur.World> w = SAAB.Artur.World.EndWorld(fbb);

        SAAB.Artur.World.FinishWorldBuffer(fbb, w);

        //var m = new MemoryStream(fbb.DataBuffer.ToFullArray(), fbb.DataBuffer.Position, fbb.Offset);
        
        //var b = new BinaryWriter(process.StandardInput.BaseStream);

        //UnityEngine.Debug.Log(fbb.Offset);

        //var lenb = BitConverter.GetBytes(fbb.Offset);
        //UnityEngine.Debug.Log(lenb.Length);
        process.StandardInput.WriteLine("");
        process.StandardInput.Flush();
        //b.Write(lenb, 0, lenb.Length);
        
        //b.Write(fbb.DataBuffer.ToFullArray(), fbb.DataBuffer.Position, fbb.Offset);
        //b.Flush();
        //process.StandardInput.Flush();
        //b.Close(); 
        //b.Flush();
        //process.StandardInput.Flush();
        //b.Close();

        //process.StandardInput.BaseStream.Write(fbb.DataBuffer.ToFullArray(), fbb.DataBuffer.Position, fbb.Offset);

        //process.StandardInput.BaseStream.Flush();


        //m.WriteTo(process.StandardInput.BaseStream);
        //m.Flush();

        // Save the data into "SAVE_FILENAME.whatever" file, name doesn't matter obviously
        using (var ms = new MemoryStream(fbb.DataBuffer.ToFullArray(), fbb.DataBuffer.Position, fbb.Offset))
        {
            File.WriteAllBytes("SAVE_FILENAME.whatever", ms.ToArray());
            UnityEngine.Debug.Log("SAVED !");
        }

    }


    /*
    void Save()
    {
        // Create flatbuffer class
        FlatBufferBuilder fbb = new FlatBufferBuilder(1);

        // Create our sword for GameDataWhatever
        //------------------------------------------------------

        WeaponClassesOrWhatever weaponType = WeaponClassesOrWhatever.Sword;
        Sword.StartSword(fbb);
        Sword.AddDamage(fbb, 123);
        Sword.AddDistance(fbb, 999);
        Offset<Sword> offsetWeapon = Sword.EndSword(fbb);

        /*
        // For gun uncomment this one and remove the sword one
        WeaponClassesOrWhatever weaponType = WeaponClassesOrWhatever.Gun;
        Gun.StartGun(fbb);
        Gun.AddDamage(fbb, 123);
        Gun.AddReloadspeed(fbb, 999);
        Offset<Gun> offsetWeapon = Gun.EndGun(fbb);
        
        //------------------------------------------------------

        // Create strings for GameDataWhatever
        //------------------------------------------------------
        StringOffset cname = fbb.CreateString("Test String ! time : " + DateTime.Now);
        //------------------------------------------------------

        // Create GameDataWhatever object we will store string and weapon in
        //------------------------------------------------------
        GameDataWhatever.StartGameDataWhatever(fbb);

        GameDataWhatever.AddName(fbb, cname);
        GameDataWhatever.AddPos(fbb, Vec3.CreateVec3(fbb, 1, 2, 1)); // structs can be inserted directly, no need to be defined earlier
        GameDataWhatever.AddColor(fbb, CompanyNamespaceWhatever.Color.Red);

        //Store weapon
        GameDataWhatever.AddWeaponType(fbb, weaponType);
        GameDataWhatever.AddWeapon(fbb, offsetWeapon.Value);

        var offset = GameDataWhatever.EndGameDataWhatever(fbb);
        //------------------------------------------------------

        GameDataWhatever.FinishGameDataWhateverBuffer(fbb, offset);

        // Save the data into "SAVE_FILENAME.whatever" file, name doesn't matter obviously
        using (var ms = new MemoryStream(fbb.DataBuffer.ToFullArray(), fbb.DataBuffer.Position, fbb.Offset))
        {
            File.WriteAllBytes("SAVE_FILENAME.whatever", ms.ToArray());
            UnityEngine.Debug.Log("SAVED !");
        }
    }

    void Load()
    {

        if (!File.Exists("SAVE_FILENAME.whatever")) throw new Exception("Load failed : 'SAVE_FILENAME.whatever' not exis, something went wrong");

        ByteBuffer bb = new ByteBuffer(File.ReadAllBytes("SAVE_FILENAME.whatever"));

        if (!GameDataWhatever.GameDataWhateverBufferHasIdentifier(bb))
        {
            throw new Exception("Identifier test failed, you sure the identifier is identical to the generated schema's one?");
        }

        GameDataWhatever data = GameDataWhatever.GetRootAsGameDataWhatever(bb);

        UnityEngine.Debug.Log("LOADED DATA : ");
        UnityEngine.Debug.Log("NAME : " + data.Name);
        UnityEngine.Debug.Log("POS : " + data.Pos?.X + ", " + data.Pos?.Y + ", " + data.Pos?.Z);
        UnityEngine.Debug.Log("COLOR : " + data.Color);
        UnityEngine.Debug.Log("INVENTORY : " + data.GetInventoryArray()?.ToString());

        UnityEngine.Debug.Log("WEAPON TYPE : " + data.WeaponType);

        switch (data.WeaponType)
        {
            case WeaponClassesOrWhatever.Sword:
                Gun sword = new Gun();
                sword = data.WeaponAsGun();
                UnityEngine.Debug.Log("SWORD DAMAGE  : " + sword.Reloadspeed);
                break;
            case WeaponClassesOrWhatever.Gun:
                Gun gun = new Gun();
                gun = data.WeaponAsGun();
                UnityEngine.Debug.Log("GUN RELOAD SPEED  : " + gun.Reloadspeed);
                break;
            default:
                break;
        }
    }
    */

}
