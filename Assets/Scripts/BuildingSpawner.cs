using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnData
{
    public Vector3 position;
    public GameObject prefab; // 對應 buildingPrefabs 裡的 index（0～5）
}

public class BuildingSpawner : MonoBehaviour
{
    public List<SpawnData> spawnPoints = new List<SpawnData>();

    public void Spawn(){
        StartCoroutine(SpawnAndBuild());
    }


    public IEnumerator SpawnAndBuild()
    {
        foreach (var data in spawnPoints)
        {

            if (data.prefab == null)
            {
                Debug.LogWarning("Prefab 未設定！");
                continue;
            }

            GameObject obj = Instantiate(data.prefab, data.position, Quaternion.identity);

            BuildingManager manager = obj.GetComponent<BuildingManager>();
            if (manager != null)
            {
                manager.StartBuild();
            }
            yield return new WaitForSeconds(5f);

        }
    }
}
