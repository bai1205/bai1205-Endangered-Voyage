using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullByView : MonoBehaviour
{
    [Tooltip("如果不填，就自动抓取自己和子节点上的 Renderer")]
    public Renderer[] targetRenderers;

    [Tooltip("包围球半径（大致包住物体）。不填会尝试从 Renderer bounds 估算")]
    public float radius = 0f;

    private bool registered = false;

    void Start()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>(true);

        if (radius <= 0f)
        {
            // 粗略估算半径
            Bounds b = new Bounds(transform.position, Vector3.zero);
            foreach (var r in targetRenderers) if (r) b.Encapsulate(r.bounds);
            radius = Mathf.Max(0.5f, b.extents.magnitude);
        }

        if (VisibilityCullingManager.Instance == null)
        {
            Debug.LogWarning("No VisibilityCullingManager in scene. Add one to a GameObject.");
            return;
        }

        VisibilityCullingManager.Instance.Register(transform, targetRenderers, radius);
        registered = true;
    }

    void OnDestroy()
    {
        if (registered && VisibilityCullingManager.Instance)
            VisibilityCullingManager.Instance.Unregister(transform);
    }
}

