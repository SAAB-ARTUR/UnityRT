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
    float c;
    float cz;
    float czz;
    uint Layer;
};


/*
Tabulates  the sound speed profile and its derivatives
Also returns a vector Layer indicating the layer a depth point is in

Layer is the index of the layer that each ray is in
SSP.z and SSP.c contains the depth/sound speed values
*/
SSPOutput ssp(float z, SSP soundSpeedProfile, uint Layer, uint istep, uint offset, bool ba)
{
    uint len;
    uint stride;
    _SSPBuffer.GetDimensions(len, stride);

    /*if (ba) {
        xrayBuf[istep + offset] = float2(z, Layer);
    }*/


    // search through deeper layers
    while (z <= _SSPBuffer[Layer + 1].depth && Layer < len) //remember that z should be negative, hence the <= operation
    {
        Layer = Layer + 1;
    }

    //search through shallower layers
    while (z > _SSPBuffer[Layer].depth && Layer > 0)
    {
        Layer = Layer - 1;
    }

    /*if (ba) {
        xrayBuf[istep + offset] = float2(z, Layer);
    }*/

    float w = z - _SSPBuffer[Layer].depth;

    float c, cz, czz;
    /*switch (soundSpeedProfile.type)
    {
        default:
        {
            c = _SSPBuffer[Layer].velocity + w * _SSPBuffer[Layer].derivative1;
            cz = _SSPBuffer[Layer].derivative1;
            czz = 0.0;
            break;
        }
    }*/

    c = _SSPBuffer[Layer].velocity + w * _SSPBuffer[Layer].derivative1;
    cz = _SSPBuffer[Layer].derivative1;
    czz = 0.0;

    // Construct and return the output
    SSPOutput result;
    result.c = c;
    result.cz = cz;
    result.czz = czz;
    result.Layer = Layer;

    return result;
}