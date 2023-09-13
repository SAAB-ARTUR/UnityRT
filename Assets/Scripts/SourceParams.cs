using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SourceParams : MonoBehaviour, ICloneable, IEquatable<SourceParams>
{
    const int MIN_SIZE_ANG = 8;
    const int MIN_INTERACTIONS = 1;


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


    public int phi = 0;
    [SerializeField]
    private int _nphi = MIN_SIZE_ANG;
    public int nphi
    {
        get { return _nphi; }
        set
        {
            if (value > MIN_SIZE_ANG) { _nphi = value; }
            else { _nphi = MIN_SIZE_ANG; }
        }
    }


    public int MAXINTERACTIONS {
        get { return _maxInteractions; }
        set
        {
            if (value > MIN_INTERACTIONS) { _maxInteractions = value; }
            else { _maxInteractions = MIN_INTERACTIONS; }
        }
    }

    [SerializeField]
    private int _maxInteractions;



    private void OnValidate()
    {   
        ntheta = _ntheta;
        nphi = _nphi;
        MAXINTERACTIONS = _maxInteractions;

    }
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


    // Update is called once per frame
    void Update()
    {
        // Ensure that the source params are within valid range
        if (this.ntheta < 1) {
            this.ntheta = 1;
        }
        if (this.nphi < 1)
        {
            this.nphi = 1;
        }
        if (this.MAXINTERACTIONS < 1)
        {
            this.MAXINTERACTIONS = 1;
        }
    }
}
