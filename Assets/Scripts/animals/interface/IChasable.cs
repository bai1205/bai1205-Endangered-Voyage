using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IChasable
{
    Transform GetTransform();                 // ����Ŀ���λ�ã����ڵ�����
    void OnChasedBy(Predator predator);       // ��׷��ʱ������Ӧ
}
