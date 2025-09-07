using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IChasable
{
    Transform GetTransform();                 // 返回目标的位置（用于导航）
    void OnChasedBy(Predator predator);       // 被追逐时做出反应
}
