void SurfaceHit(inout Ray ray, RayHit hit, uint index1D)
{
    // save hit data to buffer
    _RayPoints[index1D + ray.nrOfInteractions].origin = hit.position;
    _RayPoints[index1D + ray.nrOfInteractions].set = 12345;

    // update the ray data
    ray.origin = hit.position + float3(0, -0.005f, 0); // move origin slightly so we don't accidentaly rehit the same plane
    float3 directionTemp = ray.direction;
    float3 newDirection = float3(directionTemp.x, directionTemp.y * -1, directionTemp.z);
    ray.direction = newDirection;
    ray.nrOfInteractions++;
}


void SeafloorHit(inout Ray ray, RayHit hit, uint index1D)
{
    // save hit data to buffer
    _RayPoints[index1D + ray.nrOfInteractions].origin = hit.position;
    _RayPoints[index1D + ray.nrOfInteractions].set = 12345;

    // update the ray data
    ray.origin = hit.position + float3(0, 0.005f, 0); // move origin slightly so we don't accidentaly rehit the same plane
    float3 directionTemp = ray.direction;
    float3 newDirection = float3(directionTemp.x, directionTemp.y * -1, directionTemp.z);
    ray.direction = newDirection;
    ray.nrOfInteractions++;
}


void WaterplaneHit(inout Ray ray, RayHit hit, uint index1D)
{
    // kod för när ett vattenplan träffats här... (förhoppningsvis bör vi kunna skriva en funktion som ser likadan ut oavsett vilket vattenplan som träffats. 
    // vissa variabelvärden ändras beroende på djup och sånt men ekvationerna bör se likadana ut.)

    // do nothing for now
    _RayPoints[index1D + ray.nrOfInteractions].set = 400;
    _RayPoints[index1D + ray.nrOfInteractions].origin = float3(0, 1000, 0);
}


void NoHit(Ray ray, uint index1D)
{
    // kod för miss ...

    _RayPoints[index1D + ray.nrOfInteractions].origin = float3(0, 1000, 0); // impossible hit since all hits should have y-coordinate <= 0
    _RayPoints[index1D + ray.nrOfInteractions].set = 400; // no hit found

}

void TargetHit(inout Ray ray, RayHit hit, uint index1D)
{
    _RayPoints[index1D + ray.nrOfInteractions].origin = hit.position;
    _RayPoints[index1D + ray.nrOfInteractions].set = 12345;
    ray.nrOfInteractions++;
}


bool SendNewRay(Ray ray)
{
    if (ray.nrOfInteractions < _MAXINTERACTIONS)
    {
        return true;
    }
    return false;
}