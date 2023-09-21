
/*
struct SSP
{
    // 0: Default (Linear? Jonas: otherwise)
    // 1: Q: Quadratic 
    uint type;
    
    // Contains points on speed profile
    // data[0] is the first depth 
    // data[0].r (Jonas z) is the depth of the sound speed profile point
    // data[0].g (Jonas c) is the corresponding sound speed. 
    // data[0].b (Jonas cz) is the corresponding change in the sound speed profile. 
    RWTexture1D<double3> SSP;
    
    
    
};

struct SSPOutput
{
    double c;
    double cz;
    double czz;
    uint Layer;
};


/* 
Tabulates  the sound speed profile and its derivatives
Also returns a vector Layer indicating the layer a depth point is in

Layer is the index of the layer that each ray is in
SSP.z and SSP.c contains the depth/sound speed values
*/
/*
SSPOutput ssp(double z, SSP soundSpeedProfile, uint Layer)
{
    uint len;
    soundSpeedProfile.SSP.GetDimensions(len);
    
    
    
    while (z >= soundSpeedProfile.SSP[Layer].r && Layer < len)
    {
        Layer = Layer + 1;
    }
    
    while (z < soundSpeedProfile.SSP[Layer].r && Layer > 0)
    {
        Layer = Layer - 1;
    }
    
    double w = z - soundSpeedProfile.SSP[Layer].r;
    
    double c, cz, czz;
    switch (soundSpeedProfile.type)
    {
        default:
            {
                c = soundSpeedProfile.SSP[Layer].g + w * soundSpeedProfile.SSP[Layer].b;
                cz = soundSpeedProfile.SSP[Layer].b;
                czz = 0.0;
            }
    }
    
    // Construct and return the output
    SSPOutput result;
    result.c = c;
    result.cz = cz;
    result.czz = czz;
    result.Layer = Layer;
    
    return result;

}

double ReduceStep(double2 x0, double2 Tray, double zmin, double zmax, double c, double deltas, double h)
{
    
    // Reduces the ray step size to make sure we land on interfaces and boundries
    double2 cTray = c * Tray;
    double2 x = x0 + h * cTray; // Make a trial step
    
    
    // This could probably be solved by the acceleration struct? 
    if (x.y < zmin)
    {
        
        h = (zmin - x0.y) / cTray.y;
        
    }
    if (x.y > zmax)
    {
        
        h = (zmax - x0.y) / cTray.y;
        
    }
    
    
    // Ensure that we make at least a little step. 
    h = max(h, 0.000001 * deltas);
    
    return h;
}
*/

//#include "BSSP.cginc"
#include "BStep.cginc"
#include "BReflect.cginc"


uint2 elem = { 17,17 };


struct BRay
{

    uint ntop;
    uint nbot;
    uint ncaust;
    double delay;
    double curve;
    double xn;
    double qi;

};

struct TraceOutput
{
   
    BRay ray;
    double beta;
    
};

