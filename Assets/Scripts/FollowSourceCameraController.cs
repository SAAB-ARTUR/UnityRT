using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityTemplateProjects;

public class FollowSourceCameraController : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    Camera SourceCamera;

    SimpleCameraController controller;
    bool follow_state = true;

    void Start()
    {
        controller = GetComponent<SimpleCameraController>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyUp(KeyCode.R)) {
            follow_state = !follow_state;
        }

        if (follow_state)
        {
            controller.enabled = false;
            this.transform.parent = SourceCamera.transform;
            this.transform.localPosition = Vector3.back * 10;
            this.transform.eulerAngles = SourceCamera.transform.eulerAngles;            
        }
        else {
            this.transform.parent = null;
            controller.enabled = true;
        }

    }
}
