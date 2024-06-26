using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    public GameObject panel = null;
    private bool panelEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(panelEnabled);        
    }

    public void OnSettingsButtonClick()
    {
        panelEnabled = !panelEnabled;
        panel.SetActive(panelEnabled);
    }
}
