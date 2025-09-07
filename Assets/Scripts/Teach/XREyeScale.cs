using UnityEngine;
using UnityEngine.XR;

public class XREyeScale : MonoBehaviour
{
    [Range(0.5f, 1.5f)]
    public float scale = 0.8f;   // 0.7~0.9 通常能明显降 GPU 压力

    void Start()
    {
        XRSettings.eyeTextureResolutionScale = scale;   // 调整渲染目标分辨率
        XRSettings.renderViewportScale = Mathf.Clamp01(scale); // 进一步裁剪视口（可选）
        // 说明：eyeTextureResolutionScale 改“渲染贴图大小”，renderViewportScale 改“视口尺寸”
    }
}
