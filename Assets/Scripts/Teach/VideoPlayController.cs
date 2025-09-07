using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayController : MonoBehaviour
{
    [Header("��Ƶ�������")]
    public VideoPlayer videoPlayer;      // ��ק VideoPlayer ����
    public RawImage videoDisplay;        // ��ʾ��Ƶ�� RawImage����ѡ��

    [Header("��ť����")]
    public Button playButton;            // ���Ű�ť
    public Button replayButton;          // �ز���ť����Ƶ��������ʾ��

    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer δ���ã�");
            return;
        }

        // �ر��Զ�����
        videoPlayer.playOnAwake = false;

        // ע���¼�
        if (playButton != null) playButton.onClick.AddListener(PlayVideo);
        if (replayButton != null) replayButton.onClick.AddListener(RestartVideo);

        // ��ʼ����ť״̬
        if (replayButton != null) replayButton.gameObject.SetActive(false);

        // ������Ƶ����
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void PlayVideo()
    {
        if (playButton != null) playButton.gameObject.SetActive(false);
        if (replayButton != null) replayButton.gameObject.SetActive(false);

        videoPlayer.Stop();
        videoPlayer.Play();
    }

    void RestartVideo()
    {
        if (replayButton != null) replayButton.gameObject.SetActive(false);

        videoPlayer.Stop();
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (replayButton != null)
            replayButton.gameObject.SetActive(true); // ��Ƶ����ʱ��ʾ�ز���ť
    }
}
