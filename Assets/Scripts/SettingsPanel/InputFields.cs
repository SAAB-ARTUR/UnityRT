using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputFields : MonoBehaviour
{
    [SerializeField] GameObject srcSphere = null;
    [SerializeField] InputField ntheta = null;
    [SerializeField] InputField nphi = null;
    //[SerializeField] InputField maxInteractions = null;
    [SerializeField] InputField theta = null;
    [SerializeField] InputField phi = null;    
    [SerializeField] GameObject world_manager = null;
    [SerializeField] InputField range = null;
    [SerializeField] InputField nrOfIntegrationSteps = null;
    [SerializeField] InputField integrationStepSize = null;
    [SerializeField] GameObject bellhop = null;
    [SerializeField] InputField callbackCommand = null;

    // Start is called before the first frame update
    void Start()
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        ntheta.text = sourceParams.ntheta.ToString();
        nphi.text = sourceParams.nphi.ToString();
        //maxInteractions.text = sourceParams.MAXINTERACTIONS.ToString();
        theta.text = sourceParams.theta.ToString();
        phi.text = sourceParams.phi.ToString();

        World world = world_manager.GetComponent<World>();
        range.text = world.range.ToString();

        BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
        nrOfIntegrationSteps.text = bellhopParams.BELLHOPINTEGRATIONSTEPS.ToString();
        integrationStepSize.text = bellhopParams.BELLHOPSTEPSIZE.ToString();
        callbackCommand.text = "This will do nothing for now.";
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
    public void OnNphiChange()
    {
        string str = nphi.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0 && numericValue % 8 == 0)
        {
            SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
            sourceParams.nphi = numericValue;
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
    public void OnPhiChange()
    {
        string str = phi.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0)
        {
            SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
            sourceParams.phi = numericValue;
        }
    }
    /*public void OnMaxInteractionsChange()
    {
        string str = maxInteractions.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0)
        {
            SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
            sourceParams.MAXINTERACTIONS = numericValue;
        }
    }*/

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

    public void OnCallbackCommandEntered()
    {
        Debug.Log("shshj");
    }
}
