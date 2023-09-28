using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellhopParams : MonoBehaviour
{
    [SerializeField]
    private int nrOfBellhopIntegrationSteps = 0;
    [SerializeField]
    private float bellhopIntegrationStepSize = 0;

    public int BELLHOPINTEGRATIONSTEPS
    {
        get { return nrOfBellhopIntegrationSteps; }
        set
        {
            if (value > 0) { nrOfBellhopIntegrationSteps = value; }
            else { nrOfBellhopIntegrationSteps = 1; }
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

    public struct Properties
    {
        public int nrOfBellhopIntegrationSteps;
        public float bellhopIntegrationStepSize;
    }

    public Properties ToStruct()
    {
        Properties p = new Properties();
        p.nrOfBellhopIntegrationSteps = nrOfBellhopIntegrationSteps;
        p.bellhopIntegrationStepSize = bellhopIntegrationStepSize;

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