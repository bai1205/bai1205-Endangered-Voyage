using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HandMenuController : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject SettingPanel;
    public GameObject ControllerPanel;
    public GameObject ExitPanel;

    public void goToMainPanel()
    {
        SettingPanel.SetActive(false);
        MainPanel.SetActive(true);
    }

    public void goToSettingPanel()
    {
        SettingPanel.SetActive(true);
        MainPanel.SetActive(false);
    }

    public void goToControllerPanel()
    {
        ControllerPanel.SetActive(true);
        ExitPanel.SetActive(false);
    }

    public void goToExitPanel()
    {
        ExitPanel.SetActive(true);
        ControllerPanel.SetActive(false);
    }

    public void backToMainMenu()
    {
        SceneManager.LoadScene("MainScenes");
    }

    public void quit()
    {
        Application.Quit();
    }
}
