using UnityEngine;
using UnityEngine.XR;

public class XREyeScale : MonoBehaviour
{
    [Range(0.5f, 1.5f)]
    public float scale = 0.8f;   // 0.7~0.9 ͨ�������Խ� GPU ѹ��

    void Start()
    {
        XRSettings.eyeTextureResolutionScale = scale;   // ������ȾĿ��ֱ���
        XRSettings.renderViewportScale = Mathf.Clamp01(scale); // ��һ���ü��ӿڣ���ѡ��
        // ˵����eyeTextureResolutionScale �ġ���Ⱦ��ͼ��С����renderViewportScale �ġ��ӿڳߴ硱
    }
}
