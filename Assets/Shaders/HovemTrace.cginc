/*float3 toCartesian(float phi, float2 rz) // toCartesian for Hovem should be the same as for BellHop so no need to redefine it here
{
    float radius = rz.x;
    float depth = rz.y;

    return float3(radius * cos(phi) + srcPosition.x, depth, radius * sin(phi) + srcPosition.z);
}*/ 

void HovemTrace(SSP soundSpeedProfile, float theta, float2 xs, float2 xr, float depth, uint maxtop,
    uint maxbot, uint offset, float phi, inout PerRayData prd, uint3 id)
{

    // find current layer
    uint ilay = 0;
    while (_SSPBuffer[ilay].depth > xs.y) {
        ilay++;
    }    

    // initial conditions
    float c = _SSPBuffer[ilay].velocity;
    float r = xs.x;
    float z = xs.y;
    float ksi = cos(theta) / c;
    float tz = -sin(theta);
    float tau = 0;
    float len = 0;
    uint ntop = 0;
    uint nbot = 0;

    

    // avoid tz == 0, if possible
    uint nlay;
    uint stride;
    _SSPBuffer.GetDimensions(nlay, stride);
    if (ilay == 0) {
        tz = -abs(tz) - 1e-32;
    }
    else if (ilay > nlay-1) {
        tz = abs(tz) + 1e-32;
    }
    else if (tz == 0) {
        float cz1 = _SSPBuffer[ilay - 1].derivative1;
        float cz2 = _SSPBuffer[ilay].derivative1;
        if (cz1 > 0 || cz2 < 0) {
            tz = -1e-32 * sign(cz1 + cz2);
        }
    }

    float c0, r0, z0, tz0, tau0, len0, ilay0, dz;

    float xxs = 1;
    
    RayPositionsBuffer[0 + offset] = toCartesian(phi, xs);
    uint istep = 1;

    while (xxs > 0 && ntop <= maxtop && nbot <= maxbot && istep < _MAXSTEPS) 
    {
        // save data from previous step
        c0 = c;
        r0 = r;
        z0 = z;
        tz0 = tz;
        tau0 = tau;
        len0 = len;
        ilay0 = ilay;
        dz = 0;

        // take a step
        if (tz == 0) {
            // horizontal step
            r = xr.x;
            tau = tau0 + (r - r0) / c;
            len = len0 + (r - r0);
        }
        else {
            // next layer
            float cz;
            if (tz < 0) {
                cz = _SSPBuffer[ilay].derivative1;
                ilay = ilay + 1;
            }
            else {
                ilay = ilay - 1;
                cz = _SSPBuffer[ilay].derivative1;
            }

            // curvature
            float kappa = cz * ksi;

            // sound speed
            c = _SSPBuffer[ilay].velocity;
            float tr = c * ksi;

            if (tr < 1) {
                // continue in this direction
                tz = sign(tz) * sqrt(1 - tr * tr);
                z = _SSPBuffer[ilay].depth;
            }
            else {
                // U-turn
                c = c0;
                tz = -tz;
                ilay = ilay0;
                dz = (1 / ksi - c) / cz;
            }

            // integrate r, tau and len
            if (kappa != 0) {
                // arc
                r = r0 + (tz0 - tz) / kappa;
                tau = tau0 + abs(log(c / c0 * (1 + tz0) / (1 + tz)) / cz);
                float ds = sqrt(pow(r - r0, 2) + pow(z - z0, 2));
                len = len0 + ds * (1 + pow(kappa * ds, 2) / 24);
            }
            else {
                // line segment
                float ds = (z - z0) / tz;
                r = r0 + tr * ds;
                tau = tau0 + ds / c;
                len = len0 + ds;
            }

            // reflections
            if (ilay == 0) {
                ntop++;
                tz = -tz;
            }
            if (ilay >= nlay - 1) { // TODO: av nån anledning blir det knäppt när man har bottenstuds
                nbot++;
                tz = -tz;
            }
        }

        // distance left to receiver
        xxs = (r - r0) * (xr.x - r) + (z - z0) * (xr.y - z);

        // save ray coordinates 
        RayPositionsBuffer[istep + offset] = toCartesian(phi, float2(r, z));
        istep++;
    }

    // easy solution for buffer problem, positions that should be empty sometimes gets filled with weird values, therefore we force an invalid float3 (positve y-coord is not possible) into the buffer that the cpu can look for
    for (uint i = istep; i < _MAXSTEPS; i++) {
        RayPositionsBuffer[i + offset] = float3(0, 10, 0);
    }    

    // calculate ray tangent for segment
    float tr = r - r0;
    tz = z - z0;
    float rlen = sqrt(tr * tr + tz * tz);
    tr = tr / rlen;
    tz = tz / rlen;    

    
    // local receiver coordinates
    float xt = tr * (xr.x - r0) + tz * (xr.y - z0); // proportional distance along ray
    float xn = -tz * (xr.x - r0) + tr * (xr.y - z0); // normal distance to ray
    
    // interpolate
    float s = xt / rlen;
    float delay = tau0 + s * (tau - tau0); // interpolated time delay
    float curve = len0 + s * (len - len0); // interpolated curve length

    // set data for the traced ray
    prd.theta = theta;
    prd.phi = phi; 
    prd.delay = delay;
    prd.curve = curve;
    prd.ntop = ntop;
    prd.nbot = nbot;
    prd.xn = xn;
    prd.target = id.x;
    prd.TL = 0; // calculated later
    prd.ncaust = 0; // not for hovem
    prd.beta = 0; // not for hovem
    prd.qi = 0; // not for hovem    
    prd.contributing = 0; // not for hovem

}