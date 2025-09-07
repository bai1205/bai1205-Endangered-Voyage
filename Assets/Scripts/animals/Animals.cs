using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum AnimalState
{
    Idle,
    Moving,
    Chase,
    SeekWater,
    SeekFood,
    Breed,
    Die,
}



[RequireComponent(typeof(NavMeshAgent))]

public class Animals : MonoBehaviour, IDamageable
{
    [Header("Wander")]
    [SerializeField] private float wanderDistance = 50f;
    [SerializeField] public float walkSpeed = 5f;
    [SerializeField] private float maxWalkTime = 6f;
    [SerializeField] private float escapeMaxDistance = 80f;

    [Header("Idle")]
    [SerializeField] private float idleTime = 3f;

    [Header("Chase")]
    [SerializeField] private float runSpeed = 8f;
    protected bool isAttacking = false;

    [Header("Seeking")]
    [SerializeField] private float seekSpeed = 3f;

    [Header("Attributes")]
    [SerializeField] private int health = 10;
    [SerializeField] protected float thirst = 100;
    [SerializeField] protected float hunger = 100;

    [Header("Breed")]
    [SerializeField] public GameObject babyPrefab;
    [SerializeField] public float BreedRange = 5f;
    [SerializeField] public LayerMask breedMask;
    [SerializeField] public string targetTag = "WildBoar";
    [SerializeField] public float breedCooldown = 10f;
    private Collider[] breedHits = new Collider[10];
    public bool canBreed = true;
    public bool isAdult = true;

    [Header("Detection Tags")]
    [SerializeField] private string foodTag = "Food";
    [SerializeField] private string waterTag = "Water";
    [SerializeField] protected float detectionRadius = 50f;

    [Header("VFX")]
    [SerializeField] private GameObject heartEffectPrefab;


    protected NavMeshAgent navMeshAgent;
    protected Animator animator;
    [SerializeField] protected AnimalState currentState = AnimalState.Idle;
    protected Coroutine threatRoutine;

    private Coroutine needsCoroutine;
    protected bool isRecovering = false;
    private bool isArrived = false;

    protected bool _isDead = false;
    protected bool IsDead => _isDead || currentState == AnimalState.Die;

    public WaterPoint targetWaterPoint;
    //test building
    //public  BuildingSpawner builder;
    public WaterManager wm;

    // Start is called before the first frame update
    private void Start()
    {
        InitialiseAnimal();
        needsCoroutine = StartCoroutine(ReduceThirstAndHunger());
        
    }

    // Update is called once per frame
    protected virtual void InitialiseAnimal()
    {
        animator = transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = walkSpeed;
        currentState = AnimalState.Idle;
        UpdateState();
    }


