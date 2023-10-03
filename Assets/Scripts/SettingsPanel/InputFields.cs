using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] GameObject target = null;
    [SerializeField] Slider targetXSlider = null;
    [SerializeField] Slider targetYSlider = null;
    [SerializeField] Slider targetZSlider = null;
    [SerializeField] GameObject targetXSliderMin = null;
    [SerializeField] GameObject targetXSliderMax = null;
    [SerializeField] GameObject targetYSliderMin = null;
    [SerializeField] GameObject targetYSliderMax = null;
    [SerializeField] GameObject targetZSliderMin = null;
    [SerializeField] GameObject targetZSliderMax = null;
    [SerializeField] InputField maxNrOfSurfaceHits = null;
    [SerializeField] InputField maxNrOfBottomHits = null;
    [SerializeField] InputField bellhopIterations = null;


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
        maxNrOfSurfaceHits.text = "0";
        maxNrOfBottomHits.text = "0";
        bellhopIterations.text = "1";
        callbackCommand.text = "This will do nothing for now.";

        int worldRange = world.range;
        targetXSlider.minValue = -worldRange / 2;
        targetXSlider.maxValue = worldRange / 2;
        targetZSlider.minValue = -worldRange / 2;
        targetZSlider.maxValue = worldRange / 2;
        targetYSlider.minValue = world.GetWaterDepth();
        targetYSlider.maxValue = 0;
        targetXSlider.value = target.transform.position.x;
        targetYSlider.value = target.transform.position.y;
        targetZSlider.value = target.transform.position.z;

        TextMeshProUGUI tmProXmin = targetXSliderMin.GetComponent<TextMeshProUGUI>();        
        tmProXmin.text = targetXSlider.minValue.ToString();
        TextMeshProUGUI tmProXmax = targetXSliderMax.GetComponent<TextMeshProUGUI>();
        tmProXmax.text = targetXSlider.maxValue.ToString();

        TextMeshProUGUI tmProYmin = targetYSliderMin.GetComponent<TextMeshProUGUI>();
        tmProYmin.text = targetYSlider.minValue.ToString();
        TextMeshProUGUI tmProYmax = targetYSliderMax.GetComponent<TextMeshProUGUI>();
        tmProYmax.text = targetYSlider.maxValue.ToString();

        TextMeshProUGUI tmProZmin = targetZSliderMin.GetComponent<TextMeshProUGUI>();
        tmProZmin.text = targetZSlider.minValue.ToString();
        TextMeshProUGUI tmProZmax = targetZSliderMax.GetComponent<TextMeshProUGUI>();
        tmProZmax.text = targetZSlider.maxValue.ToString();
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

        if (isNumber && numericValue > 0)
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

            targetXSlider.minValue = -world.range / 2; //borde kunna bli floats??
            targetXSlider.maxValue = world.range / 2;
            targetZSlider.minValue = -world.range / 2;
            targetZSlider.maxValue = world.range / 2;

            targetXSlider.value = 0; //TODO: Sätt till något bättre
            targetZSlider.value = 0;

            TextMeshProUGUI tmProXmin = targetXSliderMin.GetComponent<TextMeshProUGUI>();
            tmProXmin.text = targetXSlider.minValue.ToString();
            TextMeshProUGUI tmProXmax = targetXSliderMax.GetComponent<TextMeshProUGUI>();
            tmProXmax.text = targetXSlider.maxValue.ToString();

            TextMeshProUGUI tmProZmin = targetZSliderMin.GetComponent<TextMeshProUGUI>();
            tmProZmin.text = targetZSlider.minValue.ToString();
            TextMeshProUGUI tmProZmax = targetZSliderMax.GetComponent<TextMeshProUGUI>();
            tmProZmax.text = targetZSlider.maxValue.ToString();
        }
    }

    public void OnTargetXSliderChange()
    {
        target.transform.position = new Vector3(targetXSlider.value, target.transform.position.y, target.transform.position.z);
    }

    public void UpdateDepth()
    {
        World world = world_manager.GetComponent<World>();
        targetYSlider.minValue = world.GetWaterDepth();
        targetYSlider.maxValue = 0;
        targetYSlider.value = target.transform.position.y;        

        TextMeshProUGUI tmProYmin = targetYSliderMin.GetComponent<TextMeshProUGUI>();
        tmProYmin.text = targetYSlider.minValue.ToString();
        TextMeshProUGUI tmProYmax = targetYSliderMax.GetComponent<TextMeshProUGUI>();
        tmProYmax.text = targetYSlider.maxValue.ToString();        
    }

    public void OnTargetYSliderChange()
    {
        target.transform.position = new Vector3(target.transform.position.x, targetYSlider.value, target.transform.position.z);
    }

    public void OnTargetZSliderChange()
    {
        target.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, targetZSlider.value);
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

        if (isNumber && numericValue > 0)
        {
            BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
            bellhopParams.MAXNRSURFACEHITS = numericValue;
        }
    }

    public void OnBottomHitsChange()
    {        
        string str = maxNrOfBottomHits.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0)
        {
            BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
            bellhopParams.MAXNRBOTTOMHITS = numericValue;
        }
    }

    public void OnBellhopIterationsChange()
    {
        string str = bellhopIterations.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0)
        {
            BellhopParams bellhopParams = bellhop.GetComponent<BellhopParams>();
            bellhopParams.BELLHOPITERATIONS = numericValue;
        }
    }

    public void OnCallbackCommandEntered()
    {
        Debug.Log("shshj");
    }


}
