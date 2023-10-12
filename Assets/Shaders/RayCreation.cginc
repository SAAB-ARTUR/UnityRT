Ray CreateRay(float3 origin, float3 direction, int nrOfInteractions, float phi)
{
    Ray ray;
    ray.origin = origin;
    ray.direction = direction;
    ray.energy = float3(1.0f, 1.0f, 1.0f);
    ray.nrOfInteractions = nrOfInteractions;
    ray.phi = phi;
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

    return CreateRay(origin, direction, nrOfInteractions, 0);
}

float2 CreateRay2(uint3 id) {
    float srcX, srcY, srcZ;
    if (abs(srcDirection.x) < 1e-6) {
        srcX = 0;
    }
    else {
        srcX = srcDirection.x;
    }
    if (abs(srcDirection.y) < 1e-6) {
        srcY = 0;
    }
    else {
        srcY = srcDirection.y;
    }
    if (abs(srcDirection.z) < 1e-6) {
        srcZ = 0;
    }
    else {
        srcZ = srcDirection.z;
    }

    // angles for srcSphere's forward vector (which is of length 1 meaning that r can be removed from all equations below)
    float origin_theta = -asin(srcY);
    float origin_phi = atan2(srcZ, srcX);

    if (srcZ == 0 && srcX == 0) {
        origin_phi = 0;
    }

    float theta_rad = theta * PI / 180; //convert to radians
    float phi_rad = phi * PI / 180;

    float dtheta = theta_rad / (ntheta + 1); // resolution in theta
    float dphi = phi_rad / (nphi + 1); // resolution in phi

    float offset_theta = origin_theta + theta_rad / 2 - (id.y + 1) * dtheta;
    float offset_phi = origin_phi + phi_rad / 2 - (id.x + 1) * dphi;

    debugBuf[id.y * nphi + id.x] = float3(srcZ, srcX, atan2(srcZ, srcX));

    return float2(offset_theta, offset_phi);
}

Ray CreateThetaPhiRay(uint3 id) // absolut sämstaste namnet någonsin
{   
    float srcX, srcY, srcZ;
    if (abs(srcDirection.x) < 1e-6) {
        srcX = 0;
    }
    else {
        srcX = srcDirection.x;
    }
    if (abs(srcDirection.y) < 1e-6) {
        srcY = 0;
    }
    else {
        srcY = srcDirection.y;
    }
    if (abs(srcDirection.z) < 1e-6) {
        srcZ = 0;
    }
    else {
        srcZ = srcDirection.z;
    }

    float3 origin = mul(_SourceCameraToWorld, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz; // source sphere's position

    float theta_rad = theta * PI / 180; //convert to radians
    float phi_rad = phi * PI / 180;

    float dtheta = theta_rad / (ntheta + 1); // resolution in theta
    float dphi = phi_rad / (nphi + 1); // resolution in phi

    // angles for srcSphere's forward vector (which is of length 1 meaning that r can be removed from all equations below)
    float origin_theta = acos(srcY);
    float origin_phi = atan2(srcZ, srcX);

    // calculate the angular offset for the ray compared to the forward vector of the source
    //float offset_theta = origin_theta - theta_rad / 2 + id.y * dtheta;
    float offset_theta = origin_theta + theta_rad / 2 - (id.y + 1) * dtheta;
    //float offset_phi = origin_phi + phi_rad / 2 - id.x * dphi;
    //float offset_phi = 0;
    float offset_phi = origin_phi - phi_rad / 2 + (id.x + 1) * dphi;    

    float s0 = sin(origin_phi); //1 //0
    float c0 = cos(origin_phi); //0 //1

    float s1 = sin(offset_phi - origin_phi); //-1 //0
    float c1 = cos(offset_phi - origin_phi); //0 // 1

    float x = c0 * c1 * sin(offset_theta) - s0 * s1; // 1 // 1
    float z = s0 * c1 * sin(offset_theta) + c0 * s1; // 0 //0
    /*float z = sin(origin_phi);
    float x = cos(origin_phi);*/
    float y = c1 * cos(offset_theta);
    //float y = cos(-origin_phi) * cos(offset_theta);

    // normalize x so that the vector is of unit length
    //x = x / sqrt(x * x + y * y);

    // rotate ray around the y-axis to get where the source is facing
    //x = x * cos(origin_phi) - z * sin(origin_phi);
    //z = z * cos(origin_phi) + x * sin(origin_phi);

    // rotate ray around the y-axis offset_phi radians
    /*offset_phi = phi_rad / 2 - (id.x + 1) * dphi;
    x = x * cos(offset_phi) - z * sin(offset_phi);
    z = z * cos(offset_phi) + x * sin(offset_phi);*/

    float3 direction = float3(x, y, z);

    //debugBuf[id.y * nphi + id.x] = float3(origin_phi, z, 12);

    //debugBuf[id.y * nphi + id.x] = direction;

    int nrOfInteractions = 0;

    float rayPhi = -phi_rad / 2 + (id.x + 1) * dphi;

    return CreateRay(origin, direction, nrOfInteractions, rayPhi);
}