using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputFields : MonoBehaviour
{
    [SerializeField] GameObject srcSphere = null;
    [SerializeField] InputField ntheta = null;
    [SerializeField] InputField nphi = null;
    [SerializeField] InputField maxInteractions = null;
    [SerializeField] InputField theta = null;
    [SerializeField] InputField phi = null;    
    [SerializeField] GameObject world_manager = null;    
    [SerializeField] InputField range = null;
    [SerializeField] InputField depth = null;
    [SerializeField] InputField nrOfWaterplanes = null;

    // Start is called before the first frame update
    void Start()
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        ntheta.text = sourceParams.ntheta.ToString();
        nphi.text = sourceParams.nphi.ToString();
        maxInteractions.text = sourceParams.MAXINTERACTIONS.ToString();
        theta.text = sourceParams.theta.ToString();
        phi.text = sourceParams.phi.ToString();

        World world = world_manager.GetComponent<World>();
        range.text = world.range.ToString();
        depth.text = world.waterDepth.ToString();
        nrOfWaterplanes.text = world.nrOfWaterplanes.ToString();
           
    }

    // Update is called once per frame
    void Update()
    {
        
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
    public void OnMaxInteractionsChange()
    {
        string str = maxInteractions.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0)
        {
            SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
            sourceParams.MAXINTERACTIONS = numericValue;
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
    public void OnDepthChange()
    {
        string str = depth.text;
        bool isNumber = int.TryParse(str, out int numericValue);

        if (isNumber && numericValue > 0)
        {
            World world = world_manager.GetComponent<World>();
            world.waterDepth = numericValue;
        }
    }
    public void OnNrOfWaterlayersChange()
    {
        string str = nrOfWaterplanes.text;
        bool isNumber = int.TryParse(str, out int numericValue);        

        if (isNumber && numericValue > 0)
        {
            World world = world_manager.GetComponent<World>();
            world.nrOfWaterplanes = numericValue;
        }
    }
}
