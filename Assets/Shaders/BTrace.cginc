
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

#include "BStep.cginc"
#include "BReflect.cginc"

float3 toCartesian(float phi, float2 rz) {

    float radius = rz.x;
    float depth = rz.y;

    return float3(radius * cos(phi) + srcPosition.x, depth, radius * sin(phi) + srcPosition.z);

}

/*struct BRay
{
    uint ntop;
    uint nbot;
    uint ncaust;
    float delay;
    float curve;
    float xn;
    float qi;

};*/

struct TraceOutput
{   
    //BRay ray;
    float beta;
    uint ntop;
    uint nbot;
    uint ncaust;
    float delay;
    float curve;
    float xn;
    float qi;
};

TraceOutput btrace(
    SSP soundSpeedProfile,
    float alpha,
    float dalpha,
    float2 xs,
    float2 xr,
    float depth,
    float deltas,
    uint maxtop,
    uint maxbot,
    uint3 id,
    uint width,
    float3 raydir,
    float phi,
    inout PerRayData prd
)
{
    uint offset = (id.y * width + id.x) * _BELLHOPSIZE;
    SSPOutput initialSsp = ssp(xs.y, soundSpeedProfile, 0);
    
    // Initial conditions
    
    float c = initialSsp.c;
    float2 x = xs;
    float2 Tray = { cos(alpha) / c, -sin(alpha) / c }; //i matlab behövde jag lägga till ett minustecken för att det skulle bli rätt, här var jag tvungen att ta bort det, oklart varför.
    float p = 1;
    float q = 0;
    float tau = 0;
    float len = 0;
    uint ntop = 0;
    uint nbot = 0;
    uint ncaust = 0;
    uint Layer = initialSsp.Layer;

    float xxs = 1;
    float q0 = 0;
 

    float2 x0;
    float tau0;
    float len0;    

    float original_distance = sqrt(pow((srcPosition.x - receiverPosition.x), 2) + pow((srcPosition.y - receiverPosition.y), 2) + pow((srcPosition.z - receiverPosition.z), 2));    
    float current_distance = original_distance;
    float previous_distance = original_distance;
    float3 x0_cart;
    float3 x_cart;
        
    //debugBuf[id.y * width + id.x] = float3(previous_distance, current_distance, 24);
    //debugBuf[id.y * width + id.x] = float3(initialSsp.c, initialSsp.Layer, 24);
    xrayBuf[0 + offset] = toCartesian(phi, xs);
    debugBuf[0 + offset] = float3(Tray, Layer);
    uint istep = 1;

    bool add = true;

    //while (xxs > 0 && ntop <= maxtop && nbot <= maxbot && istep < _BELLHOPSIZE)
    while (current_distance <= previous_distance && ntop <= maxtop && nbot <= maxbot && istep < _BELLHOPSIZE)
    {
        // Apply caustic phase change
        if (q <= 0 && q0 > 0 || q >= 0 && q0 < 0)
        {
            ncaust++;
        }

        // Save data from previous step
        x0 = x;
        q0 = q;
        tau0 = tau;
        len0 = len;
        previous_distance = current_distance;

        // Take a step
        StepOutput stepOutput = bstep(soundSpeedProfile, x0, Tray, p, q, tau, len, deltas, depth, Layer, id, width);

        /*if (add) {
            add = false;
            debugBuf[id.y * width + id.x] = float3(stepOutput.c, stepOutput.cz, 12);
        }*/

        Tray = stepOutput.Tray;
        p = stepOutput.p;

        // Reflection to top and bottom
        // TODO: Accelerate with the acceleration structure
        if (stepOutput.x.y >= 0)
        {
            ntop++;
            Reflection reflection = breflect(stepOutput.c, stepOutput.cz, Tray, p, stepOutput.q);
            Tray = reflection.Tray;
            p = reflection.p;
        }

        if (stepOutput.x.y <= depth)
        {
            nbot++;
            Reflection reflection = breflect(stepOutput.c, stepOutput.cz, Tray, p, stepOutput.q);
            Tray = reflection.Tray;
            p = reflection.p;

        }

        // Reset for the next step
        c = stepOutput.c;
        x = stepOutput.x;
        q = stepOutput.q;
        tau = stepOutput.tau;
        len = stepOutput.len;

        x0_cart = toCartesian(phi, x0);
        x_cart = toCartesian(phi, x);
        
        // distance between ray and receiver
        current_distance = sqrt(pow((x_cart.x - receiverPosition.x), 2) + pow((x_cart.y - receiverPosition.y), 2) + pow((x_cart.z - receiverPosition.z), 2));

        // Distance left to the receiver
        //xxs = (x.x - x0.x) * (xr.x - x.x) + (x.y - x0.y) * (xr.y - x.y);

        xrayBuf[istep + offset] = x_cart;
        debugBuf[istep + offset] = float3(x, 12349);

        istep++;
    }

    // easy solution for buffer problem, positions that should be empty sometimes gets filled with weird values, therefore we force the empty positions to be empty
    for (uint i = istep; i < _BELLHOPSIZE; i++) { 
        xrayBuf[i + offset] = float3(0, 0, 0);
        debugBuf[i + offset] = float3(0, 0, 0);
    }

    // Calculate ray tangent
    float tr = x.x - x0.x;
    float tz = x.y - x0.y;
    float rlen = sqrt(tr * tr + tz * tz);
    tr = tr / rlen;
    tz = tz / rlen;


    // Interpolate

    float xn;
    float xs2;

    float diffx = xr.x - x0.x;
    float diffy = xr.y - x0.y;
    float diffx2 = x.x - x0.x;
    float diffy2 = x.y - x0.y;

    xs2 = tr * (xr.x - x0.x) + tz * (xr.y - x0.y); // proportional distance along ray
    xn = -tz * (xr.x - x0.x) + tr * (xr.y - x0.y); // normal distance to ray

    float s;
    s = xs2 / rlen;

    float qi;
    qi = q0 + s * (q - q0);

    float delay;
    delay = tau0 + s * (tau - tau0);


    float curve;
    curve = len0 + s * (len - len0);

    debugBuf[offset + _BELLHOPSIZE - 1] = float3(diffx, diffy, 128);
    debugBuf[offset + _BELLHOPSIZE - 2] = float3(diffx2, diffy2, 128); //det verkar som för den första bidragande strålen så blir det sista steget väldigt kort, jag vet ej om det är så att det sista steget måste vara stort för att koden ska funka bra
    debugBuf[offset + _BELLHOPSIZE - 3] = float3(tr, tz, rlen);
    debugBuf[offset + _BELLHOPSIZE - 4] = float3(xs2, xn, 128);
    debugBuf[offset + _BELLHOPSIZE - 5] = float3(s, q, q0);
    // Beam radius
    float RadMax = abs(qi) / initialSsp.c * dalpha;

    float beta = abs(xn) / RadMax;

    debugBuf[offset + _BELLHOPSIZE - 6] = float3(qi, initialSsp.c, alpha);
    debugBuf[offset + _BELLHOPSIZE - 7] = float3(beta, xn, RadMax); //INDEX 38 borde vara första strålen som bidrar
    
    // shift phase for rays that have passed through a caustic

    if (qi <= 0 && q0 > 0 || qi >= 0 && q0 < 0)
    {
        ncaust++;
    }
     
    // Create the output
    TraceOutput result;
    /*BRay r;
    r.curve = curve;
    r.delay = delay;
    r.nbot = nbot;
    r.ntop = ntop; 
    r.qi = qi;
    r.xn = xn; 
    r.ncaust = ncaust;*/  
    
    //result.xray = xrayBuf;
    result.beta = beta;
    //result.ray = r;
    result.ntop = ntop;
    result.nbot = nbot;
    result.ncaust = ncaust;
    result.delay = delay;
    result.curve = curve;
    result.xn = xn;
    result.qi = qi;
    
    prd.iseig = 0;
    prd.beta = beta;
    //prd.ray = r;
    prd.ntop = ntop;
    prd.nbot = nbot;
    prd.ncaust = ncaust;
    prd.delay = delay;
    prd.curve = curve;
    prd.xn = xn;
    prd.qi = qi;
    prd.alpha = alpha;

    if (beta < 1) {
        prd.contributing = 1;
    }
    else {
        prd.contributing = 0;
    }

    return result;
    
}