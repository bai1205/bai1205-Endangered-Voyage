using UnityEngine;

public class AnchorFollowTarget : MonoBehaviour
{
    [Header("����˭��Ϭţ��")]
    public Transform target;

    [Header("���Ŀ���ƫ�ƣ�����ռ䣩")]
    public float height = 10f;          // ������Ŀ�����Ϸ��ĸ߶�
    public Vector3 extraOffset = Vector3.zero; // ����ƫ�ƣ���ѡ��ˮƽλ�ƣ�

    [Header("��������")]
    public bool lookDown = true;        // �Ƿ��ӳ���
    [Range(10f, 89f)] public float pitchDown = 60f; // ���ӽǶȣ�������Ǵ�90�ȶ��ӣ�

    [Header("����ƽ��")]
    public bool smoothFollow = true;
    public float posLerp = 10f;
    public float rotLerp = 10f;

    void LateUpdate()
    {
        if (!target) return;

        // Ŀ��λ�ã���Ŀ�����Ϸ� + ����ƫ��
        Vector3 desiredPos = target.position + Vector3.up * height + extraOffset;

        // Ŀ����ת�����¿����򱣳ֵ�ǰ
        Quaternion desiredRot = transform.rotation;
        if (lookDown)
        {
            // ���ӣ��� X ����ѹ��pitchDown����������Ŀ��� Yaw ���Ա���һ�£���ѡ��
            float yaw = target.eulerAngles.y;     // ���������� Yaw���ɸ�Ϊ 0
            desiredRot = Quaternion.Euler(pitchDown, yaw, 0f);
        }

        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-posLerp * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-rotLerp * Time.deltaTime));
        }
        else
        {
            transform.SetPositionAndRotation(desiredPos, desiredRot);
        }
    }
}
