using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoCamera : MonoBehaviour
{
    public float distanceFromCamera = 2.5f;     // 视频在摄像头前方的距离
    public Vector3 offset = new Vector3(0, -0.3f, 0);  // 额外偏移（比如略微降低）
    public bool alwaysFaceCamera = true;        // 是否始终朝向摄像头
    public bool onlyPositionOnce = true;        // 是否只在播放时出现一次

    private Transform cameraTransform;
    private bool hasPositioned = false;

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (cameraTransform == null) return;

        if (!onlyPositionOnce || !hasPositioned)
        {
            PositionInFrontOfCamera();
        }
    }

    public void PositionInFrontOfCamera()
    {
        Vector3 targetPos = cameraTransform.position + cameraTransform.forward * distanceFromCamera + offset;
        transform.position = targetPos;

        if (alwaysFaceCamera)
        {
            transform.LookAt(cameraTransform);
            transform.Rotate(0, 180f, 0); // 让 RawImage 背面朝向相机，正面可见
        }

        hasPositioned = true;
    }
}