    #region States
    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case AnimalState.Idle:
                HandleIdleState();
                break;
            case AnimalState.Moving:
                HandleMovingState();
                break;
            case AnimalState.Chase:
                HandleChaseState();
                break;
            case AnimalState.SeekWater:
                HandleSeekWaterState();
                break;
            case AnimalState.SeekFood:
                HandleSeekFoodState();
                break;
            case AnimalState.Breed:
                HandleBreedState(); 
                break;
            case AnimalState.Die:
                HandleDieState();
                break;

        }
    }
    protected void SetState(AnimalState newState)
    {
        if (IsDead && newState != AnimalState.Die)
            return;

        if (currentState == newState)
            return;

        currentState = newState;
        OnStateChanged(newState);
        //Debug.Log($"{gameObject.name} changed state to {newState}");
    }

    protected virtual void OnStateChanged(AnimalState newState)
    {
        animator?.CrossFadeInFixedTime(newState.ToString(), 0.5f);

        if (newState == AnimalState.Idle) 
        {
            navMeshAgent.stoppingDistance = 0.1f;
            navMeshAgent.speed = walkSpeed;
        }

        if (newState == AnimalState.Moving)
        {
            navMeshAgent.stoppingDistance = 0.1f;
            navMeshAgent.speed = walkSpeed;
            //Debug.Log($"{gameObject.name} is now moving");
        }
            
        if (newState == AnimalState.Chase)
        {
            navMeshAgent.speed = runSpeed;
            navMeshAgent.stoppingDistance = 1f; // Set a stopping distance for chase
            //Debug.Log($"{gameObject.name} is now chasing");
        }
            
        if (newState == AnimalState.SeekWater || newState == AnimalState.SeekFood)
        {
            navMeshAgent.speed = seekSpeed;
            navMeshAgent.stoppingDistance = 0.1f;
            //Debug.Log($"{gameObject.name} is now seeking {newState}");
        }

        if (newState == AnimalState.Die)
        {
            HandleDieState();
            return;
        }

        //if (isAttacking && newState != AnimalState.Die) return;

        UpdateState();
    }
    #endregion


    protected Vector3 GetRandomNavMeshPosition(Vector3 origin, float distance)
    {
        for(int i = 0; i < 5; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * distance;
            randomDirection += origin;
            NavMeshHit navMeshHit;

            if (NavMesh.SamplePosition(randomDirection, out navMeshHit, distance, NavMesh.AllAreas))
            {
                 Vector3 finalPosition = navMeshHit.position;

                // 向地圖中心靠近 1f
                Vector3 directionToCenter = (new Vector3(50f,0f,50f) - finalPosition).normalized;
                finalPosition += directionToCenter * 1f;

                if (NavMesh.SamplePosition(finalPosition, out navMeshHit, 1f, NavMesh.AllAreas))
            {
                return navMeshHit.position;
            }
            }
        }

        return origin; // If no valid position found, return the original position
    }


    protected virtual void CheckChaseConditions()
    {

    }

    protected virtual void CheckFoodConditions()
    {

    }

    protected virtual void HandleChaseState()
    {
        
    }

    protected virtual void HandleIdleState()
    {
        if (IsDead) return;
        CheckNeed();
        StartCoroutine(WaitTomove());
    }

    private IEnumerator WaitTomove()
    {
        if (IsDead) yield break;

        float waitTime = Random.Range(idleTime / 2, idleTime * 2);
        yield return new WaitForSeconds(waitTime);

        Vector3 randomDestination = GetRandomNavMeshPosition(transform.position, wanderDistance);
      

        navMeshAgent.SetDestination(randomDestination);
        SetState(AnimalState.Moving);
    }

    protected virtual void HandleMovingState()
    {
        if (IsDead) return;

        CheckNeed();
        StartCoroutine(WaitToReachDestination());
    }

    private IEnumerator WaitToReachDestination()
    {
        if (IsDead) yield break;
        const float DEST_EPS_SQR = 0.04f;               
        float deadline = Time.time + maxWalkTime;       // deadline calculated in advance

        while (navMeshAgent.isActiveAndEnabled)
        {
            if (IsDead) yield break;
            //overtime check
            if (currentState != AnimalState.SeekWater &&  currentState != AnimalState.SeekFood && currentState != AnimalState.Breed)
            {
                if (Time.time >= deadline)
                {
                    isArrived = false;
                    //Debug.Log($"{name} walk timed-out, resetting path.");
                    navMeshAgent.ResetPath();
                    break;
                }
            }
            

            // check if the path is valid
            if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                isArrived = false;
                //Debug.LogWarning($"{name} has invalid path.");
                navMeshAgent.ResetPath();
                break;
            }

            // arrived at destination check
            if (!navMeshAgent.pathPending &&
                navMeshAgent.remainingDistance * navMeshAgent.remainingDistance <= DEST_EPS_SQR)
            {
                isArrived = true;
                break;
            }

            if (hunger <= 70) 
            { 
                CheckChaseConditions();
            }
            
            //CheckFoodConditions();
            yield return null;
        }

        if (IsDead) yield break;

        Debug.Log("isArrived :"+isArrived);
        if (currentState == AnimalState.SeekWater ||  currentState == AnimalState.SeekFood && isArrived){
            //Debug.Log(123);
            isArrived = false;
            StartCoroutine(Recover());

        } 
        else
        {
            SetState(AnimalState.Idle);
        }
        
    }

    public virtual void ReceiveDamage(int damage, GameObject source)
    {
        if (IsDead) return;

        if (gameObject.layer == LayerMask.NameToLayer("Protected")) 
        { 
            Debug.Log($"{gameObject.name} is protected and cannot take damage.");
            return;
        }
            
        //Debug.Log($"{gameObject.name} received {damage} damage. Remaining health: {health - damage}");
        health -= damage;

        if (health <= 0)
        {
            SetState(AnimalState.Die);
        }
           
    }

    #region Die State
    protected virtual void HandleDieState()
    {
        Die();
    }

    protected virtual void Die()
    {
        if (_isDead) return;
        _isDead = true;
        // Handle death logic here, such as playing an animation, disabling the animal, etc.
        StopAllCoroutines();

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            //agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        SetState(AnimalState.Die);

        gameObject.layer = LayerMask.NameToLayer("Body");

        //Destroy(gameObject);

    }
    #endregion

    private IEnumerator ReduceThirstAndHunger(){
        float hungerTimer = 0f;

        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (health <= 0)
            {
                SetState(AnimalState.Die);
                yield break; // 停止 Coroutine
            }

            if (isRecovering)
                continue;

            // 每秒扣 thirst
            thirst = Mathf.Max(0, thirst - 1);
            //Debug.Log("Thirst: " + thirst);

            // 每 5 秒才扣 hunger
            hungerTimer += 1f;
            if (hungerTimer >= 2f)
            {
                hunger = Mathf.Max(0, hunger - 1);
                //Debug.Log("Hunger: " + hunger);
                hungerTimer = 0f;
            }

            if (thirst == 0 || hunger == 0)
            {
                ReceiveDamage(1,null); // 如果 thirst 或 hunger 為 0，則扣除 1 點生命值
            }
        }
    }

    protected virtual void HandleSeekWaterState(){
        StartCoroutine(WaitToReachDestination());
    }

    protected virtual void CheckNeed()
    {
        if (currentState == AnimalState.Chase || currentState == AnimalState.Breed)
            return;

        if (thirst <= 50)
        {
            Transform nearestWater = FindNearestWater();
            //wm.PolluteWaterByID(1);
            if (nearestWater != null)
            {
                if (IsDead) return;
                navMeshAgent.SetDestination(nearestWater.position);
                SetState(AnimalState.SeekWater);
                return;
            }
            else
            {
                Debug.Log($"{gameObject.name} is thirsty but no water found.");
            }
        }

        if (hunger <= 50)
        {
            Transform nearestFood = FindNearestFood();
            if (nearestFood != null)
            {
                if (IsDead) return;
                navMeshAgent.SetDestination(nearestFood.position);
                SetState(AnimalState.SeekFood);
                return;
            }   
            else
            {
                Debug.Log($"{gameObject.name} is hungry but no food found.");
            }
        }
    }


    private Collider[] detectionBuffer = new Collider[10];//Find size of detection buffer

    protected Transform FindNearestFood()
    {
        GameObject[] foods = GameObject.FindGameObjectsWithTag(foodTag);
        if (foods.Length == 0)
            return null;

        Transform nearest = null;
        float shortestDistance = float.MaxValue;

        foreach (GameObject food in foods)
        {
            float distance = Vector3.Distance(transform.position, food.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = food.transform;
            }
        }

        return nearest;
    }
    protected Transform FindNearestWater()
    {
        GameObject[] waters = GameObject.FindGameObjectsWithTag(waterTag);
        if (waters.Length == 0)
            return null;

        Transform nearest = null;
        float shortestDistance = float.MaxValue;

        foreach (GameObject water in waters)
        {
            WaterPoint wp = water.GetComponent<WaterPoint>();

            float distance = Vector3.Distance(transform.position, water.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearest = water.transform;
            }
        }

        return nearest;
    }


    private IEnumerator Recover()
    {
        isRecovering = true;

        switch (currentState)
        {
            case AnimalState.SeekWater:
                yield return StartCoroutine(RecoverThirst());
                break;

            case AnimalState.SeekFood:
                yield return StartCoroutine(RecoverHunger());
                break;
        }

        SetState(AnimalState.Idle);
        isRecovering = false;
    }


    protected virtual IEnumerator RecoverHunger()
    {

        Collider[] colliders = new Collider[1];
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, colliders, LayerMask.GetMask("Food"));

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
            StartCoroutine(DestroyFoodAfterDelay(targetFood as MonoBehaviour, 2f));
            //targetFood.OnEaten(5f);
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

    private IEnumerator DestroyFoodAfterDelay(MonoBehaviour foodObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (foodObj != null)
            Destroy(foodObj.gameObject);
    }


    protected virtual IEnumerator RecoverThirst()
    {
        animator.SetTrigger("Drinking");

        var wp = targetWaterPoint;

        if (wp == null)
        {
            var t = FindNearestWater();
            if (t && t.TryGetComponent(out WaterPoint found))
                wp = found;
        }

        while (thirst < 100)
        {
            if(wp != null && wp.isPolluted)
            {
                ReceiveDamage(1,null);
            }
            yield return new WaitForSeconds(5f);
            thirst += 5;
            thirst = Mathf.Min(thirst, 100);
        }
    }

    protected virtual void HandleSeekFoodState(){
        StartCoroutine(WaitToReachDestination());
    }

    protected virtual void CheckFoodInRange()
    {
        // 空的，讓子類 override 自己的搜尋邏輯
    }

    public void AlertFrom(Transform threat, float range, string sourceName)
    {
        if (IsDead) return;
        SetState(AnimalState.Chase);
        if (threatRoutine != null)
            StopCoroutine(threatRoutine);

        threatRoutine = StartCoroutine(RunFromThreat(threat, range));
        Debug.Log($"{name} alerted by {sourceName}: {threat.name}");
    }



    private IEnumerator RunFromThreat(Transform threat, float range)
    {
        if (IsDead || (threat == null)) yield break;

        float escapeStartTime = Time.time;
        //float maxEscapeTime = 20f; // 最多逃 10 秒

        // 等待威脅接近
        while (threat != null && Vector3.Distance(transform.position, threat.position) > range)
            yield return null;

        // 逃跑中
        while (!IsDead && threat != null && Vector3.Distance(transform.position, threat.position) <= range)
        {
            RunAwayFromThreat(threat);
            yield return null;
        }

        if (IsDead) yield break;
        // 等待最後移動完成
        float waitStart = Time.time;
        while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            
            yield return null;
        }

        
        //Debug.Log($"{name} escape complete wait timed out.11");
        SetState(AnimalState.Idle);
        threatRoutine = null;
    }

    private void RunAwayFromThreat(Transform threat)
    {
        if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled) return;

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
        {
            Vector3 runDirection = (transform.position - threat.position).normalized;
            Vector3 escapeDestination = transform.position + runDirection * (escapeMaxDistance * 2);

            if (Vector3.Distance(navMeshAgent.destination, escapeDestination) > 1f) // 防止重複設置
            {
                navMeshAgent.SetDestination(GetRandomNavMeshPosition(escapeDestination, escapeMaxDistance));
            }
        }
    }

    #region BreedState
    protected virtual void HandleBreedState()
    {
        if (!canBreed || !isAdult)
        {
            SetState(AnimalState.Idle);
            return;
        }

        StartCoroutine(TryBreed());
    }

    private IEnumerator TryBreed()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, BreedRange, breedHits, breedMask);

        Animals targetMate = null;

        for (int i = 0; i < count; i++)
        {
            Collider hit = breedHits[i];
            

            if (hit.gameObject == gameObject) continue;

            if (hit.CompareTag(targetTag))
            {
                if (hit.TryGetComponent(out Animals partner))
                {
                    //Debug.Log($"检测 partner - canBreed: {partner.canBreed}, isAdult: {partner.isAdult}");

                    if (partner.canBreed && partner.isAdult)
                    {
                        targetMate = partner;
                        //Debug.Log($"✅ 找到配偶：{partner.name}"); 
                        break;
                    }
                    else
                    {
                        //Debug.Log($"配偶 {partner.name} 不符合繁殖条件 (canBreed: {partner.canBreed}, isAdult: {partner.isAdult})");
                    }
                }
                else
                {
                    //Debug.LogWarning($"未找到 Animals 组件在 {hit.name}");
                }
            }
        }

        if (targetMate != null)
        {
            navMeshAgent.SetDestination(targetMate.transform.position);
            
            Debug.Log($"{name} 正在靠近 {targetMate.name} 进行繁殖");
            navMeshAgent.stoppingDistance = 1.5f;

            // 等待靠近目标
            while (Vector3.Distance(transform.position, targetMate.transform.position) > 2f)
            {
                yield return null;

                // 确保目标还在
                if (targetMate == null)
                {
                    SetState(AnimalState.Idle);
                    yield break;
                }
            }

            // 到达后进行繁殖
            Debug.Log($"{name} 与 {targetMate.name} 成功靠近并开始繁殖");

            SpawnHeartEffect(transform.position);

            yield return new WaitForSeconds(3f);

            SpawnBaby();

            canBreed = false;
            targetMate.canBreed = false;
            Invoke(nameof(ResetBreedCooldown), breedCooldown);
            targetMate.Invoke(nameof(targetMate.ResetBreedCooldown), breedCooldown);
        }

        yield return new WaitForSeconds(1f);
        SetState(AnimalState.Idle);
    }

    private void SpawnHeartEffect(Vector3 position)
    {
        if (heartEffectPrefab == null) return;

        Vector3 effectPos = position + new Vector3(0, 1.5f, 0); // 偏移到头顶
        Instantiate(heartEffectPrefab, effectPos, Quaternion.identity);
    }
    private void ResetBreedCooldown()
    {
        canBreed = true;
    }

    private void SpawnBaby()
    {
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-1f, 1f), 0.3f, Random.Range(-1f, 1f));
        GameObject baby = Instantiate(babyPrefab, spawnPos, Quaternion.identity);
        Debug.Log("生成新宝宝：" + baby.name);

        if (baby.TryGetComponent(out BabyPrey babyScript))
        {
            babyScript.SetParent(this); // 设置母体引用
        }

    }

    private void OnEnable()
    {
        TimeManager.OnBreedDay += TriggerBreedFromTime;
    }   

    private void OnDisable()
    {
        TimeManager.OnBreedDay -= TriggerBreedFromTime;
    }

    private void TriggerBreedFromTime()
    {
        if (IsDead) return;
        if (canBreed && isAdult && currentState != AnimalState.Die)
        {
            Debug.Log($"{name} 响应时间广播，进入繁殖状态");
            SetState(AnimalState.Breed);
        }
    }
    #endregion

}


