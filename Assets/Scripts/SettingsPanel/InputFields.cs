using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputFields : MonoBehaviour
{
    [SerializeField] InputField ntheta = null;
    [SerializeField] InputField nphi = null;
    [SerializeField] InputField maxInteractions = null;
    [SerializeField] InputField theta = null;
    [SerializeField] InputField phi = null;
    [SerializeField] GameObject srcSphere = null;

    // Start is called before the first frame update
    void Start()
    {
        SourceParams sourceParams = srcSphere.GetComponent<SourceParams>();
        ntheta.text = sourceParams.ntheta.ToString();
        nphi.text = sourceParams.nphi.ToString();
        maxInteractions.text = sourceParams.MAXINTERACTIONS.ToString();
        theta.text = sourceParams.theta.ToString();
        phi.text = sourceParams.phi.ToString();
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
}
