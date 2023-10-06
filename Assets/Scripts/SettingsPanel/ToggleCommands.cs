using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleCommands : MonoBehaviour
{

    public Camera mainCamera = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEnableCommands()
    {
        mainCamera.GetComponent<apiv2>().enabled = GetComponent<Toggle>().isOn;
    }
}
