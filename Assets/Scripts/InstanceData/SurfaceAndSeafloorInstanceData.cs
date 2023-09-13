using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System;


public class SurfaceAndSeafloorInstanceData : IDisposable
{
    public NativeArray<Matrix4x4> matrices;

    public SurfaceAndSeafloorInstanceData()
    {
        matrices = new NativeArray<Matrix4x4>(1, Allocator.Persistent);

        int index = 0;

        matrices[index] = float4x4.Translate(new Vector3(0, 0, 0));
    }

    public void Dispose()
    {
        if (matrices.IsCreated)
        {
            matrices.Dispose();
        }
    }
}