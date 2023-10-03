float ReduceStep(float2 x0, float2 Tray, float zmin, float zmax, float c, float deltas, float h)
{
    // Reduces the ray step size to make sure we land on interfaces and boundries
    float2 cTray = c * Tray;
    float2 x = x0 + h * cTray; // Make a trial step

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
    h = max(h, 1e-4 * deltas);

    return h;
}

struct StepOutput
{
    float2 x;
    float2 Tray;
    float q;
    float p;
    float tau;
    float len;
    float c;
    float cz;
    uint Layer;
};

StepOutput bstep(
    SSP soundSpeedProfile,
    float2 x0,
    float2 Tray0,
    float p0,
    float q0,
    float tau0,
    float len0,
    float deltas,
    float depth,
    uint Layer,
    uint3 id,
    uint width)
{
    SSPOutput phase0 = ssp(x0.y, soundSpeedProfile, Layer);

    float csq0 = phase0.c * phase0.c;
    float cnn0_csq0 = phase0.czz * Tray0.x * Tray0.x;

    //debugBuf[id.y * width + id.x] = float3(x0.y, Layer, 33);

    float zmax = max(_SSPBuffer[phase0.Layer].depth, depth);
    float zmin = max(_SSPBuffer[phase0.Layer + 1].depth, depth);

    float h0 = deltas;
    h0 = ReduceStep(x0, Tray0, zmin, zmax, phase0.c, deltas, h0);

    float hh = 0.5 * h0;
    float2 x1 = x0 +  hh * phase0.c * Tray0;    

    float2 mulvec0;
    mulvec0.x = 0;
    mulvec0.y = phase0.cz;

    float2 Tray1 = Tray0 - hh * mulvec0 / csq0;
    float p1 = p0 - hh * cnn0_csq0 * q0;
    float q1 = q0 + hh * phase0.c * p0;

    SSPOutput phase1 = ssp(x1.y, soundSpeedProfile, phase0.Layer);
    float csq1 = phase1.c * phase1.c;
    float cnn1_csq1 = phase1.czz * (Tray1.x * Tray1.x);

    float h1 = ReduceStep(x0, Tray1, zmin, zmax, phase1.c, deltas, h0);

    float2 mulvec1;
    mulvec1.x = 0.0;
    mulvec1.y = phase1.cz;

    float w1 = h1 / h0;
    float w0 = 1 - w1;

    float2 x = x0 + h1 * (w0 * phase0.c * Tray0 + w1 * phase1.c * Tray1);

    float2 Tray = Tray0 - h1 * (w0 * mulvec0 / csq0 + w1 * mulvec1 / csq1);
    float p = p0 - h1 * (w0 * cnn0_csq0 * q0 + w1 * cnn1_csq1 * q1);
    float q = q0 + h1 * (w0 * phase0.c * p0 + w1 * phase1.c * p1);

    float tau = tau0 + h1 * (w0 / phase0.c + w1 / phase1.c);
    float len = len0 + h1;    

    SSPOutput phase2 = ssp(x.y, soundSpeedProfile, phase1.Layer);

    if (!(phase2.Layer == phase0.Layer))
    {
        float RN = -Tray.x * Tray.x / Tray.y * (phase2.cz - phase0.cz) / phase0.c;
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