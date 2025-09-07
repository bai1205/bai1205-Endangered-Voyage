using System.Collections;
using UnityEngine;
using UnityEngine.AI;

//using UnityEngine.AI;

public enum HunterState { Idle, Moving, Chase,Leave }

public class HunterManger : MonoBehaviour
{
    [Header("Wander")]
    [SerializeField] private float wanderDistance = 50f;
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float maxWalkTime = 40f;

    [Header("Idle")]
    [SerializeField] private float idleTime = 3f;

    [Header("Chase")]
    [SerializeField] private float runSpeed = 2f;

    [Header("Shooting")]
    [SerializeField] private float shootingTime = 2f;
    [SerializeField] private float shootingRange = 10f;
    [SerializeField] private AudioClip shootClip;
    private AudioSource audioSource;

    [Header("Hunter Variables")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float maxChaseTime = 50f;
    [SerializeField] private int damage = 10;

    [SerializeField] private LayerMask preyMask;
    [SerializeField] private LayerMask predatorMask;
    [SerializeField] private LayerMask rhinoMask;

    [Header("Leave Settings")]
    [SerializeField] private Transform exitPoint;   // 离开目的地
    [SerializeField] private float leaveDelay = 300f; // 300 秒后离开


    private NavMeshAgent navMeshAgent;
    private Actions actions;
    private HunterState currentState = HunterState.Idle;
    private Coroutine stateCoroutine;
    private Transform currentChaseTarget;
    private int combinedMask;

    // 可识别的目标类型
    private System.Type[] allowedTargets = new System.Type[]
    {
        typeof(Prey),
        typeof(Predator),
        typeof(RhinoController),       
    };

    private void Start()
    {
        Debug.Log($"Hunter {gameObject.name} initialized with detection range: {detectionRange}, shooting range: {shootingRange}, damage: {damage}");
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = walkSpeed;

        actions = transform.GetChild(0).GetChild(0).GetComponent<Actions>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        combinedMask = preyMask | predatorMask| rhinoMask;

        currentState = HunterState.Idle;
        UpdateState();
        StartCoroutine(LeaveAfterDelay());
    }


    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case HunterState.Idle:
                StartCoroutine(IdleRoutine());
                break;
            case HunterState.Moving:
                StartCoroutine(MoveRoutine());
                break;
            case HunterState.Chase:
                StartCoroutine(ChaseRoutine());
                break;          
            case HunterState.Leave: // ✅ 新增 Leave 状态
                StartCoroutine(LeaveRoutine());
                break;

        }
    }
    protected void SetState(HunterState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;
        OnStateChanged(newState);
        Debug.Log($"{gameObject.name} changed state to {newState}");
    }

    protected virtual void OnStateChanged(HunterState newState)
    {
        if (newState == HunterState.Idle) 
        {
            actions.Stay();
        }

        if (newState == HunterState.Moving)
        {
            actions.Walk();
            navMeshAgent.stoppingDistance = 0.1f;
            navMeshAgent.speed = walkSpeed;
            Debug.Log($"{gameObject.name} is now moving");
        }

        if (newState == HunterState.Chase)
        {
            actions.Run();
            navMeshAgent.speed = runSpeed;
            navMeshAgent.stoppingDistance = 1f; // Set a stopping distance for chase
            Debug.Log($"{gameObject.name} is now chasing");
        }

        if (newState == HunterState.Leave)
        {
            actions.Walk();
            navMeshAgent.speed = walkSpeed;
            navMeshAgent.stoppingDistance = 0.1f;
            Debug.Log($"{gameObject.name} is leaving the scene");
        }

        UpdateState();
    }

    private IEnumerator IdleRoutine()
    {
        yield return new WaitForSeconds(Random.Range(idleTime / 2, idleTime * 2));

        Vector3 destination = GetRandomNavMeshPosition(transform.position, wanderDistance);
        navMeshAgent.SetDestination(destination);

        //Debug.Log($"Hunter {gameObject.name} is moving to {destination}");
        SetState(HunterState.Moving);
    }

    private IEnumerator MoveRoutine()
    {
        float deadline = Time.time + maxWalkTime;

        while (true)
        {
            if (Time.time >= deadline)
            {
                navMeshAgent.ResetPath();
                break;
            }

            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= 0.2f)
                break;

            if (TryFindTarget(out Transform target))
            {
                StartChase(target);
                yield break;
            }

            yield return null;
        }
        SetState(HunterState.Idle);
    }

    private IEnumerator ChaseRoutine()
    {
        if (currentChaseTarget == null) { SetState(HunterState.Idle); yield break; }

        float startTime = Time.time;

        while (currentChaseTarget != null &&
               currentChaseTarget.gameObject.activeInHierarchy &&
               Vector3.SqrMagnitude(transform.position - currentChaseTarget.position) > shootingRange * shootingRange)
        {
            if (Time.time - startTime >= maxChaseTime)
            {
                StopChase();
                yield break;
            }

            navMeshAgent.SetDestination(currentChaseTarget.position);
            yield return null;
        }

        if (currentChaseTarget != null && currentChaseTarget.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            navMeshAgent.isStopped = true;

            Vector3 dir = (currentChaseTarget.position - transform.position).normalized;
            dir.y = 0; // 保持水平旋转
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = lookRot;

            actions.Aiming();
            yield return new WaitForSeconds(shootingTime);

            actions.Attack();
            if (shootClip != null && audioSource != null)
            {
                Debug.Log($"{gameObject.name} shooting at {currentChaseTarget.name}");
                audioSource.PlayOneShot(shootClip);
            }
            else
            {
                Debug.LogWarning("Shoot clip or audio source is not set!");
            }

            if (Random.value <= 0.3f)
                damageable.ReceiveDamage(damage, gameObject);

            if (currentChaseTarget.TryGetComponent<IAlert>(out IAlert alertable))
            {
                alertable.AlertByHunter(this);
            }
                
            navMeshAgent.isStopped = false;
        }

        StopChase();
    }

    private bool TryFindTarget(out Transform target)
    {
        target = null;
        Collider[] colliders = new Collider[20];
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, colliders, combinedMask);

        float nearestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = colliders[i];
            if (col == null) continue;

            foreach (var type in allowedTargets)
            {
                if (col.GetComponent(type) != null)
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        target = col.transform;
                    }
                }
            }
        }
        return target != null;
    }

    private void StartChase(Transform target)
    {
        currentChaseTarget = target;
        SetState(HunterState.Chase);
    }

    private void StopChase()
    {
        navMeshAgent.ResetPath();
        currentChaseTarget = null;
        SetState(HunterState.Idle);
    }

    private Vector3 GetRandomNavMeshPosition(Vector3 origin, float distance)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * distance + origin;
            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, distance, NavMesh.AllAreas))
                return hit.position;
        }
        return origin;
    }

    private IEnumerator LeaveAfterDelay()
    {
        yield return new WaitForSeconds(leaveDelay);
        if (exitPoint != null)
        {
            Debug.Log($"{gameObject.name} 开始离开，目标点：{exitPoint.position}");
            SetState(HunterState.Leave);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 没有设置 exitPoint，无法执行离开动作！");
        }
    }

    private IEnumerator LeaveRoutine()
    {
        if (exitPoint == null)
        {
            Debug.LogWarning("LeaveRoutine 被调用，但 exitPoint 没有设置！");
            yield break;
        }

        navMeshAgent.SetDestination(exitPoint.position);

        while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        Debug.Log($"{gameObject.name} 到达出口并消失");
        Destroy(gameObject); // ✅ 到达后销毁
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}
