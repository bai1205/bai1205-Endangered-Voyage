using UnityEngine;

public class WaterPoint : MonoBehaviour
{
    public int WaterID;
    public bool isPolluted = false;

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock mpb;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        mpb = new MaterialPropertyBlock();

        // ✅ 启动时把自己注册到管理器，避免每次 FindObjectsOfType
        var mgr = FindObjectOfType<WaterManager>();
        if (mgr != null) mgr.Register(this);
    }

    public void InitVisual()
    {
        if (!meshRenderer) return;

        meshRenderer.enabled = true; // ✅ 始終啟用渲染器，顏色反映污染狀態

        meshRenderer.GetPropertyBlock(mpb);

        Color targetColor = isPolluted ? Color.red : Color.green;

        if (!meshRenderer.sharedMaterial || !meshRenderer.sharedMaterial.HasProperty(BaseColor))
            mpb.SetColor("_Color", targetColor);
        else
            mpb.SetColor(BaseColor, targetColor);

        meshRenderer.SetPropertyBlock(mpb);
 

    }

    public void Pollute()
    {
        if (isPolluted) return;         // ✅ 幂等：已经污染过直接返回
        isPolluted = true;
        if (meshRenderer) meshRenderer.enabled = false;
        // 更新可视化状态
        InitVisual();
        
        // 建议减少频繁日志（真要打可以加个总开关）
        // Debug.Log($"Water point {WaterID} polluted.");
    }
}
