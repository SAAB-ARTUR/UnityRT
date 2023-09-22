

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
SSPOutput ssp(double z, SSP soundSpeedProfile, uint Layer)
{
    uint len;
    uint stride;
    _SSPBuffer.GetDimensions(len, stride);
    
    while (z >= _SSPBuffer[Layer].depth && Layer < len)
    {
        Layer = Layer + 1;
    }
    
    while (z < _SSPBuffer[Layer].depth && Layer > 0)
    {
        Layer = Layer - 1;
    }    
    
    double w = z - _SSPBuffer[Layer].depth;
    
    double c, cz, czz;
    switch (soundSpeedProfile.type)
    {
        default:
            {
                c = _SSPBuffer[Layer].velocity + w * _SSPBuffer[Layer].derivative1;
                cz = _SSPBuffer[Layer].derivative1;
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



double ReduceStep(double2 x0, double2 Tray, double zmin, double zmax, double c,  double deltas, double h)
{
    
    // Reduces the ray step size to make sure we land on interfaces and boundries
    double2 cTray = c * Tray;
    double2 x = x0 + h * cTray; // Make a trial step
    

    // LITE OKLART OM ALLT GÅR RÄTT TILL HÄR!!!!!!!
    
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
    h = max(h, 0.000001 * deltas); // 0.2 2e-1 * 1e-6
    
    return h;
}