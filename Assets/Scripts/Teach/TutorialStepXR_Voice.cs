using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class TutorialStepXR_Voice
{
    /*    public AudioClip voiceClip;                         // 当前步骤播放的语音
        public InputActionProperty requiredAction;          // 等待的输入行为（如 Trigger、A键等）
        public bool requirePerformed = true;                // 是否要求 action 被触发（默认 true）
        public System.Action optionalSetup;                 // 可选：步骤前的设置方法（如切换状态）*/
    public AudioClip voiceClip;

    [Tooltip("触发该步骤的一个或多个输入（可为单键或多键）")]
    public InputActionProperty[] requiredActions;

    [Tooltip("是否所有 requiredActions 都需要在同一帧触发")]
    public bool requireAll = false;

    public System.Action optionalSetup;
}
