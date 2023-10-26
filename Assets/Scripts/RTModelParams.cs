using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTModelParams : MonoBehaviour
{
    [SerializeField]
    private int nrOfIntegrationSteps = 0;
    [SerializeField]
    private float bellhopIntegrationStepSize = 0;
    [SerializeField]
    private int maxNrSurfaceHits = 0;
    [SerializeField]
    private int maxNrBottomHits = 0;

    public enum RT_Model
    {
        Bellhop,
        Hovem
    }

    private RT_Model rtmodel = RT_Model.Hovem;

    public RT_Model RTMODEL
    {
        get { return rtmodel; }
        set
        {
            rtmodel = value;
        }
    }

    public int INTEGRATIONSTEPS
    {
        get { return nrOfIntegrationSteps; }
        set
        {
            if (value > 0) { nrOfIntegrationSteps = value; }
            else { nrOfIntegrationSteps = 1; }
        }
    }

    public float BELLHOPSTEPSIZE
    {
        get { return bellhopIntegrationStepSize; }
        set
        {
            if (value > 0) { bellhopIntegrationStepSize = value; }
            else { bellhopIntegrationStepSize = 1; }
        }
    }

    public int MAXNRSURFACEHITS
    {
        get { return maxNrSurfaceHits; }
        set
        {
            if (value >= 0) { maxNrSurfaceHits = value; }
            else { maxNrSurfaceHits = 0; }
        }
    }

    public int MAXNRBOTTOMHITS
    {
        get { return maxNrBottomHits; }
        set
        {
            if (value >= 0) { maxNrBottomHits = value; }
            else { maxNrBottomHits = 0; }
        }
    }    

    public struct Properties
    {
        public int nrOfIntegrationSteps;
        public float bellhopIntegrationStepSize;
        public RT_Model rtmodel;
    }

    public Properties ToStruct()
    {
        Properties p = new Properties();
        p.nrOfIntegrationSteps = nrOfIntegrationSteps;
        p.bellhopIntegrationStepSize = bellhopIntegrationStepSize;
        p.rtmodel = rtmodel;

        return p;
    }

    public bool HasChanged(Properties? p)
    {
        if (p is null)
        {
            return true;
        }

        Properties pp = this.ToStruct();        

        return !pp.Equals(p);
    }
}
