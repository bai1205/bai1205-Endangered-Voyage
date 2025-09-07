using UnityEngine;
using UnityEngine.XR;

public class FollowCameraTextUI : MonoBehaviour
{
    public float distanceFromCamera = 2f;   // UI 距离摄像头的距离
    public Vector3 offset = new Vector3(0, -0.2f, 0); // 相对位置偏移（比如稍微低一点）
    public bool followRotation = true;      // 是否始终朝向摄像头

    private Transform cameraTransform;

    void Start()
    {
        // 自动寻找 XR 主相机（带有 "MainCamera" 标签）
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            cameraTransform = mainCam.transform;
        }
        else
        {
            Debug.LogWarning("未找到主摄像头，请检查是否为 MainCamera 标签！");
        }
    }

    void Update()
    {
        if (cameraTransform == null) return;

        // 计算新位置：摄像头前方 + 偏移
        Vector3 forwardPosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera + offset;
        transform.position = forwardPosition;

        // 让 UI 朝向摄像头
        if (followRotation)
        {
            transform.LookAt(cameraTransform);
            transform.Rotate(0, 180f, 0); // 让文字正面朝向玩家
        }
    }
}
