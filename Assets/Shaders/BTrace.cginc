#include "BStep.cginc"
#include "BReflect.cginc"

float3 toCartesian(float phi, float2 rz) 
{
    float radius = rz.x;
    float depth = rz.y;

    return float3(radius * cos(phi) + srcPosition.x, depth, radius * sin(phi) + srcPosition.z);
}

void btrace(SSP soundSpeedProfile, float alpha, float dalpha, float2 xs, float2 xr, float depth, float deltas, uint maxtop,
            uint maxbot, uint offset, float phi, float rayPhi, inout PerRayData prd)
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

    rayPositionsBuffer[0 + offset] = toCartesian(phi, xs);    
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
        //current_distance = sqrt(pow((x_cart.x - receiverPosition.x), 2) + pow((x_cart.y - receiverPosition.y), 2) + pow((x_cart.z - receiverPosition.z), 2));
        current_distance = sqrt(pow((x.x - xr.x), 2) + pow((x.y - xr.y), 2));

        // Distance left to the receiver
        //xxs = (x.x - x0.x) * (xr.x - x.x) + (x.y - x0.y) * (xr.y - x.y);

        rayPositionsBuffer[istep + offset] = x_cart;        

        istep++;
    }

    // easy solution for buffer problem, positions that should be empty sometimes gets filled with weird values, therefore we force an invalid float3 (positve y-coord is not possible) into the buffer that the cpu can look for
    for (uint i = istep; i < _BELLHOPSIZE; i++) { 
        rayPositionsBuffer[i + offset] = float3(0, 10, 0);        
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
        // calculate angle between source, receiver and end of ray
        float origin_phi = atan2(srcDirection.z, srcDirection.x); // calculate angle that source is viewing at
        // rotate the receiver around the y-axis to have the z-ccordinate be 0
        float srcXnorm = receiverPosition.x * cos(-origin_phi) - receiverPosition.z * sin(-origin_phi);
        float srcZnorm = receiverPosition.z * cos(-origin_phi) + receiverPosition.x * sin(-origin_phi);

        float dx = srcXnorm - srcPosition.x;
        float dz = srcZnorm - x_cart.z;
       
        float angle = atan2(dz, dx);

        float kappa = 10;

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

float2 complexsqrt(float a, float b) 
{
    float x, y;
    if (a >= 0) {
        x = sqrt((sqrt(a * a + b * b) + a) / 2);
        if (x > 0) {
            y = b / x / x;
        }
        else {
            y = 0;
        }
    }
    else {
        y = sqrt((sqrt(a * a + b * b) - a) / 2);
        if (b < 0) {
            y = -y;
        }
        x = b / y / 2;
    }
    return float2(x, y);
}


float2 bottom_reflection(float cwater, float cp, float rho, float bottom_alpha, float Tg, float omega, float alphaT)
{
    float alpha = bottom_alpha / 8.6858896f + alphaT;
    float ci = alpha * pow(cp, 2) / omega;

    float g1 = pow(Tg, 2) - 1 / pow(cwater, 2);
    float h1 = 0;

    float cp2 = cp * cp + ci * ci;
    float x = cp / cp2;
    float y = -ci / cp2;
    float g2 = pow(Tg, 2) - x * x + y * y;
    float h2 = -2 * x * y;

    float2 g1h1 = complexsqrt(g1, h1);
    float2 g2h2 = complexsqrt(g2, h2);

    float A = rho * g1h1.x - g2h2.x;
    float B = rho * g1h1.y - g2h2.y;
    float C = rho * g1h1.x + g2h2.x;
    float D = rho * g1h1.y + g2h2.y;
    float R = (A * C + B * D) / (C * C + D * D);
    float Q = (B * C - A * D) / (C * C + D * D);

    float Rfa = sqrt(R * R + Q * Q);
    float gamma = atan2(Q, R);

    return float2(Rfa, gamma);

}


void btrace_eig(SSP soundSpeedProfile, float alpha, float dalpha, float2 xs, float2 xr, float depth, float deltas, uint maxtop,
    uint maxbot, float phi, float rayPhi, inout PerRayData prd, uint3 id)
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
    /*float3 x0_cart;
    float3 x_cart;*/

    //rayPositionsBuffer[0 + offset] = toCartesian(phi, xs);
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

        /*x0_cart = toCartesian(phi, x0);
        x_cart = toCartesian(phi, x);*/

        // distance between ray and receiver
        //current_distance = sqrt(pow((x_cart.x - receiverPosition.x), 2) + pow((x_cart.y - receiverPosition.y), 2) + pow((x_cart.z - receiverPosition.z), 2));
        current_distance = sqrt(pow((x.x - xr.x), 2) + pow((x.y - xr.y), 2));

        // Distance left to the receiver
        //xxs = (x.x - x0.x) * (xr.x - x.x) + (x.y - x0.y) * (xr.y - x.y);

        //rayPositionsBuffer[istep + offset] = x_cart;

        istep++;
    }

    // easy solution for buffer problem, positions that should be empty sometimes gets filled with weird values, therefore we force an invalid float3 (positve y-coord is not possible) into the buffer that the cpu can look for
    /*for (uint i = istep; i < _BELLHOPSIZE; i++) {
        rayPositionsBuffer[i + offset] = float3(0, 10, 0);
    }*/

    // Calculate ray tangent
    float tr = x.x - x0.x;
    float tz = x.y - x0.y;
    float rlen = sqrt(tr * tr + tz * tz);
    tr = tr / rlen;
    tz = tz / rlen;

    // Interpolate
    float xn;
    float xs2;

    /*float diffx = xr.x - x0.x;
    float diffy = xr.y - x0.y;
    float diffx2 = x.x - x0.x;
    float diffy2 = x.y - x0.y;*/

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

    if (beta < 1) {
        prd.contributing = 1; // this should always be the case for rays traced in this function
    }
    else {
        prd.contributing = 0;
    }

    // calculate transmission loss

    float cp = 1600; // m/s
    float rho = 1.8f; // rho/rho0
    float bottom_alpha = 0.025f; // dB/m

    // sound speed: source, receiver
    float cs = initialSsp.c;
    float cr = ssp(xr.y, soundSpeedProfile, 0).c;
    float cwater = ssp(depth, soundSpeedProfile, 0).c;
    

    // arrays
    //float Amp[freqsdamps];
    //float Phase[freqsdamps];
    float Amp;
    float Phase;

    // amplitudes
    float Arms = 0;
    float Amp0 = sqrt(cos(alpha) * cr / abs(qi) / xr.x);

    // ray tangent in r-direction
    float Tg = cos(alpha) / cs;
        
    for (uint i = 0; i < freqsdamps; i++) {

        float Rfa, gamma;
        // bottom reflection coefficient
        if (nbot > 0) {
            float omega = 2 * PI * FreqsAndDampData[i].x;
            float2 RfaGamma = bottom_reflection(cwater, cp, rho, bottom_alpha, Tg, omega, FreqsAndDampData[i].y);
            Rfa = RfaGamma.x;
            gamma = RfaGamma.y;
        }
        else {
            Rfa = 1;
            gamma = 0;
        }

        // amplitude and phase
        Amp = Amp0 * pow(Rfa, nbot) * exp(-FreqsAndDampData[i].y * curve);
        gamma = PI * ntop + gamma * nbot + PI / 2 * ncaust;
        Phase = (gamma + PI) % (2 * PI) - PI;

        // RMS amplitude
        Arms += pow(Amp, 2);

        /*if (!iseig) {
            Amp *= (1 - beta);
        }*/
        debugBuf[id.y] = float3(-FreqsAndDampData[i].y, curve, 170);
    }    

    // transmission loss
    rho = 1;
    float I1 = 1 / cs / rho;
    float I2 = Arms / freqsdamps / cr / rho;
    float TL = 10 * log10(I1 / I2);

    

    prd.TL = TL;
}