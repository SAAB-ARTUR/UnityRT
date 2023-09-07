using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class RayTracingObject : MonoBehaviour
{
    public MeshObjectType meshObjectType;

    private void OnEnable()
    {
        Main.RegisterObject(this);
    }

    private void OnDisable()
    {
        Main.UnregisterObject(this);
    }
}