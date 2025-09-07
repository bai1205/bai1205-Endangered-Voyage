using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;           // 基础移动速度
    public float fastMultiplier = 2f;      // 按住Shift加速倍数

    [Header("旋转参数")]
    public float mouseSensitivity = 2f;    // 鼠标灵敏度
    public bool lockCursor = true;         // 是否锁定鼠标光标

    private float yaw;   // 水平旋转角
    private float pitch; // 垂直旋转角

    void Start()
    {
        // 锁定鼠标光标
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 初始化旋转
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f); // 防止翻转

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        // 获取WASD输入
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        // 按住Shift加速
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= fastMultiplier;
        }

        // 移动向量（相对于摄像机方向）
        Vector3 move = transform.forward * vertical + transform.right * horizontal;
        transform.position += move * speed * Time.deltaTime;
    }
}
