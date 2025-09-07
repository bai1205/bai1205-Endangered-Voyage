using UnityEngine;

public class AnchorFollowTarget : MonoBehaviour
{
    [Header("跟随谁（犀牛）")]
    public Transform target;

    [Header("相对目标的偏移（世界空间）")]
    public float height = 10f;          // 保持在目标正上方的高度
    public Vector3 extraOffset = Vector3.zero; // 额外偏移（可选，水平位移）

    [Header("朝向设置")]
    public bool lookDown = true;        // 是否俯视朝下
    [Range(10f, 89f)] public float pitchDown = 60f; // 俯视角度（如果不是纯90度顶视）

    [Header("跟随平滑")]
    public bool smoothFollow = true;
    public float posLerp = 10f;
    public float rotLerp = 10f;

    void LateUpdate()
    {
        if (!target) return;

        // 目标位置：在目标正上方 + 额外偏移
        Vector3 desiredPos = target.position + Vector3.up * height + extraOffset;

        // 目标旋转：向下看，或保持当前
        Quaternion desiredRot = transform.rotation;
        if (lookDown)
        {
            // 俯视：绕 X 轴下压（pitchDown），朝向与目标的 Yaw 可以保持一致（可选）
            float yaw = target.eulerAngles.y;     // 如果不想跟随 Yaw，可改为 0
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
