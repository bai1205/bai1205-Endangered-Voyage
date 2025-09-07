using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ProtectionArea : MonoBehaviour
{
    [Header("Protection Setting")]
    public float protectionRadius = 10f;          // 进入半径
    public float exitBuffer = 1.0f;               // 退出缓冲（离开需 > radius+buffer）
    public string targetTag = "BabyRhino";
    public string protectedLayerName = "Protected";
    public string preyLayerName = "Prey";
    public float checkInterval = 0.25f;           // 多少秒检查一次

    [Header("Visualization")]
    [Range(8, 256)] public int segments = 48;     // 圆分段（越大越圆）
    public float ringHeight = 0.05f;              // 抬高一点，避免穿模
    public float lineWidth = 0.06f;
    public Color lineColor = new(0, 1, 1, 1);

    private LineRenderer _lr;

    // —— 全局保护计数（允许多个保护区叠加）——
    private static readonly Dictionary<GameObject, int> s_protectCount = new();
    // —— 本保护区内的对象集合（滞回用）——
    private readonly HashSet<GameObject> _insideSet = new();

    // 缓存层索引
    private int _protectedLayer = -1;
    private int _preyLayer = -1;

    private static readonly List<GameObject> _tempToRemove = new();

    private void Awake()
    {
        _protectedLayer = LayerMask.NameToLayer(protectedLayerName);
        _preyLayer = LayerMask.NameToLayer(preyLayerName);

        if (_protectedLayer < 0 || _preyLayer < 0)
        {
            Debug.LogError($"[ProtectionArea] Layer 名称错误：'{protectedLayerName}' 或 '{preyLayerName}' 未在 Project Settings > Tags and Layers 中定义。");
        }
    }

    private void Start()
    {
        // ——— LineRenderer 基础配置 ———
        _lr = GetComponent<LineRenderer>();
        _lr.loop = true;
        _lr.widthMultiplier = lineWidth;
        _lr.startColor = _lr.endColor = lineColor;
        _lr.receiveShadows = false;
        _lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // 可视化点缓存
        segments = Mathf.Max(8, segments);
        _lr.positionCount = segments;

        // 本地圆：一次绘制即可，自动跟随物体移动
        _lr.useWorldSpace = false;
        DrawLocalCircle();

        StartCoroutine(CheckProtectionLoop());
    }

    // —— 循环检查保护对象 —— //
    private IEnumerator CheckProtectionLoop()
    {
        var wait = new WaitForSeconds(checkInterval);
        while (true)
        {
            UpdateProtectedObjects();
            yield return wait;
        }
    }

    private void UpdateProtectedObjects()
    {
        var all = GameObject.FindGameObjectsWithTag(targetTag);
        float exitRadius = protectionRadius + exitBuffer;

        foreach (var go in all)
        {
            if (!go) continue;
            float dist = Vector3.Distance(transform.position, go.transform.position);

            bool wasInside = _insideSet.Contains(go);
            bool nowInside = wasInside ? (dist <= exitRadius) : (dist <= protectionRadius); // 滞回

            if (nowInside && !wasInside)
            {
                _insideSet.Add(go);
                AddProtection(go);
            }
            else if (!nowInside && wasInside)
            {
                _insideSet.Remove(go);
                RemoveProtection(go);
            }
        }

        // 清理无效引用（销毁/失活）
        _tempToRemove.Clear();
        foreach (var tracked in _insideSet)
        {
            if (!tracked || !tracked.activeInHierarchy)
                _tempToRemove.Add(tracked);
        }
        foreach (var dead in _tempToRemove)
        {
            _insideSet.Remove(dead);
            RemoveProtection(dead);
        }
    }

    // —— 本地圆 —— //
    private void DrawLocalCircle()
    {
        if (!_lr) return;

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments * Mathf.PI * 2f;
            Vector3 p = new(
                Mathf.Cos(t) * protectionRadius,
                ringHeight,
                Mathf.Sin(t) * protectionRadius
            );
            _lr.SetPosition(i, p);
        }
    }

    // —— 递归换层 —— //
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (!obj) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }

    // —— 全局引用计数：支持多个保护区叠加 —— //
    private void AddProtection(GameObject obj)
    {
        if (!s_protectCount.TryGetValue(obj, out int n)) n = 0;
        n++;
        s_protectCount[obj] = n;

        if (n == 1 && _protectedLayer >= 0)
            SetLayerRecursively(obj, _protectedLayer);
    }

    private void RemoveProtection(GameObject obj)
    {
        if (!s_protectCount.TryGetValue(obj, out int n)) return;
        n = Mathf.Max(0, n - 1);

        if (n == 0)
        {
            s_protectCount.Remove(obj);
            if (_preyLayer >= 0)
                SetLayerRecursively(obj, _preyLayer);
        }
        else
        {
            s_protectCount[obj] = n;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        segments = Mathf.Max(8, segments);
        exitBuffer = Mathf.Max(0f, exitBuffer);

        if (_lr)
        {
            _lr.widthMultiplier = lineWidth;
            _lr.startColor = _lr.endColor = lineColor;
            _lr.positionCount = segments;
            DrawLocalCircle();
        }
    }
#endif
}
