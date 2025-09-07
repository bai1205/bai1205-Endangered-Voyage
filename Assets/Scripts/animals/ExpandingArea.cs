using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandingArea : MonoBehaviour
{
    [Header("Area Settings")]
    public float startRadius = 2f;          // 初始半径
    public float maxRadius = 20f;           // 最大半径
    public float expandSpeed = 1f;          // 每秒增长速度

    [Header("Detection Layers")]
    public LayerMask animalLayer;           // 用于检测动物的 Layer
    public LayerMask foodLayer;             // 用于检测食物的 Layer

    [Header("Food Settings")]
    public string foodTag = "Food";         // 当前食物的 Tag
    public string defaultTag = "Untagged";  // 改成的默认 Tag
    public string defaultLayerName = "Default"; // 改成的默认 Layer

    [Header("Alert Settings")]
    public bool callAlert = true;           // 是否对动物调用 AlertByBuilding

    private float currentRadius;
    private int defaultLayer;

    private void Start()
    {
        currentRadius = startRadius;
        defaultLayer = LayerMask.NameToLayer(defaultLayerName);

        if (defaultLayer == -1)
            Debug.LogWarning($"⚠️ Layer '{defaultLayerName}' 未找到，请在 Project Settings → Tags and Layers 添加！");
    }

    private void Update()
    {
        // 半径随时间增长
        if (currentRadius < maxRadius)
        {
            currentRadius += expandSpeed * Time.deltaTime;
            currentRadius = Mathf.Min(currentRadius, maxRadius);
        }

        // 检测动物 & 食物
        ProcessAnimalsInRange();
        ProcessFoodInRange();
    }

    private void ProcessAnimalsInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, currentRadius, animalLayer);

        foreach (var hit in hits)
        {
            if (callAlert && hit.TryGetComponent(out IAlert alertTarget))
            {
                alertTarget.AlertByBuilding(gameObject);
                // Debug.Log($"🚨 {hit.name} 被驱赶源驱赶");
            }
        }
    }

    private void ProcessFoodInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, currentRadius, foodLayer);

        foreach (var hit in hits)
        {
            GameObject obj = hit.gameObject;

            if (obj.CompareTag(foodTag))
            {
                obj.tag = defaultTag;
                if (defaultLayer != -1)
                    obj.layer = defaultLayer;

                // Debug.Log($"🍞 {obj.name} 食物被标记为默认，动物无法识别");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 初始半径（绿色）
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, startRadius);

        // 当前半径（红色）
        Gizmos.color = Color.red;
        float radiusToDraw = Application.isPlaying ? currentRadius : startRadius;
        Gizmos.DrawWireSphere(transform.position, radiusToDraw);

        // 最大半径（蓝色）
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxRadius);
    }
}
