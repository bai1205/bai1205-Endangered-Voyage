using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class StepVideoTutorialUI : MonoBehaviour
{
    [System.Serializable]
    public class VideoStep
    {
        public string title;                 // 可选：步骤标题
        public VideoClip videoClip;
    }

    public VideoStep[] steps;

    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    public Button leftArrowButton; 
    public Button rightArrowButton;            // 显示“Next”或“Finish”
    public Button finishButton;
   // public Button replayButton;  // 视频播放完后出现的重播按钮
    //public GameObject pausePanel;
    // public TMP_Text rightArrowText;
    public string nextSceneName = "MainGameScene";

    private int currentStep = 0;

    void Start()
    {
        SetupButtons();
        //videoPlayer.loopPointReached += OnVideoFinished;
        PlayStep(currentStep);
    }
    void OnVideoFinished(VideoPlayer vp)
    {
        //replayButton.gameObject.SetActive(true);
        //pausePanel.SetActive(true);
    }

    void SetupButtons()
    {
        leftArrowButton.onClick.AddListener(GoToPreviousStep);
        rightArrowButton.onClick.AddListener(GoToNextStep);
        finishButton.onClick.AddListener(FinishTutorial);
        //replayButton.onClick.AddListener(RestartVideo);
    }

    void PlayStep(int index)
    {
        if (index < 0 || index >= steps.Length) return;

        // 播放视频
        videoPlayer.clip = steps[index].videoClip;
        videoPlayer.Play();

        // 更新按钮状态
        leftArrowButton.interactable = index > 0;

        bool isLast = (index == steps.Length - 1);

        rightArrowButton.gameObject.SetActive(!isLast);
        finishButton.gameObject.SetActive(isLast);
       // replayButton.gameObject.SetActive(false);
       // pausePanel.SetActive(false);
    }

    void GoToPreviousStep()
    {
        if (currentStep > 0)
        {
            currentStep--;
            PlayStep(currentStep);
        }
    }

    void GoToNextStep()
    {
        if (currentStep < steps.Length - 1)
        {
            currentStep++;
            PlayStep(currentStep);
        }
        else
        {
            // 最后一步点击 Finish：跳转场景
            SceneManager.LoadScene(nextSceneName);
        }
    }
    void FinishTutorial()
    {
        SceneManager.LoadScene(nextSceneName);  // 跳转场景
    }
    void RestartVideo()
    {
        //replayButton.gameObject.SetActive(false);  // 隐藏重播按钮
        //pausePanel.SetActive(false);
        videoPlayer.Stop();
        videoPlayer.Play();
    }
}
