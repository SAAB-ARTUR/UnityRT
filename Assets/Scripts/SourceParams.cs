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

        public float phi;
        public int nphi;
        //public int MAXINTERACTIONS;
        public bool sendRaysContinously;
        public bool visualizeRays;
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

    /*public int MAXINTERACTIONS {
        get { return _maxInteractions; }
        set
        {
            if (value > MIN_INTERACTIONS) { _maxInteractions = value; }
            else { _maxInteractions = MIN_INTERACTIONS; }
        }
    }*/

    //[SerializeField]
    //private int _maxInteractions;

    public bool sendRaysContinously = false;
    public bool visualizeRays = false;

    private void OnValidate()
    {   
        ntheta = _ntheta;
        nphi = _nphi;
        //MAXINTERACTIONS = _maxInteractions;

    }
    public object Clone()
    {
        SourceParams s = new SourceParams();
        s.theta = theta;
        s.ntheta = ntheta;  
        s.phi = phi;
        s.nphi = nphi;
        //s.MAXINTERACTIONS = MAXINTERACTIONS;
        s.sendRaysContinously = sendRaysContinously;
        s.visualizeRays = visualizeRays;
        return s;

    }

    public Properties ToStruct() 
    {     
        Properties p = new Properties();
        p.theta = theta;
        p.ntheta = ntheta;

        p.phi = phi;    
        p.nphi = nphi;

        //p.MAXINTERACTIONS = MAXINTERACTIONS;
        p.sendRaysContinously = sendRaysContinously;
        p.visualizeRays = visualizeRays;
        return p;

    }

    public bool HasChanged(Properties? p) 
    {
        if (p is null) {
            //Debug.Log("Pp inte satt");
            return true;
        }

        Properties pp = this.ToStruct();
        //Debug.Log("pp satt");
        //Debug.Log(pp.theta);
        //Debug.Log(pp.ntheta);
        //Debug.Log(pp.phi);
        //Debug.Log(pp.nphi);
        //Debug.Log("-------------------------------------");

        return !pp.Equals(p);
    }

    public bool Equals(SourceParams other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (
            this.theta == other.theta &&
            this.ntheta == other.ntheta &&
            this.phi == other.phi &&
            this.nphi == other.nphi //&&
            //this.MAXINTERACTIONS == other.MAXINTERACTIONS
            ) { 
            return true;
        }
        return false;
    }
}