TraceOutput btrace(
    SSP soundSpeedProfile, 
    double alpha, 
    double dalpha, 
    double2 xs, 
    double2 xr, 
    double depth, 
    double deltas, 
    uint maxtop,   
    uint maxbot,
    uint3 id, 
    uint width
)
{
        
    // https://coderwall.com/p/fzni3g/bidirectional-translation-between-1d-and-3d-arrays
    // I want x -> iteration index
    // y -> phi
    // z -> theta
    uint offset = _BELLHOPSIZE * id.x + id.y * _BELLHOPSIZE * width;
    
    
    if (id.y == elem.y && id.x == elem.x)
    {
        
        xrayBuf[0 + offset] = xs;
        double2 tmpData = { 15, 15 };
        xrayBuf[0 + offset] = tmpData;
    }
     
    SSPOutput initialSsp = ssp(xs.g, soundSpeedProfile, 1);
    
    // Initial conditions
    
    double c = initialSsp.c;
    double2 x = xs;
    double2 Tray = { cos(alpha)/c, sin(alpha)/c };
    double p = 1;
    double q = 0;
    double tau = 0;
    double len = 0;
    uint ntop = 0;
    uint nbot = 0;
    uint ncaust = 0;
    uint Layer = initialSsp.Layer;
    
    double xxs = 1;
    double q0 = 0;
    uint istep = 0;
    
    
    double2 x0;
    double tau0;
    double len0;
    
    while (xxs > 0 && ntop <= maxtop && nbot <= maxbot)
    {
        istep++;
        
        // Apply caustic phase change
        if (q <= 0 && q0 > 0 || q >= 0 && q0 < 0)
        {
            ncaust++;
        }
        
        // Save data from previous step
        double2 x0 = x;
        q0 = q;
        tau0 = tau;
        len0 = len;
        
        // Take a step
        StepOutput stepOutput = bstep(soundSpeedProfile, x0, Tray, p, q, tau, len, deltas, depth, Layer);
        
        Tray = stepOutput.Tray;
        p = stepOutput.p;
        
        // Reflection to top and bottom
        // TODO: Accelerate with the acceleration structure
        if (stepOutput.x.g <= 0)
        {
            ntop++;
            Reflection reflection = breflect(stepOutput.c, stepOutput.cz, Tray, p, stepOutput.q);
            Tray = reflection.Tray;
            p = reflection.p;
        }
        
        if (stepOutput.x.g >= depth)
        {
            nbot++; 
            Reflection reflection = breflect(stepOutput.c, stepOutput.cz, Tray, p, stepOutput.q);
            Tray = reflection.Tray;
            p = reflection.p;
            
        }
        
        
        
        if (true) // (id.x == elem.x && id.y == elem.y)
        {
            //xrayBuf[istep+1 + offset] = stepOutput.x;
            double2 tmpData = id.xy; //{ 16, 16 };
            
            xrayBuf[istep + offset] = tmpData;
        }
        //xrayBuf[istep + offset] = id.xy;
        
        
        // Reset for the next step
        c = stepOutput.c; 
        x = stepOutput.x;
        q = stepOutput.q;
        tau = stepOutput.tau;
        len = stepOutput.len;
        // Distance left to the reciever
        xxs = (x.x - x0.x) * (xr.x - x.x) + (x.y - x0.y) * (xr.x - x.y);
        
        
    }
    
    // Calculate ray tangent
    double tr = x.x - x0.x;
    double tz = x.y - x0.y;
    double rlen =  sqrt(tr*tr + tz*tz);
    tr = tr / rlen;
    tz = tz / rlen;
    
    
    // Interpolate
    
    double2 xn;

    xs = tr * (xr.x - x0.x) + tz * (xr.y - x0.y); // proportional distance along ray
    xn = -tz * (xr.x - x0.x) + tr * (xr.y - x0.y); // normal distance to ray
    
    double s;
    s = xs / rlen;
    
    double qi;
    qi = q0 + s * (q - q0);
    
    double delay; 
    delay = tau0 + s * (tau - tau0); 
    
    
    double curve;
    curve = len0 + s * (len - len0);
    
    // Beam radius
    double RadMax = abs(qi) / initialSsp.c * dalpha; 
    
    double beta = abs(xn) / RadMax; 
    
    // shift phase for rays that have passed through a caustic

    if (qi <= 0 && q0 > 0 || qi >= 0 && q0 < 0)
    {
        ncaust++;
    }
     
    // Create the output
    TraceOutput result;
    BRay r;
    r.curve = curve;
    r.delay = delay;
    r.nbot = nbot;
    r.ntop = ntop; 
    r.qi = qi;
    r.xn = xn; 
    r.ncaust = ncaust;
    
    
    //result.xray = xrayBuf;
    result.beta = beta;
    result.ray = r;
    
    return result;
    
}