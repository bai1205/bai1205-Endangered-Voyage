using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAlert
{
    // ����
    void AlertByHunter(HunterManger hunter);
    void AlertByBuilding(GameObject building);

}