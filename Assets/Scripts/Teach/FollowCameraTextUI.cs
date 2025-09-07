using UnityEngine;
using UnityEngine.XR;

public class FollowCameraTextUI : MonoBehaviour
{
    public float distanceFromCamera = 2f;   // UI ��������ͷ�ľ���
    public Vector3 offset = new Vector3(0, -0.2f, 0); // ���λ��ƫ�ƣ�������΢��һ�㣩
    public bool followRotation = true;      // �Ƿ�ʼ�ճ�������ͷ

    private Transform cameraTransform;

    void Start()
    {
        // �Զ�Ѱ�� XR ����������� "MainCamera" ��ǩ��
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            cameraTransform = mainCam.transform;
        }
        else
        {
            Debug.LogWarning("δ�ҵ�������ͷ�������Ƿ�Ϊ MainCamera ��ǩ��");
        }
    }

    void Update()
    {
        if (cameraTransform == null) return;

        // ������λ�ã�����ͷǰ�� + ƫ��
        Vector3 forwardPosition = cameraTransform.position + cameraTransform.forward * distanceFromCamera + offset;
        transform.position = forwardPosition;

        // �� UI ��������ͷ
        if (followRotation)
        {
            transform.LookAt(cameraTransform);
            transform.Rotate(0, 180f, 0); // ���������泯�����
        }
    }
}
