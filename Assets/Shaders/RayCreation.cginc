Ray CreateRay(float3 origin, float3 direction, int nrOfInteractions)
{
    Ray ray;
    ray.origin = origin;
    ray.direction = direction;
    ray.energy = float3(1.0f, 1.0f, 1.0f);
    ray.nrOfInteractions = nrOfInteractions;
    return ray;
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

Ray CreateThetaPhiRay(uint3 id) // absolut sämstaste namnet någonsin
{   
    float3 origin = mul(_SourceCameraToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz; // source sphere's position

    float theta_rad = theta * PI / 180; //convert to radians
    float phi_rad = phi * PI / 180;

    float dtheta = theta_rad / (ntheta-1); // resolution in theta
    float dphi = phi_rad / (nphi-1); // resolution in phi

    // angles for srcSphere's forward vector (which is of length 1 meaning that r can be removed from all equations below)
    float origin_theta = acos(srcDirection.y);
    float origin_phi = atan2(srcDirection.z, srcDirection.x);

    // calculate the angular offset for the ray compared to the forward vector of the source
    float offset_theta = origin_theta - theta_rad / 2 + id.y * dtheta;
    //float offset_phi = origin_phi + phi_rad / 2 - id.x * dphi;
    float offset_phi = 0;

    float s0 = sin(origin_phi);
    float c0 = cos(origin_phi);

    float s1 = sin(offset_phi - origin_phi);
    float c1 = cos(offset_phi - origin_phi);

    float x = c0 * c1 * sin(offset_theta) - s0 * s1;
    float z = s0 * c1 * sin(offset_theta) + c0 * s1;
    float y = c1 * cos(offset_theta);

    float3 direction = float3(x, y, z);

    int nrOfInteractions = 0;

    return CreateRay(origin, direction, nrOfInteractions);
}