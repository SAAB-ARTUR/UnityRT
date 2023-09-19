
#include "BSSP.cginc"
#include "BStep.cginc"

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
    
    double xray; 
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
        double tau0 = tau;
        double len0 = len;
        
        // Take a step
        StepOutput stepOutput = bstep(soundSpeedProfile, x0, Tray, p, q, tau, len, deltas, depth, Layer);
        
        // Reflection to top and bottom
        // TODO: Accelerate with the acceleration structure
        if (x.g <= 0)
        {
            ntop++;
            Reflection reflection = reflect(stepOutput.c, stepOutput.cz, stepOutput.Tray);
            // TODO: Write reflection function.
            
        }
        
        
        
    }
    
    
    
}