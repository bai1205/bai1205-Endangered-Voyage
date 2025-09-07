using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScenesPanelController : MonoBehaviour
{
    public GameObject panel;     
    public TMP_Text messageText; 
    // Start is called before the first frame update
    void Start()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (panel != null)
            panel.SetActive(true);

        if (messageText != null)
            messageText.text = message;
    }

    // Update is called once per frame
    public void HideMessage()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}
