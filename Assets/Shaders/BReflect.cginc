struct Reflection
{
    float2 Tray;
    float p;
};

Reflection breflect(float c, float cz, float2 Tray, float p, float q, uint istep, uint offset)
{
    
    Reflection result;
    float kappa = 0;
    uint curveFact = 1;
    float2 TBdry = { 1.0, 0.0 };
    float2 NBdry = { 0.0, 1.0 };
    
    // Reflect a ray / beam off a boundry

    float Tg = dot((float2) Tray, TBdry);
    float Th = dot((float2) Tray, NBdry);    
     
    // Calcualte the change in curvature. 
    // Based on formulas given by Muller, Geoph. J. R.A.S., 79 (1984)
    
    // Incident unit ray tangent and normal
    float cnjump = 2 * cz * Tray.x;
    float csjump = 2 * cz * Tray.y;

    //xrayBuf[istep + offset] = float3(c, Tg, 14);
    
    // Boundry curvature correction
    float RN = 2 * kappa / (c * c) / Th;
    
    float RM = Tg / Th;
    RN = RN + RM * (2 * cnjump - RM * csjump) / c;
    
    result.Tray = Tray - 2 * Th * NBdry;
    result.p = p + curveFact * RN * q; 
    
    return result;   
}