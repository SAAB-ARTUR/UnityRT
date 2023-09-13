using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System;

sealed class WaterplaneInstanceData : IDisposable
{
    public NativeArray<Matrix4x4> matrices;
    public int layers;
    public float depth;

    public WaterplaneInstanceData(int _layers, float _depth)
    {
        layers = _layers;
        depth = _depth;

        matrices = new NativeArray<Matrix4x4>(layers, Allocator.Persistent);

        int index = 0;

        NativeArray<Vector3> data = new NativeArray<Vector3>(layers, Allocator.Temp);

        float delta = (float)depth / (float)(layers + 1);
        for (int layer = 0; layer < layers; layer++)
        {
            float y = layer * delta;

            matrices[index] = float4x4.Translate(new Vector3(0, -y, 0));

            index++;
        }
    }

    public void Dispose()
    {
        if (matrices.IsCreated)
        {
            matrices.Dispose();
        }
    }
}