using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string gameScene;
    public string tutorialGameScene;
    public GameObject MainPanel;
    public GameObject StartPanel;
    public GameObject SettingPanel;

    public void startGame()
    {
        switchPanel("startGame");
    }

    public void settingPanel()
    {
        switchPanel("setting");
    }

    public void skipTutorialGame()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void tutorialGame()
    {
        SceneManager.LoadScene(tutorialGameScene);
    }

    public void backToMain(){
        switchPanel("main");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void switchPanel(string state)
    {
        MainPanel.SetActive(false);
        StartPanel.SetActive(false);
        SettingPanel.SetActive(false);

        if(state == "startGame") 
            StartPanel.SetActive(true);
        if(state == "setting") 
            SettingPanel.SetActive(true);
        if(state == "main") 
            MainPanel.SetActive(true);
    }
}

