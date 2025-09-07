
using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class GodViewControl : MonoBehaviour
{
    public Transform targetToMove;
    public float verticalSpeed = 2f;
    public float minY = 5f;
    public float maxY = 50f;

    private InputDevice leftHand;

    void Update()
    {
        if (!leftHand.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
            if (devices.Count > 0)
            {
                leftHand = devices[0];
                Debug.Log("Left hand controller connected");
            }
        }

        if (leftHand.isValid && targetToMove != null)
        {
            bool moveUp = false;
            bool moveDown = false;

            leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out moveUp);
            leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out moveDown);

            Vector3 pos = targetToMove.position;

            if (moveUp)
            {
                pos.y += verticalSpeed * Time.deltaTime;
                Debug.Log("Primary button pressed: moving up");
            }

            if (moveDown)
            {
                pos.y -= verticalSpeed * Time.deltaTime;
                Debug.Log("Secondary button pressed: moving down");
            }

           // pos.y = Mathf.Clamp(pos.y, minY, maxY);
            targetToMove.position = pos;
        }
    }

}
