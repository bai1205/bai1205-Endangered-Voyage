using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonController : MonoBehaviour
{
    public void RestartGame()
    {
        Time.timeScale = 1f; // �Ȼָ�ʱ��
        SceneManager.LoadScene("Main Project");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
