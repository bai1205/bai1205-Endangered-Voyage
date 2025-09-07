using UnityEngine;

public class ParallelGround : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            // ��ǰ��ǰ���򣨱��ֲ��䣩
            Vector3 forward = transform.forward;

            // ������ up �������Ե��淨��
            Vector3 up = hit.normal;

            // ���¼��� rotation��ֻ���� forward �� up ȷ����̬��
            Quaternion targetRotation = Quaternion.LookRotation(forward, up);

            // ƽ����ֵ������˲��Ťת
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
