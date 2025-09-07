using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoCamera : MonoBehaviour
{
    public float distanceFromCamera = 2.5f;     // ��Ƶ������ͷǰ���ľ���
    public Vector3 offset = new Vector3(0, -0.3f, 0);  // ����ƫ�ƣ�������΢���ͣ�
    public bool alwaysFaceCamera = true;        // �Ƿ�ʼ�ճ�������ͷ
    public bool onlyPositionOnce = true;        // �Ƿ�ֻ�ڲ���ʱ����һ��

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
            transform.Rotate(0, 180f, 0); // �� RawImage ���泯�����������ɼ�
        }

        hasPositioned = true;
    }
}
