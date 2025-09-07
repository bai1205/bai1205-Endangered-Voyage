using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneWarmup : MonoBehaviour
{
    [Header("Per-frame init limits")]
    public int waterPerFrame = 50;
    //public int animalsPerFrame = 3;

    [Header("Animals Root (optional)")]
    public Transform animalsRoot; // ������ж���ͳһ�ĸ����壬�����Ͻ���
    public int animalsPerBatch = 2;        // ÿ�����ü������ԽСԽ����
    public float batchInterval = 1f;     // ÿ��֮��ȴ���ã��룩
    public float initialDelay = 3f;      // ������ʼ���ȵ�һ��������

    private IEnumerator Start()
    {
        yield return null; // �ȵ�һ֡

        // 1) ˮԴ���ӳ�ʼ������֡��
        var waters = FindObjectsOfType<WaterPoint>(true);
        int c = 0;
        foreach (var w in waters)
        {
            w.InitVisual();
            if (++c % waterPerFrame == 0)
                yield return null;
        }

        // 2) �����������
        yield return new WaitForSeconds(initialDelay);
        yield return StartCoroutine(EnableAnimalsInBatches());
    }

    private IEnumerator EnableAnimalsInBatches()
    {
        // �ռ�Ҫ���õĶ�����󣨵�ǰ�ǡ�δ����ģ�
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
            // û�и��ڵ���� Tag��ȷ�����ﶼ���� "Animal" ��ǩ��
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

            // ������ȴ�������ǡ����������Ĺؼ���
            yield return new WaitForSeconds(batchInterval);
        }
    }
}
