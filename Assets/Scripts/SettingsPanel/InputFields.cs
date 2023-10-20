using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputFields : MonoBehaviour
{
    [SerializeField] GameObject srcSphere = null;
    [SerializeField] InputField ntheta = null;    
    [SerializeField] InputField theta = null;    
    [SerializeField] GameObject world_manager = null;
    [SerializeField] InputField range = null;
    [SerializeField] InputField nrOfIntegrationSteps = null;
    [SerializeField] InputField integrationStepSize = null;
    [SerializeField] GameObject bellhop = null;
    [SerializeField] InputField callbackCommand = null;     
    [SerializeField] InputField targets = null;
    [SerializeField] InputField maxNrOfSurfaceHits = null;
    [SerializeField] InputField maxNrOfBottomHits = null;

    apiv2 api = null;

    private string oldTargets;
    // Start is called before the first frame update
    void Start()
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        ntheta.text = sourceParams.ntheta.ToString();        
        theta.text = sourceParams.theta.ToString();        

        World world = world_manager.GetComponent<World>();
        range.text = world.range.ToString();
        Vector3 mainTargetPos = world.GetMainTargetPosition();
        targets.text = "{" + mainTargetPos.x.ToString() + ", " + mainTargetPos.y.ToString() + ", " + mainTargetPos.z.ToString() + "}";
        oldTargets = targets.text;

        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
        nrOfIntegrationSteps.text = bellhopParams.BELLHOPINTEGRATIONSTEPS.ToString();
        integrationStepSize.text = bellhopParams.BELLHOPSTEPSIZE.ToString();
        maxNrOfSurfaceHits.text = "0";
        maxNrOfBottomHits.text = "0";        
        callbackCommand.text = "This will do nothing for now.";

                // Setup interface to API 
        try {
            api = GetComponent<apiv2>();
        } catch
        {
            
        }

    }

    public void OnNthetaChange()
    {        
        string str = ntheta.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0 && numericValue % 8 == 0)
        {
            SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
            sourceParams.ntheta = numericValue;
        }
    }

    public void OnThetaChange()
    {
        string str = theta.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0)
        {
            SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
            sourceParams.theta = numericValue;
        }
    }


    public void OnRangeChange()
    {
        string str = range.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0)
        {
            World world = world_manager.GetComponent<World>();
            world.range = numericValue;
        }
    }

    public void ExtraTargets()
    {              
        string str = targets.text;
        List<float> targetCoords = new List<float>();        
        try
        {
            string[] targetsStr = str.Split(":");
            foreach (string target in targetsStr)
            {
                string[] coords = target.Split(",");
                foreach(string coord in coords)
                {
                    string s = string.Empty;
                    float val = 0;
                    for (int i = 0; i < coord.Length; i++)
                    {                        
                        if (Char.IsDigit(coord[i]) || coord[i].Equals('-'))
                        {                         
                            s += coord[i];
                        }                     
                    }
                    if(s.Length > 0)
                    {
                        bool parsed = float.TryParse(s, out val);
                        if (parsed)
                        {
                            targetCoords.Add(val);
                        }
                        else
                        {
                            targets.text = oldTargets;
                            Debug.Log("Incorrectly formatted target list!");
                            return;
                        }
                    }
                }
            }
        }
        catch(Exception e)
        {
            targets.text = oldTargets;
            Debug.Log("Incorrectly formatted target list!");
            Debug.Log(e);
        }

        // check valid list length
        if (targetCoords.Count % 3 != 0 || targetCoords.Count <= 0)
        {
            targets.text = oldTargets;
            Debug.Log("Incorrectly formatted target list!");
            return;
        }

        // create the targets from the coords
        World world = world_manager.GetComponent<World>();
        if (!world.CreateTargets(targetCoords))
        {
            targets.text = oldTargets;
            Debug.Log("Incorrectly formatted target list, one or more targets are outside the volume boundaries!");
            return;
        }
        
        oldTargets = targets.text;
    }

    public void OnNrOfStepsChange()
    {
        string str = nrOfIntegrationSteps.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0)
        {
            BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
            bellhopParams.BELLHOPINTEGRATIONSTEPS = numericValue;
        }
    }

    public void OnStepSizeChange()
    {
        string str = integrationStepSize.text;
        bool isNumber = float.TryParse(str, out float numericValue);

        if (isNumber && numericValue > 0)
        {
            BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
            bellhopParams.BELLHOPSTEPSIZE = numericValue;
        }
    }

    public void OnSurfaceHitsChange()
    {        
        string str = maxNrOfSurfaceHits.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue >= 0)
        {
            BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
            bellhopParams.MAXNRSURFACEHITS = numericValue;
        }
    }

    public void OnBottomHitsChange()
    {        
        string str = maxNrOfBottomHits.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue >= 0)
        {
            BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
            bellhopParams.MAXNRBOTTOMHITS = numericValue;
        }
    }

    public void OnCallbackCommandEntered()
    {
        string command = callbackCommand.text;
        
    }

    public void ModelSelector()
    {

    }


}
