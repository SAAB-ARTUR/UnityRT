
struct SSP
{
    // 0: Default (Linear? Jonasa: otherwise)
    // 1: Q: Quadratic 
    uint type; 
    
    // Contains points on speed profile
    // data[0] is the first depth 
    // data[0].r (Jonas z) is the depth of the sound speed profile point
    // data[0].g (Jonas c) is the corresponding sound speed. 
    RWTexture1D<double2> SSP; 
    
    
    
};

struct BSSPStruct
{
    double c;
    double cz;
    double czz;
    uint Layer;
};

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