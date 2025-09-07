using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayController : MonoBehaviour
{
    [Header("视频播放组件")]
    public VideoPlayer videoPlayer;      // 拖拽 VideoPlayer 进来
    public RawImage videoDisplay;        // 显示视频的 RawImage（可选）

    [Header("按钮控制")]
    public Button playButton;            // 播放按钮
    public Button replayButton;          // 重播按钮（视频结束后显示）

    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer 未设置！");
            return;
        }

        // 关闭自动播放
        videoPlayer.playOnAwake = false;

        // 注册事件
        if (playButton != null) playButton.onClick.AddListener(PlayVideo);
        if (replayButton != null) replayButton.onClick.AddListener(RestartVideo);

        // 初始化按钮状态
        if (replayButton != null) replayButton.gameObject.SetActive(false);

        // 监听视频结束
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
            replayButton.gameObject.SetActive(true); // 视频结束时显示重播按钮
    }
}
