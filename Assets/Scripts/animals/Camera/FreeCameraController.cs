using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    [Header("�ƶ�����")]
    public float moveSpeed = 5f;           // �����ƶ��ٶ�
    public float fastMultiplier = 2f;      // ��סShift���ٱ���

    [Header("��ת����")]
    public float mouseSensitivity = 2f;    // ���������
    public bool lockCursor = true;         // �Ƿ����������

    private float yaw;   // ˮƽ��ת��
    private float pitch; // ��ֱ��ת��

    void Start()
    {
        // ���������
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // ��ʼ����ת
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
        pitch = Mathf.Clamp(pitch, -89f, 89f); // ��ֹ��ת

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        // ��ȡWASD����
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        // ��סShift����
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= fastMultiplier;
        }

        // �ƶ���������������������
        Vector3 move = transform.forward * vertical + transform.right * horizontal;
        transform.position += move * speed * Time.deltaTime;
    }
}
