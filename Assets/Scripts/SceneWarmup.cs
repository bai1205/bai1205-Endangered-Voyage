using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneWarmup : MonoBehaviour
{
    [Header("Per-frame init limits")]
    public int waterPerFrame = 50;
    //public int animalsPerFrame = 3;

    [Header("Animals Root (optional)")]
    public Transform animalsRoot; // 如果你有动物统一的父物体，可以拖进来
    public int animalsPerBatch = 2;        // 每批启用几个动物（越小越慢）
    public float batchInterval = 1f;     // 每批之间等待多久（秒）
    public float initialDelay = 3f;      // 场景开始后先等一会再启用

    private IEnumerator Start()
    {
        yield return null; // 先等一帧

        // 1) 水源可视初始化（分帧）
        var waters = FindObjectsOfType<WaterPoint>(true);
        int c = 0;
        foreach (var w in waters)
        {
            w.InitVisual();
            if (++c % waterPerFrame == 0)
                yield return null;
        }

        // 2) 动物分批启用
        yield return new WaitForSeconds(initialDelay);
        yield return StartCoroutine(EnableAnimalsInBatches());
    }

    private IEnumerator EnableAnimalsInBatches()
    {
        // 收集要启用的动物对象（当前是“未激活”的）
        var animals = new List<GameObject>();

        if (animalsRoot != null)
        {
            foreach (Transform child in animalsRoot)
            {
                if (child != null && !child.gameObject.activeSelf)
                    animals.Add(child.gameObject);
            }
        }
        else
        {
            // 没有父节点就用 Tag（确保动物都打了 "Animal" 标签）
            foreach (var go in GameObject.FindGameObjectsWithTag("Animal"))
                if (!go.activeSelf) animals.Add(go);
        }

        int index = 0;
        while (index < animals.Count)
        {
            int thisBatch = 0;
            while (thisBatch < animalsPerBatch && index < animals.Count)
            {
                var go = animals[index++];
                if (go != null && !go.activeSelf)
                    go.SetActive(true);
                thisBatch++;
            }

            // 批间隔等待（这就是“慢下来”的关键）
            yield return new WaitForSeconds(batchInterval);
        }
    }
}
