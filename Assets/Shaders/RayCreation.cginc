Ray CreateRay(float3 origin, float3 direction, int nrOfInteractions)
{
    Ray ray;
    ray.origin = origin;
    ray.direction = direction;
    ray.energy = float3(1.0f, 1.0f, 1.0f);
    ray.nrOfInteractions = nrOfInteractions;    
    return ray;
}

Ray CreateHovemRTRay(float3 origin, uint3 id)
{
    float srcY;
    if (abs(srcDirection.y) < 1e-6) {
        srcY = 0;
    }
    else {
        srcY = srcDirection.y;
    }    

    // calculate angular direction of ray given the id of the thread
    float theta_rad = theta * PI / 180;
    float dtheta = theta_rad / (ntheta + 1); // resolution in theta
    float theta = theta_rad / 2 - (id.y + 1) * dtheta;
    float origin_theta = -asin(srcY);

    // add source's view angle to the ray
    theta += origin_theta;

    float phi = targetBuffer[id.x].phi; // angle towards target id.x, should be in radians   

    float x = phi * cos(phi);
    float z = phi * sin(phi);
    float y = cos(theta);

    float3 direction = float3(x, y, z);
    
    return CreateRay(origin, direction, 0);
}

float2 CreateCylindricalRay(uint3 id)
{
    float srcY;    
    if (abs(srcDirection.y) < 1e-6) {
        srcY = 0;
    }
    else {
        srcY = srcDirection.y;
    }

    // a set of rays are created based on the unit ray [1 0 0] (x-axis) and are then rotated according to the source's view direction
    float theta_rad = theta * PI / 180; //convert to radians    
    float dtheta = theta_rad / (ntheta + 1); // resolution in theta
    float theta = theta_rad / 2 - (id.y + 1) * dtheta;
    
    float origin_theta = -asin(srcY);

    // add source's view angle to the ray
    theta += origin_theta;    

    float phi = targetBuffer[id.x].phi; // angle towards target id.x

    return float2(theta, phi);
}

Ray CreateCameraRay(float2 uv)
{
    // Transform the camera origin to world space
    float3 origin = mul(_SourceCameraToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;

    // Invert the perspective projection of the view-space position
    float3 direction = mul(_CameraInverseProjection, float4(uv, 0.0f, 1.0f)).xyz;
    // Transform the direction from camera to world space and normalize
    direction = mul(_SourceCameraToWorld, float4(direction, 0.0f)).xyz;
    direction = normalize(direction);
    int nrOfInteractions = 0;

    return CreateRay(origin, direction, nrOfInteractions);
}