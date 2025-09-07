using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullByView : MonoBehaviour
{
    [Tooltip("���������Զ�ץȡ�Լ����ӽڵ��ϵ� Renderer")]
    public Renderer[] targetRenderers;

    [Tooltip("��Χ��뾶�����°�ס���壩������᳢�Դ� Renderer bounds ����")]
    public float radius = 0f;

    private bool registered = false;

    void Start()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>(true);

        if (radius <= 0f)
        {
            // ���Թ���뾶
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

