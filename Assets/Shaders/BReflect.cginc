struct Reflection
{
    double2 Tray;
    double p;
};

Reflection breflect(double c, double cz, double2 Tray, double p, double q)
{
    
    Reflection result;
    double kappa = 0;
    uint curveFact = 1;
    float2 TBdry = { 1.0, 0.0 };
    float2 NBdry = { 0.0, 1.0 };
    
    // Reflect a ray / beam off a boundry

    double Tg = dot((float2) Tray, TBdry);
    double Th = dot((float2) Tray, NBdry);
    
     
    // Calcualte the change in curvature. 
    // Based on formulas given by Muller, Geoph. J. R.A.S., 79 (1984)
    
    // Incident unit ray tangent and normal
    double cnjump = 2 * cz * Tray.r;
    double csjump = 2 * cz * Tray.g;
    
    // Boundry curvature correction
    double RN = 2 * kappa / (c * c) / Th;
    
    double RM = Tg / Th;
    RN = RN + RM * (2 * cnjump - RM * csjump) / c;
    
    result.Tray = Tray - 2 * Th * NBdry;
    result.p = p + curveFact * RN * q; 
    
    return result;   
}