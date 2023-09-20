
#include "BSSP.cginc"
#include "BStep.cginc"
#include "BReflect.cginc"

struct Ray
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
    
    RWTexture1D<double2> xray; 
    Ray ray;
    double beta;
    
};

TraceOutput trace(
    SSP soundSpeedProfile, 
    double alpha, 
    double dalpha, 
    double2 xs, 
    double2 xr, 
    double depth, 
    double deltas, 
    uint maxtop,   
    uint maxbot, 
    RWTexture1D<double2> xrayBuf
)
{

    
    xrayBuf[0] = xs;
    
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
        
        xrayBuf[istep+1] = stepOutput.x;
        
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
    Ray r;
    r.curve = curve;
    r.delay = delay;
    r.nbot = nbot;
    r.ntop = ntop; 
    r.qi = qi;
    r.xn = xn; 
    r.ncaust = ncaust;
    
    
    result.xray = xrayBuf;
    result.beta = beta;
    result.ray = r;
    
    return result;
    
}