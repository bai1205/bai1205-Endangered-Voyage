using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BabyPrey : Animals, IChasable
{
    public Transform GetTransform() => transform;

    public void OnChasedBy(Predator predator)
    {
        AlertFrom(predator.transform, 10f, "Predator");
    }

    [Header("Baby Settings")]
    [SerializeField] private float growUpTime = 30f;
    [SerializeField] private float followRange = 10f;        //
    [SerializeField] private float followStopDistance = 2f;  // 
    [SerializeField] private float followCheckInterval = 1f; //
    [SerializeField] private GameObject adultPrefab;
    [SerializeField] private float detectRange = 10f;

    private Animals parent;

    public void AlertByBuilding(GameObject building)
    {
        AlertFrom(building.transform, detectRange, "Building");
    }

    protected override void InitialiseAnimal()
    {
        base.InitialiseAnimal();
        
        isAdult = false;
        canBreed = false;
        navMeshAgent.speed = walkSpeed * 0.5f;

        StartCoroutine(GrowUpRoutine());
        StartCoroutine(FollowParentRoutine());
    }

    public void SetParent(Animals parentAnimal)
    {
        parent = parentAnimal;
    }

    private IEnumerator GrowUpRoutine()
    {
        yield return new WaitForSeconds(growUpTime);

        if (adultPrefab != null)
        {
            GameObject adult = Instantiate(adultPrefab, transform.position, transform.rotation);
            adult.transform.localScale = Vector3.one; //
        }
        else
        {
            Debug.LogError($"{name}: adultPrefab δָ�����޷����ɳ����壡");
        }

        Destroy(gameObject); //
    }


    private IEnumerator FollowParentRoutine()
    {
        while (true)
        {
            if (parent == null)
            {
                navMeshAgent.ResetPath();
                yield break;
            }

            float distance = Vector3.Distance(transform.position, parent.transform.position);

            if (distance > followRange)
            {
                //
                SetState(AnimalState.Chase);
                navMeshAgent.SetDestination(parent.transform.position);
            }
            else if (distance <= followStopDistance)
            {
                //
                SetState(AnimalState.Idle);
                navMeshAgent.ResetPath();
            }

            yield return new WaitForSeconds(followCheckInterval);
        }
    }

    protected override void CheckChaseConditions() { }
    protected override void CheckFoodInRange() { }
    protected override void HandleSeekFoodState() { }
    protected override void HandleSeekWaterState() { }
    protected override void HandleBreedState() { }
    protected override void HandleChaseState() { }

    protected override void HandleMovingState(){}

    protected override void Die()
    {
        StopAllCoroutines();
        base.Die();
    }
}
