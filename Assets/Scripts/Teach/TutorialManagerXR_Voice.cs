using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class TutorialManagerXR_Voice : MonoBehaviour
{
    public AudioSource audioSource;                                  // ���ڲ�������
    public TutorialStepXR_Voice[] steps;                       // ���н�ѧ����

    private int currentStep = 0;
    private bool isWaitingForInput = false;

    public string nextSceneName = "Main Project";

    void Start()
    {
        PlayCurrentStep();
    }

    void Update()
    {
        if (!isWaitingForInput) return;

        if (currentStep >= steps.Length) return;

        var step = steps[currentStep];
        if (step.requiredActions == null || step.requiredActions.Length == 0) return;

        bool allPressed = true;
        bool anyPressed = false;

        foreach (var action in step.requiredActions)
        {
            if (action.action == null) continue;

            if (action.action.WasPerformedThisFrame())
                anyPressed = true;
            else
                allPressed = false;
        }

        if ((step.requireAll && allPressed) || (!step.requireAll && anyPressed))
        {
            NextStep();
        }
    }


    void PlayCurrentStep()
    {
        if (currentStep >= steps.Length) return;

        var step = steps[currentStep];

        if (step.voiceClip != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = step.voiceClip;
            audioSource.Play();
        }

        step.optionalSetup?.Invoke();

        isWaitingForInput = false;

        float waitTime = step.voiceClip != null ? step.voiceClip.length : 0.1f;
        Invoke(nameof(EnableInput), waitTime);
    }


    void EnableInput()
    {
        isWaitingForInput = true;
    }

    void NextStep()
    {
        currentStep++;

        if (currentStep < steps.Length)
        {
            Invoke(nameof(PlayCurrentStep), 3f);  // �ӳٲ�����һ��������
        }
        else
        {
            Debug.Log("��ѧ������׼����ת����ʽ����...");

            // �ɼ�һ�㻺�����г���
            Invoke(nameof(LoadNextScene), 2f);
        }
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("δ������ת�������ƣ�");
        }
    }


}

