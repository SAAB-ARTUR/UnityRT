using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingVisualization : MonoBehaviour 
{    
    RenderTexture rayTracingOutput = null;    

    public void receiveData(RenderTexture data)
    {
        rayTracingOutput = data;
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(rayTracingOutput, destination);
    }
}
