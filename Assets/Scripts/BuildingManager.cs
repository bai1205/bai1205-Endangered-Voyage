using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public GameObject cube;
    public ParticleSystem smokeFX;
    public float growDuration = 5f;

    private Material mat;
    private Vector3 targetScale;
    private Coroutine buildCoroutine;

    void Awake()
    {
        if (cube != null)
        {
            // 儲存原始大小
            targetScale = cube.transform.localScale;
        }
    }

    public void StartBuild()
    {
        if (buildCoroutine != null)
        {
            StopCoroutine(buildCoroutine);
        }

        buildCoroutine = StartCoroutine(BuildCube());
    }

    private IEnumerator BuildCube()
    {
        if (cube == null) yield break;

        // 初始化尺寸與透明度
        cube.transform.localScale = Vector3.zero;

    

        // 播放粒子
        if (smokeFX != null)
        {
            smokeFX.Play();
        }

        float timer = 0f;
        while (timer < growDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / growDuration);

            // 變大
            cube.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);

            yield return null;
        }

        // 停止煙霧（自然淡出）
        if (smokeFX != null)
        {
            smokeFX.Stop();
        }

        buildCoroutine = null;
    }
}

