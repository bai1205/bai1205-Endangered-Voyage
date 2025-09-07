using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Unity.XR.CoreUtils;

public class ChangeView : MonoBehaviour
{
    [Header("要切换的脚本")]
    public MonoBehaviour scriptA; // 普通视角脚本
    public MonoBehaviour scriptB; // GodView 脚本

    [Header("XR / GodView")]
    public XROrigin xrOrigin;     // XR Origin 根物体
    public Transform godAnchor;   // GodAnchor，位置由 AnchorFollowTarget 控制
    public bool smoothOnEnter = true;
    public float moveLerp = 12f;
    public float rotLerp = 12f;

    private bool isAActive = true;
    private InputDevice leftHand;
    private bool lastXButtonState = false;

    private bool isBlending = false;
    private Vector3 targetPos;
    private Quaternion targetRot;

    void Start()
    {
        if (!xrOrigin) xrOrigin = FindObjectOfType<XROrigin>();
        EnableScriptA();
        TryInitializeLeftHand();
    }

    void Update()
    {
        if (!leftHand.isValid) TryInitializeLeftHand();

        bool xButtonPressed;
        if (leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out xButtonPressed))
        {
            if (xButtonPressed && !lastXButtonState) ToggleScripts();
            lastXButtonState = xButtonPressed;
        }

        if (isBlending && xrOrigin)
        {
            SmoothMove(xrOrigin.transform, targetPos, targetRot);
        }
    }

    // 不再回写 godAnchor（已移除 LateUpdate 中的回写逻辑）

    void TryInitializeLeftHand()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
        if (devices.Count > 0)
        {
            leftHand = devices[0];
            Debug.Log("Left hand controller initialized for X button input.");
        }
    }

    void ToggleScripts()
    {
        if (isAActive) EnableScriptB();
        else EnableScriptA();
    }

    void EnableScriptA()
    {
        if (scriptA) scriptA.enabled = true;
        if (scriptB) scriptB.enabled = false;
        isAActive = true;
        isBlending = false;
        Debug.Log("Switched to ScriptA (Normal View)");
    }

    void EnableScriptB()
    {
        if (scriptA) scriptA.enabled = false;
        if (scriptB) scriptB.enabled = true;
        isAActive = false;

        if (xrOrigin && godAnchor)
        {
            if (smoothOnEnter)
            {
                targetPos = godAnchor.position;
                targetRot = godAnchor.rotation;
                isBlending = true;
            }
            else
            {
                xrOrigin.transform.SetPositionAndRotation(godAnchor.position, godAnchor.rotation);
                isBlending = false;
            }
        }

        Debug.Log("Switched to ScriptB (God View)");
    }

    void SmoothMove(Transform t, Vector3 pos, Quaternion rot)
    {
        t.position = Vector3.Lerp(t.position, pos, 1f - Mathf.Exp(-moveLerp * Time.deltaTime));
        t.rotation = Quaternion.Slerp(t.rotation, rot, 1f - Mathf.Exp(-rotLerp * Time.deltaTime));

        if (Vector3.Distance(t.position, pos) < 0.01f &&
            Quaternion.Angle(t.rotation, rot) < 0.5f)
        {
            t.SetPositionAndRotation(pos, rot);
            isBlending = false;
        }
    }
}
