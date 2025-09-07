using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Ŀ������")]
    public Transform target;        // �����Ŀ�꣨��ҽ�ɫ��
    public Vector3 offset = new Vector3(0, 2, -4); // ��ʼƫ��

    [Header("��ת����")]
    public float mouseSensitivity = 100f;  // ���������
    public float minPitch = -30f;          // ��С����
    public float maxPitch = 60f;           // �������

    [Header("ƽ������")]
    public float rotationSmoothTime = 0.1f;

    [Header("���Ʋ���")]
    public float orbitSpeed = 30f;         // ��Fʱ��Ŀ����ٶȣ���/�룩

    private float yaw;     // ˮƽ��ת��
    private float pitch;   // ��ֱ��ת��
    private Vector3 currentRotation;
    private Vector3 rotationSmoothVelocity;

    private bool isOrbiting = false; // �Ƿ��ڻ���ģʽ

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("ThirdPersonCamera: û�а�Ŀ�����");
            enabled = false;
            return;
        }

        // ��ʼ���������ת
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // �������ָ��
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // ���F��״̬
        if (Input.GetKeyDown(KeyCode.F))
            isOrbiting = true;
        if (Input.GetKeyUp(KeyCode.F))
            isOrbiting = false;

        if (isOrbiting)
        {
            // ������Ŀ����ת
            yaw += orbitSpeed * Time.deltaTime;
        }
        else
        {
            // ������
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // ƽ����ת
        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        // Ӧ����ת
        transform.eulerAngles = currentRotation;

        // ���������λ��
        transform.position = target.position + transform.rotation * offset;
    }
}
