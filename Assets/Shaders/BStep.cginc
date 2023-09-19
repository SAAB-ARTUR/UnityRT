
#include "BSSP.cginc"

struct StepOutput
{
    double2 x;
    double2 Tray;
    double q;
    double p;
    double tau;
    double len;
    double c;
    double cz;
    uint Layer;
    
};

StepOutput bstep(
    SSP soundSpeedProfile, 
    double2 x0, 
    double2 Tray0, 
    double p0, 
    double q0, 
    double tau0, 
    double len0, 
    double deltas, 
    double depth, 
    uint Layer
    ) 
{
    
    
    SSPOutput phase0 = ssp(x0.r, soundSpeedProfile, Layer);
    
    double csq0 = phase0.c * phase0.c;
    double cnn0_csq0 = phase0.czz * Tray0.r * Tray0.r;
    
    double zmin = min(soundSpeedProfile.SSP[Layer].r, depth);
    double zmax = min(soundSpeedProfile.SSP[Layer + 1].r, depth);
    
    
    
    double h0 = deltas;
    h0 = ReduceStep(x0, Tray0, zmin, zmax, phase0.c, deltas, h0);
    
    double hh = 0.5 * h0; 

    double2 x1 = x0 + hh * phase0.c * Tray0;
    
    double2 mulvec0;
    mulvec0.r = 0;
    mulvec0.g = phase0.cz;

    double2 Tray1 = Tray0 - hh * mulvec0 / csq0;
    
    
    double p1 = p0 - hh * cnn0_csq0 * q0;
    double q1 = q0 + hh * phase0.c * p0;
    
    
    
    SSPOutput phase1 = ssp(x1.g, soundSpeedProfile, phase0.Layer);
    double csq1 = phase1.c * phase1.c;
    double cnn1_csq1 = phase1.czz * (Tray1.r * Tray1.r);
    
    double h1 = ReduceStep(x0, Tray1, zmin, zmax, phase1.c, deltas, h0);
    
    double2 mulvec1;
    mulvec1.r = 0.0;
    mulvec1.g = phase1.cz;
    
    
    double w1 = h1 / h0;
    double w0 = 1 - w1;
    
    double2 x = x0 + h1 * (w0 * phase0.c * Tray0 + w1 * phase1.c * Tray1);
    double2 Tray = Tray0 - h1 * (w0 * mulvec0 / csq0 + w1 * mulvec1 / csq1);
    double p = p0 - h1 * (w0 * cnn0_csq0 * q0 + w1 * cnn1_csq1 * q1);
    double q = q0 + h1 * (w0 * phase0.c * p0 + w1 * phase1.c * p1);
    
    
    double tau = tau0 + h1 * (w0 / phase0.c + w1 / phase1.c);
    double len = len0 + h1;
    
    SSPOutput phase2 = ssp(x.g, soundSpeedProfile, phase1.Layer);

    if (!(phase2.Layer == phase0.Layer))
    {
    
        double RN = -Tray.r * Tray.r / Tray.g * (phase2.cz - phase0.cz) / phase0.c;
        p = p + q * RN;
            
    }
    
    StepOutput result;
    result.x = x;
    result.Tray = Tray;
    result.p = p;
    result.q = q;
    result.tau = tau;
    result.len = len;
    result.c = phase2.c;
    result.cz = phase2.cz;
    result.Layer = phase2.Layer;
    
    return result;
}



/*
double ReduceStep(double2 x0, double2 Tray, double zmin, double zmax, double c,  double deltas, double h)
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