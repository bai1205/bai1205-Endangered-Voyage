using System.Collections.Generic;
using UnityEngine;

public class VisibilityCullingManager : MonoBehaviour
{
    public static VisibilityCullingManager Instance { get; private set; }

    [Header("Camera")]
    public Camera targetCamera;                // ������� Camera.main

    private CullingGroup cullingGroup;
    private BoundingSphere[] spheres;
    private readonly List<Item> items = new();

    private struct Item
    {
        public Transform t;
        public Renderer[] renderers;           // Ҫ��ʾ/���ص�����
        public float radius;
    }

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        if (!targetCamera) targetCamera = Camera.main;
        cullingGroup = new CullingGroup();
        cullingGroup.onStateChanged += OnStateChanged;
        cullingGroup.targetCamera = targetCamera;
    }

    void OnDisable()
    {
        if (cullingGroup != null)
        {
            cullingGroup.onStateChanged -= OnStateChanged;
            cullingGroup.Dispose();
            cullingGroup = null;
        }
    }

    public int Register(Transform t, Renderer[] renderers, float radius)
    {
        items.Add(new Item { t = t, renderers = renderers, radius = radius });
        RebuildSpheres();
        // Ĭ�ϣ�ע��ʱ�����أ��ȿɼ�����ʾ
        SetRenderers(renderers, false);
        return items.Count - 1; // ��������
    }

    public void Unregister(Transform t)
    {
        int idx = items.FindIndex(i => i.t == t);
        if (idx >= 0)
        {
            items.RemoveAt(idx);
            RebuildSpheres();
        }
    }

    void RebuildSpheres()
    {
        int n = items.Count;
        if (n == 0) { cullingGroup.SetBoundingSpheres(System.Array.Empty<BoundingSphere>()); cullingGroup.SetBoundingSphereCount(0); return; }

        if (spheres == null || spheres.Length != n) spheres = new BoundingSphere[n];

        for (int i = 0; i < n; i++)
        {
            var it = items[i];
            spheres[i].position = it.t ? it.t.position : Vector3.zero;
            spheres[i].radius = it.radius <= 0 ? 1f : it.radius;
        }

        cullingGroup.SetBoundingSpheres(spheres);
        cullingGroup.SetBoundingSphereCount(n);
        // ��ѡ��������ֲ㣨��/��/Զ��
        // cullingGroup.SetDistanceReferencePoint(targetCamera.transform);
        // cullingGroup.SetBoundingDistances(new float[]{30, 60, 120});
    }

    void LateUpdate()
    {
        // ���°�Χ��λ�ã�������
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].t) spheres[i].position = items[i].t.position;
        }
        cullingGroup.SetBoundingSpheres(spheres);
    }

    void OnStateChanged(CullingGroupEvent e)
    {
        if (e.index < 0 || e.index >= items.Count) return;
        bool visible = e.isVisible; // ����׶��
        SetRenderers(items[e.index].renderers, visible);
    }

    static void SetRenderers(Renderer[] rs, bool on)
    {
        if (rs == null) return;
        for (int i = 0; i < rs.Length; i++)
            if (rs[i]) rs[i].enabled = on;
    }
}

