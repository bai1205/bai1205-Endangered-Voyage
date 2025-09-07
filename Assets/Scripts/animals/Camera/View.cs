using UnityEngine;
using UnityEngine.InputSystem;

public class View: MonoBehaviour
{
    [Header("Ŀ�����ӽ�")]
    public Transform target;
    public float distance = 5f;
    public float height = 2f;
    public float followSpeed = 10f;

    [Header("��ת����")]
    public float rotationSpeed = 60f; // ÿ�������ת�Ƕ�
    public InputActionProperty rightStickInput; // Input System �� Vector2 ������ͨ������ҡ�ˣ�

    private float currentYaw = 0f;

    private void OnEnable()
    {
        rightStickInput.action.Enable();
    }

    private void OnDisable()
    {
        rightStickInput.action.Disable();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ��ȡ��ҡ�� X ������ֵ��-1 �� 1��
        Vector2 stick = rightStickInput.action.ReadValue<Vector2>();
        float inputX = stick.x;

        // ���� Yaw��ˮƽ��ת��
        currentYaw += inputX * rotationSpeed * Time.deltaTime;

        // ������ת���λ��
        Quaternion rotation = Quaternion.Euler(0, currentYaw, 0);
        Vector3 targetPosition = target.position - (rotation * Vector3.forward * distance) + Vector3.up * height;

        // ƽ���ƶ������
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // ���������Ŀ��
        transform.LookAt(target.position + Vector3.up * height * 0.5f);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
