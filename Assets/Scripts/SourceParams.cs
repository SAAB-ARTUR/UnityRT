using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public class SourceParams : MonoBehaviour, ICloneable, IEquatable<SourceParams>
{
    public struct Properties {
        public int ntheta;
        public float theta;        
    }

    const int MIN_SIZE_ANG = 1;
    //const int MIN_INTERACTIONS = 1;

    public int theta = 0;
    [SerializeField]
    private int _ntheta = MIN_SIZE_ANG;
    public int ntheta { 
        get { return _ntheta; }
        set {
            if (value > MIN_SIZE_ANG) { _ntheta = value; }
            else { _ntheta = MIN_SIZE_ANG; }
        }
    }

    public bool sendRaysContinously = false;
    public bool visualizeRays = false;
    public bool showContributingRaysOnly = false;

    private void OnValidate()
    {   
        ntheta = _ntheta;
    }
    public object Clone()
    {
        SourceParams s = new SourceParams();
        s.theta = theta;
        s.ntheta = ntheta;  
        s.sendRaysContinously = sendRaysContinously;
        s.visualizeRays = visualizeRays;
        s.showContributingRaysOnly = showContributingRaysOnly;
        return s;

    }

    public Properties ToStruct() 
    {     
        Properties p = new Properties();
        p.theta = theta;
        p.ntheta = ntheta;
        return p;

    }

    public bool HasChanged(Properties? p) 
    {
        if (p is null) {
            return true;
        }

        Properties pp = this.ToStruct();       

        return !pp.Equals(p);
    }

    public bool Equals(SourceParams other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (
            this.theta == other.theta &&
            this.ntheta == other.ntheta
            ) { 
            return true;
        }
        return false;
    }
}