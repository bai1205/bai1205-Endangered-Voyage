using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FoodSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private float spawnRadius = 30f;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxFoodCount = 20;

    [Header("NavMesh")]
    [SerializeField] private float navMeshSampleDistance = 5f;

    private List<GameObject> spawnedFoods = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(SpawnFoodRoutine());
    }

    private IEnumerator SpawnFoodRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 清理已经被吃掉的食物引用（null）
            spawnedFoods.RemoveAll(item => item == null);

            if (spawnedFoods.Count < maxFoodCount)
            {
                Vector3 spawnPos;
                if (TryGetRandomNavMeshPosition(out spawnPos))
                {
                    GameObject food = Instantiate(foodPrefab, spawnPos, Quaternion.identity);
                    spawnedFoods.Add(food);
                }
            }
        }
    }

    private bool TryGetRandomNavMeshPosition(out Vector3 result)
    {
        for (int i = 0; i < 10; i++) // 最多尝试10次
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = transform.position.y;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
