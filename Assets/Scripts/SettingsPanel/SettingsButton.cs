using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    public GameObject panel = null;

    private bool panelEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(panelEnabled);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSettingsButtonClick()
    {
        panelEnabled = !panelEnabled;
        panel.SetActive(panelEnabled);
    }
}
