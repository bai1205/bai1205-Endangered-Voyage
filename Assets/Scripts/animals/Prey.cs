using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prey : Animals,IChasable,IAlert
{
    public Transform GetTransform() => transform;
    [SerializeField] private LayerMask foodMask;

    public void OnChasedBy(Predator predator)
    {
        AlertByPredator(predator);
    }

    [Header("Prey Variable")]
    [SerializeField] private float detectRange = 10f;
    //[SerializeField] private float escapeMaxDistance = 80f;

    private Predator currentPredator = null;
    private HunterManger currentHunter = null;

    public void AlertByHunter(HunterManger hunter)
    {
        AlertFrom(hunter.transform, detectRange, "Hunter");
    }

    public void AlertByBuilding(GameObject building)
    {
        AlertFrom(building.transform, detectRange, "Building");
    }

    public void AlertByPredator(Predator predator)
    {
        AlertFrom(predator.transform, detectRange, "Predator");
    }

    #region CITE
    /*public void AlertPrey(Predator predator)
    {
        SetState(AnimalState.Chase);
        currentPredator = predator;
        StartCoroutine(RunFromPredator());
        Debug.Log("Prey alerted by predator: " + predator.name);

    }

    public void AlertPreyByHunter(HunterManger Hunter)
    {
        SetState(AnimalState.Chase);
        currentHunter = Hunter;
        StartCoroutine(RunFromHunter());
        Debug.Log("Prey alerted by Hunter: " + Hunter.name);

    }

    private IEnumerator RunFromHunter()
    {
        // Wait until a Hunter is detected within the range
        while (currentHunter == null || Vector3.Distance(transform.position, currentHunter.transform.position) > detectRange)
        {
            yield return null;
        }

        // Start running away from the Hunter
        while (currentHunter != null && Vector3.Distance(transform.position, currentHunter.transform.position) <= detectRange)
        {
            RunAwayFromHunter();

            yield return null;
        }

        // Stop running when the Hunter is no longer detected
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        SetState(AnimalState.Idle); 

    }

    private void RunAwayFromHunter()
    {
        if(navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            if(!navMeshAgent.pathPending && navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
            {
                Vector3 runDirection = (transform.position - currentHunter.transform.position);
                Vector3 escapeDestination = transform.position + runDirection.normalized * (escapeMaxDistance * 2);

                navMeshAgent.SetDestination(GetRandomNavMeshPosition(escapeDestination,escapeMaxDistance));
            }

        }
    }

    private IEnumerator RunFromPredator()
    {
        // Wait until a predator is detected within the range
        while (currentPredator == null || Vector3.Distance(transform.position, currentPredator.transform.position) > detectRange)
        {
            yield return null;
        }

        // Start running away from the predator
        while (currentPredator != null && Vector3.Distance(transform.position, currentPredator.transform.position) <= detectRange)
        {
            RunAwayFromPredator();

            yield return null;
        }

        // Stop running when the predator is no longer detected
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        SetState(AnimalState.Idle); 

    }

    private void RunAwayFromPredator()
    {
        if(navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            if(!navMeshAgent.pathPending && navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
            {
                Vector3 runDirection = (transform.position - currentPredator.transform.position);
                Vector3 escapeDestination = transform.position + runDirection.normalized * (escapeMaxDistance * 2);

                navMeshAgent.SetDestination(GetRandomNavMeshPosition(escapeDestination,escapeMaxDistance));
            }

        }
    }*/

    #endregion

    protected override void Die()
    {
        StopAllCoroutines();
        base.Die();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }

    protected override void CheckFoodInRange()
    {
        Debug.Log("Checking for food within detection range: " + detectRange);

        Collider[] colliders = new Collider[10];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectRange, colliders, foodMask);
        
        for (int i = 0; i < numColliders; i++)
        {
            if (colliders[i].CompareTag("Food"))
            {
                Debug.Log("Food found: " + colliders[i].name);

                navMeshAgent.SetDestination(colliders[i].transform.position);
                SetState(AnimalState.SeekFood); //change state to SeekFood
                return;
            }
        }

        Debug.Log("No food found in range.");
    }










































}
