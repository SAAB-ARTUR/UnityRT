#include "BStep.cginc"
#include "BReflect.cginc"

float3 toCartesian(float phi, float2 rz) 
{
    float radius = rz.x;
    float depth = rz.y;

    return float3(radius * cos(phi) + srcPosition.x, depth, radius * sin(phi) + srcPosition.z);
}

void btrace(SSP soundSpeedProfile, float alpha, float dalpha, float2 xs, float2 xr, float depth, float deltas, uint maxtop,
            uint maxbot, uint offset, float phi, float rayPhi, inout PerRayData prd, uint3 id)
{    
    SSPOutput initialSsp = ssp(xs.y, soundSpeedProfile, 0);
    
    // Initial conditions    
    float c = initialSsp.c;
    float2 x = xs;
    float2 Tray = { cos(alpha) / c, -sin(alpha) / c };
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

    RayPositionsBuffer[0 + offset] = toCartesian(phi, xs);    
    uint istep = 1;

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
        StepOutput stepOutput = bstep(soundSpeedProfile, x0, Tray, p, q, tau, len, deltas, depth, Layer);

        Tray = stepOutput.Tray;
        p = stepOutput.p;

        // Reflection to top and bottom        
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
        //current_distance = sqrt(pow((x_cart.x - receiverPosition.x), 2) + pow((x_cart.y - receiverPosition.y), 2) + pow((x_cart.z - receiverPosition.z), 2));
        current_distance = sqrt(pow((x.x - xr.x), 2) + pow((x.y - xr.y), 2));

        // Distance left to the receiver
        //xxs = (x.x - x0.x) * (xr.x - x.x) + (x.y - x0.y) * (xr.y - x.y);

        RayPositionsBuffer[istep + offset] = x_cart;        

        istep++;
    }

    // easy solution for buffer problem, positions that should be empty sometimes gets filled with weird values, therefore we force an invalid float3 (positve y-coord is not possible) into the buffer that the cpu can look for
    for (uint i = istep; i < _BELLHOPSIZE; i++) { 
        RayPositionsBuffer[i + offset] = float3(0, 10, 0);        
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
    
    // Beam radius
    float RadMax = abs(qi) / initialSsp.c * dalpha;

    float beta = abs(xn) / RadMax;    
    
    // shift phase for rays that have passed through a caustic
    if (qi <= 0 && q0 > 0 || qi >= 0 && q0 < 0)
    {
        ncaust++;
    }
    
    // set data for the traced ray
    prd.beta = beta;    
    prd.ntop = ntop;
    prd.nbot = nbot;
    prd.ncaust = ncaust;
    prd.delay = delay;
    prd.curve = curve;
    prd.xn = xn;
    prd.qi = qi;
    prd.theta = alpha;
    prd.phi = rayPhi;
    prd.TL = 0;

    if (beta < 1) {// Detta är inte bra löst eftersom det utgår ifrån att sändaren tittar direkt på mottagaren
        // calculate angle between receiver, source and end of ray
        float origin_phi = atan2(srcDirection.z, srcDirection.x); // calculate angle that source is viewing at
        // rotate the receiver around the y-axis to have the z-ccordinate be 0
        float srcXnorm = receiverPosition.x * cos(-origin_phi) - receiverPosition.z * sin(-origin_phi);
        float srcZnorm = receiverPosition.z * cos(-origin_phi) + receiverPosition.x * sin(-origin_phi);

        float dx = srcXnorm - srcPosition.x;
        float dz = srcZnorm - x_cart.z;
       
        float angle = atan2(dz, dx);

        //debugBuf[id.y * nphi + id.x] = float3(angle, alpha, origin_phi);
        
        float kappa = 10; // kappa determines how much a ray can miss the target with in phi and still be considered to affect the target

        if (abs(angle) < kappa) {
            prd.contributing = 1;
        }
        else {
            prd.contributing = 0;
        }        
    }
    else {
        prd.contributing = 0;
    }
}