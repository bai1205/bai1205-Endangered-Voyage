using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class LeftHandButtonTrigger : MonoBehaviour
{
    public SwitchCamera switchCameraScript;

    private InputDevice leftHand;
    private bool lastButtonState = false;

    void Update()
    {
        TryInitializeDevice();

        if (!leftHand.isValid || switchCameraScript == null)
            return;

        bool currentState = false;
        leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out currentState);

        // 检测按下瞬间触发一次
        if (currentState && !lastButtonState)
        {
            switchCameraScript.ChangeCamera();
            Debug.Log("Left X button pressed - triggered ChangeCamera()");
        }

        lastButtonState = currentState;
    }

    void TryInitializeDevice()
    {
        if (!leftHand.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
            if (devices.Count > 0)
                leftHand = devices[0];
        }
    }
}
