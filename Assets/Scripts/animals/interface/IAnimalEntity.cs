using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAlert
{
    // ¾¯¸æ
    void AlertByHunter(HunterManger hunter);
    void AlertByBuilding(GameObject building);

}