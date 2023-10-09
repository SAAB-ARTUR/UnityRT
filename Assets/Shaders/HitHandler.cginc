void SurfaceHit(inout Ray ray, RayHit hit, uint index1D)
{
    // update the ray data
    ray.origin = hit.position + float3(0, -0.005f, 0); // move origin slightly so we don't accidentaly rehit the same plane
    float3 directionTemp = ray.direction;
    float3 newDirection = float3(directionTemp.x, directionTemp.y * -1, directionTemp.z);
    ray.direction = newDirection;
    ray.nrOfInteractions++;
}


void SeafloorHit(inout Ray ray, RayHit hit, uint index1D)
{

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
}


void NoHit(Ray ray, uint index1D)
{
    // kod för miss ...

}

void TargetHit(inout Ray ray, RayHit hit, uint index1D)
{
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