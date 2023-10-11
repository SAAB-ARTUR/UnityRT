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

// similar to btrace, but this function is only called for contributing rays, meaning that some simplifications can be made
void btrace_eig(SSP soundSpeedProfile, float alpha, float dalpha, float2 xs, float2 xr, float depth, float deltas, uint maxtop,
    uint maxbot, float rayPhi, inout PerRayData prd, uint3 id)
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

    uint istep = 1;
        
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

        // distance between ray and receiver       
        current_distance = sqrt(pow((x.x - xr.x), 2) + pow((x.y - xr.y), 2));

        istep++;
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

    // sound speed: source, receiver, and bottom
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
    }

    // transmission loss
    rho = 1;
    float I1 = 1 / cs / rho;
    float I2 = Arms / freqsdamps / cr / rho;
    float TL = 10 * log10(I1 / I2);

    prd.TL = TL;
}