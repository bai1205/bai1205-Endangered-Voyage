using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class Predator : Animals, IAlert
{
    [Header("Predator Variables")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float maxChaseTime = 10f;
    [SerializeField] private int biteDamage = 3;
    [SerializeField] private float biteCooldown=1f;
    //[SerializeField] private float escapeMaxDistance = 80f;



    [SerializeField] private LayerMask preyMask;
    [SerializeField] private LayerMask bodyMask;

    //private int protectedLayer;
    private IChasable currentChaseTarget;
    private HunterManger currentHunter = null;

    public void AlertByHunter(HunterManger hunter)
    {
        AlertFrom(hunter.transform, detectionRange, "Hunter");
    }

    public void AlertByBuilding(GameObject building)
    {
        AlertFrom(building.transform, detectionRange, "Building");
    }

    #region CITE
    /*public void AlertPredatorByHunter(HunterManger Hunter)
    {
        SetState(AnimalState.Chase);
        currentHunter = Hunter;
        StartCoroutine(RunFromHunter());
        Debug.Log("Predator alerted by Hunter: " + Hunter.name);

    }

    private IEnumerator RunFromHunter()
    {
        // Wait until a Hunter is detected within the range
        while (currentHunter == null || Vector3.Distance(transform.position, currentHunter.transform.position) > detectionRange)
        {
            yield return null;
        }

        // Start running away from the Hunter
        while (currentHunter != null && Vector3.Distance(transform.position, currentHunter.transform.position) <= detectionRange)
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
    }*/

    #endregion
    protected override void CheckChaseConditions()
    {
        //Debug.Log("Checking for prey within detection range: " + detectionRange);

        if (currentChaseTarget != null)
            return;

        Collider[] colliders = new Collider[10];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, colliders, preyMask);

        for (int i = 0; i < numColliders; i++) 
        {
            Collider collider = colliders[i];

            if (collider.TryGetComponent<IChasable>(out IChasable target))
            {
                StartChase(target);
                return;
            }

        }

        currentChaseTarget = null; // No prey found within detection range

    }

    private void StartChase(IChasable target)
    {
        currentChaseTarget = target;
        SetState(AnimalState.Chase);
    }


    protected override void HandleChaseState()
    {
        if (currentChaseTarget != null)
        {
            currentChaseTarget.OnChasedBy(this);
            StartCoroutine(ChasePrey());
        }
        else
        {
            SetState(AnimalState.Idle);
        }
    }

    private IEnumerator ChasePrey()
    {
        float startTime = Time.time;

        Transform targetTransform = currentChaseTarget?.GetTransform();

        // 💡 提前判断是否为空或被销毁
        if (targetTransform == null || currentChaseTarget.Equals(null))
        {
            StopChase();
            yield break;
        }

        currentChaseTarget.OnChasedBy(this); // alert the target

        while (targetTransform != null &&
               targetTransform.gameObject != null &&
               Vector3.Distance(transform.position, targetTransform.position) > navMeshAgent.stoppingDistance)
        {

            if (targetTransform.gameObject.layer == LayerMask.NameToLayer("Protected"))
            {
                StopChase();
                yield break;
            }

            if (Time.time - startTime >= maxChaseTime)
            {
                StopChase();
                yield break;
            }

            navMeshAgent.SetDestination(targetTransform.position);
            yield return null;

            // ⚠️ 每帧都检查是否已销毁
            if (targetTransform == null || targetTransform.gameObject == null)
            {
                StopChase();
                yield break;
            }
        }

        // 再次检查是否还能访问目标
        if (targetTransform != null && targetTransform.gameObject != null &&
            targetTransform.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            isAttacking = true;
            animator.SetTrigger("Bite");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            damageable.ReceiveDamage(biteDamage, gameObject);
            isAttacking = false;
        }

        yield return new WaitForSeconds(biteCooldown);

        currentChaseTarget = null;
        HandleChaseState(); // Restart chasing
    }


    private void StopChase()
    {
        navMeshAgent.ResetPath();
        currentChaseTarget = null; // Reset the chase target
        SetState(AnimalState.Idle); // Set the state to idle
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    protected override void CheckNeed()
    {
        if (currentState == AnimalState.Chase || currentState == AnimalState.Breed || isRecovering)
            return;

        if (thirst <= 70)
        {
            Transform nearestWater = FindNearestWater();
            if (nearestWater != null)
            {
                if (IsDead) return;
                navMeshAgent.SetDestination(nearestWater.position);
                SetState(AnimalState.SeekWater);
                return;
            }
            else
            {
                //Debug.Log($"{gameObject.name} is thirsty but no water found.");
            }
        }

        if (hunger <= 50)
        {
            Collider[] colliders = new Collider[10];
            int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, colliders, bodyMask);

            Transform nearestBody = null;
            float shortestDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (colliders[i] == null) continue;

                float dist = Vector3.Distance(transform.position, colliders[i].transform.position);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    nearestBody = colliders[i].transform;
                }
            }

            if (nearestBody != null)
            {
                if (IsDead) return;
                navMeshAgent.SetDestination(nearestBody.position);
                SetState(AnimalState.SeekFood);
            }
            else
            {
                //Debug.Log($"{gameObject.name} is hungry but no body found.");
            }
        }
    }

    protected override void CheckFoodInRange()
    {
        //Debug.Log($"{name} is searching for Body...");

        Collider[] colliders = new Collider[10];
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, colliders,bodyMask);

        for (int i = 0; i < count; i++)
        {
            Transform target = colliders[i].transform;
            navMeshAgent.SetDestination(target.position);
            SetState(AnimalState.SeekFood);
            return;
        }

        //Debug.Log("No Body found.");
    }

    protected override IEnumerator RecoverHunger()
    {
      

        Collider[] colliders = new Collider[1];
        int count = Physics.OverlapSphereNonAlloc(transform.position, 1.5f, colliders, bodyMask);

        IEatable targetFood = null;

        for (int i = 0; i < count; i++)
        {
            if (colliders[i] != null && colliders[i].TryGetComponent(out IEatable eatable))
            {
                targetFood = eatable;
                break;
            }
        }

        int nutrition = 0;

        if (targetFood != null)
        {
            animator.SetTrigger("Eating");
            nutrition = targetFood.GetNutrition();
            targetFood.OnEaten(5f);
        }
        else
        {
            SetState(AnimalState.Idle);
        }

        while (hunger < 100 && nutrition > 0)
        {
            yield return new WaitForSeconds(1f);
            int consume = Mathf.Min(10, nutrition);
            hunger += consume;
            hunger = Mathf.Min(hunger, 100);
            nutrition -= consume;
        }
    }


}
