using UnityEngine;
using UnityEngine.InputSystem;

public class View: MonoBehaviour
{
    [Header("目标与视角")]
    public Transform target;
    public float distance = 5f;
    public float height = 2f;
    public float followSpeed = 10f;

    [Header("旋转设置")]
    public float rotationSpeed = 60f; // 每秒最大旋转角度
    public InputActionProperty rightStickInput; // Input System 的 Vector2 动作（通常绑定右摇杆）

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

        // 获取右摇杆 X 轴输入值（-1 到 1）
        Vector2 stick = rightStickInput.action.ReadValue<Vector2>();
        float inputX = stick.x;

        // 增加 Yaw（水平旋转）
        currentYaw += inputX * rotationSpeed * Time.deltaTime;

        // 计算旋转后的位置
        Quaternion rotation = Quaternion.Euler(0, currentYaw, 0);
        Vector3 targetPosition = target.position - (rotation * Vector3.forward * distance) + Vector3.up * height;

        // 平滑移动摄像机
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // 摄像机看向目标
        transform.LookAt(target.position + Vector3.up * height * 0.5f);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
