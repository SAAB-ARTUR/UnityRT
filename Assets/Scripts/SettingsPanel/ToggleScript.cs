using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleScript : MonoBehaviour
{
    public GameObject srcSphere = null;
    private bool visualizeRays = false;
    private bool sendRaysContinously = false;

    // Start is called before the first frame update
    void Start()
    {
        visualizeRays = false;
        sendRaysContinously = false;
    }

    public void OnToggleVisualizeRays()
    {
        visualizeRays = !visualizeRays;
        srcSphere.GetComponent<SourceParams>().visualizeRays = visualizeRays;
    }

    public void OnToggleSendRaysContinously()
    {
        sendRaysContinously = !sendRaysContinously;
        srcSphere.GetComponent<SourceParams>().sendRaysContinously = sendRaysContinously;
    }
}
