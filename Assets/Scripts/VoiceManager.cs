using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VoicePriority
{
    Normal = 0,
    Death = 1
}

public class VoiceManager : MonoBehaviour
{
    public static VoiceManager Instance { get; private set; }

    [SerializeField] private AudioSource voiceSource;

    private Queue<(AudioClip, VoicePriority)> voiceQueue = new();
    private bool isPlaying = false;
    private VoicePriority currentPriority = VoicePriority.Normal;

    private List<AudioSource> pausedSources = new();

    private bool isVoiceLocked = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
        }
    }

    public void PlayVoice(AudioClip clip, VoicePriority priority = VoicePriority.Normal)
    {
        if (clip == null || isVoiceLocked) return; //

        if (isPlaying && priority > currentPriority)
        {
            StopCurrentVoice();
        }

        voiceQueue.Enqueue((clip, priority));

        if (!isPlaying)
            StartCoroutine(PlayVoiceQueue());
    }

    private IEnumerator PlayVoiceQueue()
    {
        isPlaying = true;

        while (voiceQueue.Count > 0)
        {
            (AudioClip clip, VoicePriority priority) = voiceQueue.Dequeue();
            currentPriority = priority;

            PauseOtherAudio();

            voiceSource.clip = clip;
            voiceSource.Play();

            yield return new WaitForSeconds(clip.length);

            ResumeOtherAudio();

            // ✅ 锁死语音系统（只锁一次）
            if (priority == VoicePriority.Death)
            {
                isVoiceLocked = true;
                break; // 退出队列处理
            }

            currentPriority = VoicePriority.Normal;
        }

        isPlaying = false;
    }


    private void StopCurrentVoice()
    {
        if (voiceSource.isPlaying)
            voiceSource.Stop();

        currentPriority = VoicePriority.Normal;
        StopAllCoroutines();
        isPlaying = false;
    }

    private void PauseOtherAudio()
    {
        pausedSources.Clear();
        foreach (AudioSource src in FindObjectsOfType<AudioSource>())
        {
            if (src != voiceSource && src.isPlaying)
            {
                src.Pause();
                pausedSources.Add(src);
            }
        }
    }

    private void ResumeOtherAudio()
    {
        foreach (AudioSource src in pausedSources)
        {
            if (src != null) src.UnPause();
        }
        pausedSources.Clear();
    }

    public void ResetVoiceLock()
    {
        isVoiceLocked = false;
        StopCurrentVoice();
    }
}

