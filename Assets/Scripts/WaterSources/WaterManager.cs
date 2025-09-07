using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    private readonly Dictionary<int, WaterPoint> map = new();
    private readonly HashSet<int> polluted = new();

    [Header("Pollution Settings")]
    public float pollutionInterval = 1f;   // ✅ 可在 Inspector 配置

    // ✅ 注册
    public void Register(WaterPoint wp)
    {
        if (wp == null) return;
        map[wp.WaterID] = wp;
        wp.InitVisual();
    }

    public bool PolluteWaterByID(int id)
    {
        if (polluted.Contains(id)) return false;
        if (!map.TryGetValue(id, out var wp) || wp == null) return false;
        if (wp.isPolluted) { polluted.Add(id); return false; }

        wp.Pollute();
        polluted.Add(id);
        return true;
    }

    // 🚀 开启污染序列
    public void StartPollutionSequence()
    {
        StopAllCoroutines(); // 防止重复开启
        StartCoroutine(PollutionRoutine());
    }

    private IEnumerator PollutionRoutine()
    {
        var ids = map.Keys.OrderBy(id => id).ToList();
        foreach (var id in ids)
        {
            PolluteWaterByID(id);
            yield return new WaitForSeconds(pollutionInterval); // ✅ 使用 Inspector 配置的间隔
        }
    }

    public WaterPoint GetNearestAvailable(Vector3 pos, float maxDist = Mathf.Infinity)
    {
        WaterPoint best = null; float bestDist = maxDist;
        foreach (var kv in map)
        {
            var wp = kv.Value;
            if (!wp || wp.isPolluted) continue;
            float d = Vector3.Distance(pos, wp.transform.position);
            if (d < bestDist) { bestDist = d; best = wp; }
        }
        return best;
    }
}
