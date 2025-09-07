using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class TutorialStepXR_Voice
{
    /*    public AudioClip voiceClip;                         // ��ǰ���貥�ŵ�����
        public InputActionProperty requiredAction;          // �ȴ���������Ϊ���� Trigger��A���ȣ�
        public bool requirePerformed = true;                // �Ƿ�Ҫ�� action ��������Ĭ�� true��
        public System.Action optionalSetup;                 // ��ѡ������ǰ�����÷��������л�״̬��*/
    public AudioClip voiceClip;

    [Tooltip("�����ò����һ���������루��Ϊ����������")]
    public InputActionProperty[] requiredActions;

    [Tooltip("�Ƿ����� requiredActions ����Ҫ��ͬһ֡����")]
    public bool requireAll = false;

    public System.Action optionalSetup;
}
