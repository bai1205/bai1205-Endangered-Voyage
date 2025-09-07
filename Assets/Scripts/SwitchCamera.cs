using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCamera : MonoBehaviour
{
    public GameObject Camera_1;                  // 普通模式的相机
    public GameObject Camera_2;                  // God view 模式的相机
    public Transform xrOriginToLock;             // XR Origin (God view) 的 transform

    public int Manager;

    private Vector3 lockedPosition;
    private Quaternion lockedRotation;
    private bool isXrOriginLocked = false;

    public void ChangeCamera()
    {
        GetComponent<Animator>().SetTrigger("Change");
    }

    private void Update()
    {
        if (isXrOriginLocked && xrOriginToLock != null)
        {
            xrOriginToLock.position = lockedPosition;
            xrOriginToLock.rotation = lockedRotation;
        }
    }

    public void ManageCamera()
    {
        if (Manager == 0)
        {
            Cam_2();
            Manager = 1;
        }
        else
        {
            Cam_1();
            Manager = 0;
        }
    }

    void Cam_1()
    {
        Camera_1.SetActive(true);
        Camera_2.SetActive(false);

        // 锁定 XR Origin transform
        if (xrOriginToLock != null)
        {
            lockedPosition = xrOriginToLock.position;
            lockedRotation = xrOriginToLock.rotation;
            isXrOriginLocked = true;
        }
    }

    void Cam_2()
    {
        Camera_2.SetActive(true);
        Camera_1.SetActive(false);

        // 解锁 XR Origin transform
        isXrOriginLocked = false;
    }
}
