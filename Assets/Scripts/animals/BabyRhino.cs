using System.Collections;
using UnityEngine;

public class BabyRhino : MonoBehaviour
{
    [Header("Baby Settings")]
    [SerializeField] private float growUpTime = 300f;
    [SerializeField] private Vector3 adultScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private string adultTag = "Rhino";
    [SerializeField] private string adultLayerName = "Animal";
    [SerializeField] private float growDuration = 3f; // 平滑变大时间

    private void Start()
    {
        StartCoroutine(GrowUpRoutine());
    }


    private IEnumerator GrowUpRoutine()
    {
        yield return new WaitForSeconds(growUpTime);

        Debug.Log($"{name} 正在成长...");

        // 原始和目标大小
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = adultScale;
        float growDuration = 3f; // 成长动画时长
        float timer = 0f;

        while (timer < growDuration)
        {
            timer += Time.deltaTime;
            float t = timer / growDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;

        // 设置 Tag
        gameObject.tag = adultTag;

        // 设置 Layer
        int adultLayer = LayerMask.NameToLayer(adultLayerName);
        if (adultLayer != -1)
            gameObject.layer = adultLayer;
        else
            Debug.LogWarning($"⚠️ Layer '{adultLayerName}' 未找到");

        // 启用 RhinoController 行为
        RhinoController controller = GetComponent<RhinoController>();
        if (controller != null)
            controller.enabled = true;

        // 禁用 BabyRhino 脚本
        this.enabled = false;

        Debug.Log($"{name} 成长完毕！");
    }

}
