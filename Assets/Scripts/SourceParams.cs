using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceParams : MonoBehaviour, ICloneable, IEquatable<SourceParams>
{


    public int theta = 0;
    public int ntheta = 0;
    public int phi = 0;
    public int nphi = 0;
    public int MAXINTERACTIONS = 0;

    public object Clone()
    {
        SourceParams s = new SourceParams();
        s.theta = theta;
        s.ntheta = ntheta;  
        s.phi = phi;
        s.nphi = nphi;
        s.MAXINTERACTIONS = MAXINTERACTIONS;
        return s;

    }

    public bool Equals(SourceParams other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (
            this.theta == other.theta &&
            this.ntheta == other.ntheta &&
            this.phi == other.phi &&
            this.nphi == other.nphi &&
            this.MAXINTERACTIONS == other.MAXINTERACTIONS
            ) { 
            return true;
        }
        return false;
    }

    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
