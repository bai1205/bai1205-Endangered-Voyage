using UnityEngine;

public class ParallelGround : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            // 当前的前向方向（保持不变）
            Vector3 forward = transform.forward;

            // 计算新 up 方向来自地面法线
            Vector3 up = hit.normal;

            // 重新计算 rotation（只根据 forward 和 up 确定姿态）
            Quaternion targetRotation = Quaternion.LookRotation(forward, up);

            // 平滑插值，避免瞬间扭转
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
