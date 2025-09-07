using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target;        // 跟随的目标（玩家角色）
    public Vector3 offset = new Vector3(0, 2, -4); // 初始偏移

    [Header("旋转参数")]
    public float mouseSensitivity = 100f;  // 鼠标灵敏度
    public float minPitch = -30f;          // 最小俯角
    public float maxPitch = 60f;           // 最大仰角

    [Header("平滑参数")]
    public float rotationSmoothTime = 0.1f;

    [Header("环绕参数")]
    public float orbitSpeed = 30f;         // 按F时绕目标的速度（度/秒）

    private float yaw;     // 水平旋转角
    private float pitch;   // 垂直旋转角
    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;

    private bool isOrbiting = false; // 是否处于环绕模式

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("ThirdPersonCamera: 没有绑定目标对象！");
            enabled = false;
            return;
        }

        // 初始化摄像机旋转
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // 锁定鼠标指针
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // 检测F键状态
        if (Input.GetKeyDown(KeyCode.F))
            isOrbiting = true;
        if (Input.GetKeyUp(KeyCode.F))
            isOrbiting = false;

        if (isOrbiting)
        {
            // 匀速绕目标旋转
            yaw += orbitSpeed * Time.deltaTime;
        }
        else
        {
            // 鼠标控制
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // 平滑旋转
        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        // 应用旋转
        transform.eulerAngles = currentRotation;

        // 计算摄像机位置
        transform.position = target.position + transform.rotation * offset;
    }
}
