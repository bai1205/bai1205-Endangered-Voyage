using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEatable
{
    int GetNutrition();
    void OnEaten(float delay = 0f); // 支持传入延迟时间
}
