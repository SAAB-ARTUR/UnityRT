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
using CompanyNamespaceWhatever;

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

        // Test saving of a flatbuffer
        Save();
        Load();
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
    // Update is called once per frame
    // Update is called once per frame
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
        */
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

}
