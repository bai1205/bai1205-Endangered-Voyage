using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskUIController : MonoBehaviour
{

    [Header("UI")]
    public TMP_Text titleText;
    public Slider progress;

    public void SetTitle(string title){
        titleText.text = title;
    }

    public void SetProgress(float current, float max)
    {
        if (!progress) return;

        progress.maxValue = max;
        progress.value = Mathf.Clamp(current, 0, max);
    }
}
