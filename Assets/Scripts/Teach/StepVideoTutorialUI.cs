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
        public string title;                 // ��ѡ���������
        public VideoClip videoClip;
    }

    public VideoStep[] steps;

    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    public Button leftArrowButton; 
    public Button rightArrowButton;            // ��ʾ��Next����Finish��
    public Button finishButton;
   // public Button replayButton;  // ��Ƶ���������ֵ��ز���ť
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

        // ������Ƶ
        videoPlayer.clip = steps[index].videoClip;
        videoPlayer.Play();

        // ���°�ť״̬
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
            // ���һ����� Finish����ת����
            SceneManager.LoadScene(nextSceneName);
        }
    }
    void FinishTutorial()
    {
        SceneManager.LoadScene(nextSceneName);  // ��ת����
    }
    void RestartVideo()
    {
        //replayButton.gameObject.SetActive(false);  // �����ز���ť
        //pausePanel.SetActive(false);
        videoPlayer.Stop();
        videoPlayer.Play();
    }
}
