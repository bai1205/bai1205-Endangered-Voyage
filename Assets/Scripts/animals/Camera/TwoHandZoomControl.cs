using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class TwoHandZoomAndSingleHandRotate : MonoBehaviour
{
    [Header("God View Setting")]
    public Transform xrOrigin;
    public float zoomSpeed = 5f;
    public float rotateSpeed = 50f;
    public float minDistance = 1f;
    public float maxDistance = 50f;

    private InputDevice leftHand;
    private InputDevice rightHand;

    private bool zooming = false;
    private float initialDistance;
    private Vector3 initialOriginPosition;

    private bool rotating = false;
    private Vector3 initialHandPosition;
    private XRNode rotatingHand;


    void Update()
    {
        TryInitializeDevices();

        GodView();
    }

    #region God View
    void TryInitializeDevices()
    {
        if (!leftHand.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, devices);
            if (devices.Count > 0)
            {
                leftHand = devices[0];
                Debug.Log("Left hand controller initialized");
            }
        }

        if (!rightHand.isValid)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);
            if (devices.Count > 0)
            {
                rightHand = devices[0];
                Debug.Log("Right hand controller initialized");
            }
        }
    }
    void GodView()
    {
        if (!leftHand.isValid || !rightHand.isValid || xrOrigin == null)
            return;

        bool leftGrab, rightGrab;
        leftHand.TryGetFeatureValue(CommonUsages.gripButton, out leftGrab);
        rightHand.TryGetFeatureValue(CommonUsages.gripButton, out rightGrab);

        Vector3 leftPos, rightPos;
        leftHand.TryGetFeatureValue(CommonUsages.devicePosition, out leftPos);
        rightHand.TryGetFeatureValue(CommonUsages.devicePosition, out rightPos);

        if (leftGrab && rightGrab)
        {
            // Zoom Mode
            float currentDistance = Vector3.Distance(leftPos, rightPos);

            if (!zooming)
            {
                zooming = true;
                rotating = false;
                initialDistance = currentDistance;
                initialOriginPosition = xrOrigin.position;
                Debug.Log("Zoom start");
            }
            else
            {
                float delta = currentDistance - initialDistance;
                Vector3 viewDirection = Camera.main.transform.forward;
                Vector3 moveOffset = viewDirection.normalized * delta * zoomSpeed;

                Vector3 targetPosition = initialOriginPosition + moveOffset;
                float moveDistance = Vector3.Distance(initialOriginPosition, targetPosition);

                if (moveDistance >= minDistance && moveDistance <= maxDistance)
                {
                    xrOrigin.position = targetPosition;
                    Debug.Log("Zooming along view direction");
                }
            }
        }
        else if (leftGrab ^ rightGrab)
        {
            // Rotation Mode
            XRNode currentNode = leftGrab ? XRNode.LeftHand : XRNode.RightHand;
            InputDevice hand = currentNode == XRNode.LeftHand ? leftHand : rightHand;

            if (!rotating || rotatingHand != currentNode)
            {
                rotating = true;
                zooming = false;
                rotatingHand = currentNode;
                hand.TryGetFeatureValue(CommonUsages.devicePosition, out initialHandPosition);
                Debug.Log("Rotation start with " + currentNode.ToString());
            }
            else
            {
                Vector3 currentHandPosition;
                hand.TryGetFeatureValue(CommonUsages.devicePosition, out currentHandPosition);
                float deltaX = currentHandPosition.x - initialHandPosition.x;

                float angle = deltaX * rotateSpeed;
                xrOrigin.Rotate(Vector3.up, angle * Time.deltaTime, Space.World);
                Debug.Log("Rotating XR Origin: " + angle.ToString("F2") + " degrees");
            }
        }
        else
        {
            if (zooming)
                Debug.Log("Zoom end");
            if (rotating)
                Debug.Log("Rotation end");

            zooming = false;
            rotating = false;
        }
    }
    #endregion



}
