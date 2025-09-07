using System.Collections;
//using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class RhinoController: MonoBehaviour, IDamageable,IChasable,IAlert
{   
    public enum DeathCause
    {
        None,
        Hunger,
        Thirst,
        Damage
    }


    private DeathCause deathCause = DeathCause.None;
    private GameObject killer;

    [Header("XR Components")]
    public ActionBasedController controller;
    public XRRayInteractor rayInteractor;

    [Header("Input Actions")]
    public InputActionReference aButtonAction; // A键（Primary Button）
    public InputActionReference bButtonAction; // B键（Secondary Button）
    public InputActionReference yButtonAction; // Y键（Tertiary Button）

    [Header("Movement & Visuals")]
    public NavMeshAgent rhinoAgent;
    public LineRenderer laserLine;
    public Animator rhinoAnimator;

    [Header("Attributes")]
    [SerializeField] private int health = 10;
    [SerializeField] private float thirst = 100;
    [SerializeField] private float hunger = 100;
    [SerializeField] private float recoveryAmount = 100;
    [SerializeField] private float recoveryDuration = 2f;

    [Header("Audio Clips")]
    public AudioClip hungerDeathClip;
    public AudioClip thirstDeathClip;
    public AudioClip hunterKillClip;
    public AudioClip predatorKillClip;
    public AudioClip unknownKillClip;

    [Header("UI")]
    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public Slider handMenu_HealthSlider;
    public Slider handMenu_HungerSlider;
    public Slider handMenu_ThirstSlider;
    [SerializeField] private GameObject deathCanvas;


    [Header("Eat Settings")]
    [SerializeField] private LayerMask foodLayer;
    [SerializeField] private float eatRadius = 2f;

    [Header("Breeding")]
    public GameObject babyPrefab;
    public float breedRange = 5f;
    public LayerMask breedMask;
    public string breedTargetTag = "Rhino"; // 自己的Tag
    public float breedCooldown = 10f;
    public GameObject heartEffectPrefab;
    public bool canBreed = true;
    public bool isAdult = true;

    [Header("Visualization")]
    public LineRenderer pathLine;
    public GameObject destinationMarkerPrefab; // 终点标记预制体
    private GameObject currentMarker;
    private Vector3 lastMarkerPos = Vector3.positiveInfinity; // 初始用无穷大，保证第一次能更新

    [Header("Game End")]
    private bool isDead = false;
    public GameObject winCanvas;       // 胜利UI（默认在场景中设为不激活）
    public AudioClip winClip;          // 胜利音效（可选）
    private TimeManager timeMgr;       // 缓存 TimeManager，避免每帧 Find

    private AudioSource audioSource;
    private Coroutine currentActionCoroutine = null;
    private bool isInterrupted = false;


    //[Header("Death UI")]
    //public GameObject deathUIPanel;
    //public UnityEngine.UI.Text deathReasonText;

    private bool isRecovering = false;
    private bool isAutoNavigating = false;
    private string currentTargetTag = "";

    public WaterPoint wp;

    void Start()
    {
        if (controller == null)
        {
            controller = GetComponent<ActionBasedController>(); 
        }
        if (rayInteractor == null)
        {
            rayInteractor = GetComponent<XRRayInteractor>();
        }
        if (rhinoAgent == null)
        {
            rhinoAgent = GetComponent<NavMeshAgent>();
        }
        if (laserLine == null)
        {
            laserLine = GetComponent<LineRenderer>();
        }
        if (rhinoAnimator == null)
        {
            rhinoAnimator = GetComponent<Animator>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = 10; 
            healthSlider.value = health;
        }

        if (hungerSlider != null)
        {
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = 100;
            hungerSlider.value = hunger;
        }

        if (thirstSlider != null)
        {
            thirstSlider.minValue = 0;
            thirstSlider.maxValue = 100;
            thirstSlider.value = thirst;
        }

        if (handMenu_HealthSlider != null && handMenu_HungerSlider != null && handMenu_ThirstSlider != null)
        {
            handMenu_HealthSlider.minValue = 0;
            handMenu_HealthSlider.maxValue = 10;
            handMenu_HealthSlider.value = health;
            handMenu_HungerSlider.minValue = 0;
            handMenu_HungerSlider.maxValue = 100;
            handMenu_HungerSlider.value = hunger;
            handMenu_ThirstSlider.minValue = 0;
            handMenu_ThirstSlider.maxValue = 100;
            handMenu_ThirstSlider.value = thirst;
        }

        if (pathLine != null)
        {
            pathLine.positionCount = 0;
            pathLine.startWidth = 0.05f; // 起点宽度
            pathLine.endWidth = 0.05f;   // 终点宽度
        }

        StartCoroutine(ReduceThirstAndHunger());

        StartCoroutine(NeedsWarningLoop());
        //StartCoroutine(TryBreed());


    }
    void Update()
    {
        // health check
        if (isDead) return;
        if (health <= 0 && !isRecovering)
        {
            Die();
            return;
        }

        

        // update UI sliders
        if (healthSlider != null) healthSlider.value = health;
        if (hungerSlider != null) hungerSlider.value = hunger;
        if (thirstSlider != null) thirstSlider.value = thirst;

        if (handMenu_HealthSlider != null) handMenu_HealthSlider.value = health;
        if (handMenu_HungerSlider != null) handMenu_HungerSlider.value = hunger;
        if (handMenu_ThirstSlider != null) handMenu_ThirstSlider.value = thirst;




        //laser line visibility, move to target
        if (controller.activateAction.action.ReadValue<float>() > 0.1f)
        {
            InterruptCurrentAction(); // 打断一切当前行为

            Ray ray = new Ray(rayInteractor.transform.position, rayInteractor.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
                {
                    // ✅ 重要：启用 agent，确保未停用
                    rhinoAgent.enabled = true;

                    // ✅ 重置状态以允许新的导航
                    rhinoAgent.ResetPath();
                    rhinoAgent.isStopped = false;
                    rhinoAgent.velocity = Vector3.zero;

                    // ✅ 设置新目标
                    rhinoAgent.SetDestination(navHit.position);

                    // ✅ 清理自动导航状态，避免被 Update 拦截
                    currentTargetTag = "";
                    isAutoNavigating = false;

                    // ✅ 可视化射线
                    if (laserLine)
                    {
                        laserLine.SetPosition(0, ray.origin);
                        laserLine.SetPosition(1, navHit.position);
                    }

                    

                    //Debug.Log($"🚀 Trigger 移动到: {navHit.position}");
                }
            }
        }

        UpdatePathVisualization();
        // A for food, B for water (auto navigation)
        if (!isAutoNavigating && !isRecovering)
        {
            if (aButtonAction != null && aButtonAction.action.triggered)
            {
                InterruptCurrentAction();
                GameObject food = FindNearestWithTag("Food");
                if (food != null) MoveTo(food.transform.position, "Food");
            }

            if (bButtonAction != null && bButtonAction.action.triggered)
            {
                InterruptCurrentAction();
                GameObject water = FindNearestWithTag("Water");
                if (water != null) MoveTo(water.transform.position, "Water");
            }

            if (yButtonAction != null && yButtonAction.action.triggered && IsBreedUnlocked() && canBreed && isAdult)
            {
                StartCoroutine(TryBreed());
            }
            else if (yButtonAction != null && yButtonAction.action.triggered && !IsBreedUnlocked())
            {
                // 可选：给个提示
                ThreatSubtitle.Instance?.Show("Breeding unlocks on Day 3", new Color(1f, 0.8f, 0.2f, 1f));
            }
        }

        //auto navigation check
        if (isAutoNavigating && !rhinoAgent.pathPending && rhinoAgent.remainingDistance <= rhinoAgent.stoppingDistance)
        {
            if (currentMarker != null)
            {
                Destroy(currentMarker); // 到达后清除终点标记
            }

            if (currentTargetTag == "Food")
                currentActionCoroutine = StartCoroutine(Eat());
            else if (currentTargetTag == "Water")
                currentActionCoroutine = StartCoroutine(Drink());

            isAutoNavigating = false;
            currentTargetTag = "";
        }

        //animation update
        float speed = rhinoAgent.velocity.magnitude;
        rhinoAnimator.SetFloat("Speed", speed);
    }

    public void AlertByBuilding(GameObject building)
    {
        ThreatSubtitle.Instance?.Show("Leave Human area", new Color(1f, 0.2f, 0.2f, 1f)); // 红色
        VibrateController(0.5f, 0.3f); // 中等强度震动 0.3 秒
   
    }

    public void AlertByHunter(HunterManger hunter)
    {
        ThreatSubtitle.Instance?.Show("Hunter is shooting you,Run!!!", new Color(1f, 0.2f, 0.2f, 1f)); // 红色
        VibrateController(0.7f, 0.5f); // 强震动 0.5 秒
    }

    public Transform GetTransform() => transform;
    public void OnChasedBy(Predator predator)
    {
        if (!predator) return;
        ThreatSubtitle.Instance?.Show("Being chased, seeking protection from adult rhinos", new Color(1f, 0.2f, 0.2f, 1f)); // 红色
        Debug.Log($"{name} 玩家犀牛被 {predator.name} 盯上了！");
        VibrateController(0.8f, 0.4f); // 震动提醒
                                  
    }

    private void MoveTo(Vector3 target, string tag)
    {
        InterruptCurrentAction(); // 打断之前的行为

        if (NavMesh.SamplePosition(target, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
        {
     
            rhinoAgent.isStopped = false; // ✅ 恢复移动
            rhinoAgent.SetDestination(navHit.position);

            currentTargetTag = tag;
            isAutoNavigating = true;

            if (laserLine)
            {
                laserLine.SetPosition(0, rayInteractor.transform.position);
                laserLine.SetPosition(1, navHit.position);
            }

            Debug.Log("🚶 移动目标设为 " + tag + "，坐标：" + navHit.position);
        }
    }


    private GameObject FindNearestWithTag(string tag)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        if (targets.Length == 0) return null;

        GameObject nearest = null;
        float shortestDistance = float.MaxValue;
        Vector3 currentPos = transform.position;

        foreach (GameObject obj in targets)
        {
            float dist = Vector3.Distance(currentPos, obj.transform.position);
            if (dist < shortestDistance)
            {
                if(tag == "Water")
                    wp = obj.GetComponent<WaterPoint>();
                shortestDistance = dist;
                nearest = obj;
            }
        }

        return nearest;
    }

    private IEnumerator Eat()
    {
        isInterrupted = false;
        isRecovering = true;
        Debug.Log("尝试吃食物...");

        Collider[] hits = Physics.OverlapSphere(transform.position, eatRadius, foodLayer);
        IEatable nearestFood = null;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IEatable eatable))
            {
                nearestFood = eatable;
                break;
            }
        }

        if (nearestFood != null)
        {
            rhinoAnimator.SetTrigger("Eat");
            int nutrition = nearestFood.GetNutrition();
            Debug.Log($"找到食物，营养值: {nutrition}");

            yield return new WaitForSeconds(recoveryDuration);

            if (isInterrupted)
            {
                Debug.Log("🍴 吃食物被打断，停止执行");
                isRecovering = false;
                yield break;
            }

            hunger = Mathf.Min(hunger + nutrition, 100);
            nearestFood.OnEaten();
            Debug.Log($"吃完后饥饿值: {hunger}");
        }
        else
        {
            ThreatSubtitle.Instance?.Show("No food nearby", new Color(1f, 0.8f, 0.2f, 1f));
        }

        isRecovering = false;
    }

    private IEnumerator Drink()
    {
        isInterrupted = false;
        isRecovering = true;
        rhinoAnimator.SetTrigger("Eat");
        Debug.Log("Drinking...");

        yield return new WaitForSeconds(recoveryDuration);

        if (isInterrupted)
        {
            Debug.Log("💧 喝水被打断，停止执行");
            isRecovering = false;
            yield break;
        }
        if (wp.isPolluted)
        {   
            deathCause = DeathCause.Damage;
            ReceiveDamage(1,null);
            
        }
        thirst = Mathf.Min(thirst + recoveryAmount, 100);
        Debug.Log("Recovered thirst: " + thirst);
        isRecovering = false;
    }

    private IEnumerator ReduceThirstAndHunger()
    {
        float hungerTimer = 0f;

        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (isRecovering) continue;

            thirst = Mathf.Max(0, thirst - 1);
            hungerTimer += 1f;

            if (hungerTimer >= 2f)
            {
                hunger = Mathf.Max(0, hunger - 1);
                hungerTimer = 0f;
            }

            if (health > 0 && (thirst == 0 || hunger == 0))
            {
                if (thirst == 0)
                    deathCause = DeathCause.Thirst;
                else if (hunger == 0)
                    deathCause = DeathCause.Hunger;

                health -= 1; //decrease health if thirst or hunger is zero
            }

            //Debug.Log($"Thirst: {thirst}, Hunger: {hunger}");
        }
    }

    private IEnumerator NeedsWarningLoop()
    {
        var wait = new WaitForSeconds(5f);

        while (true)
        {
            yield return wait;

            if (isDead) continue;            // 已死亡不提示
            if (isRecovering) continue;      // 正在吃/喝时不打扰

            // 口渴提示
            if (thirst <= 40)
            {
                // 你要求使用 ThreatSubtitle.Instance?.Show("Leave Human area", ...)，这里把颜色改成黄色
                ThreatSubtitle.Instance?.Show("You are thirsty, press B to drink water", new Color(1f, 0.85f, 0.2f, 1f));
            }

            // 饥饿提示
            if (hunger <= 40)
            {
                ThreatSubtitle.Instance?.Show("You are hungry, press A to find food", new Color(1f, 0.85f, 0.2f, 1f));
            }
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        string killerName = killer != null ? killer.name : "Unknown";


        Debug.Log($"{name} 死亡！原因：{deathCause}，击杀者：{killerName}");
        //rhinoAgent.isStopped = true;
        rhinoAnimator.SetTrigger("Die"); //death animation trigger
        gameObject.layer = LayerMask.NameToLayer("Body");
    

        PlayDeathAudio();
        DisableBabyRhinoScript();

        if (deathCanvas != null)
        {
            deathCanvas.SetActive(true);
        }

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            //agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.enabled = false;
        }

        //destroy UI
        // Destroy(gameObject, 3f);
        //Time.timeScale = 0f;  // ⏸️ 暂停所有基于 Time.time 的操作
    }

    private void PlayDeathAudio()
    {
        if (audioSource == null) return;

        switch (deathCause)
        {
            case DeathCause.Hunger:
                VoiceManager.Instance.PlayVoice(hungerDeathClip, VoicePriority.Death);
                break;
            case DeathCause.Thirst:
                VoiceManager.Instance.PlayVoice(thirstDeathClip, VoicePriority.Death);
                break;
            case DeathCause.Damage:
                if (killer != null)
                {
                    //  Hunter or Predator kill
                    if (killer.TryGetComponent<HunterManger>(out _))
                    {
                        Debug.Log("kill by hunter");
                        VoiceManager.Instance.PlayVoice(hunterKillClip, VoicePriority.Death);
                    }
                    else if (killer.TryGetComponent<Predator>(out _))
                    {
                        Debug.Log("kill by predator");
                        VoiceManager.Instance.PlayVoice(predatorKillClip, VoicePriority.Death);
                    }
                    else
                    {
                        Debug.Log("unknow kill");
                        VoiceManager.Instance.PlayVoice(unknownKillClip, VoicePriority.Death);
                    }
                }
                else
                {
                    Debug.Log("unknow kill");
                    VoiceManager.Instance.PlayVoice(unknownKillClip, VoicePriority.Death);
                }
                break;
        }
    }

    public void ReceiveDamage(int damage, GameObject source)
    {
        if (gameObject.layer == LayerMask.NameToLayer("Protected"))
            return;

        health -= damage;
        Debug.Log($"{name} received {damage} damage from {source?.name}. Remaining health: {health}");

        if (health <= 0)
        {
            deathCause = DeathCause.Damage;
            killer = source; //whoe caused the kill


            Die();
        }
    }

    private void DisableBabyRhinoScript()
    {
        BabyRhino babyRhino = GetComponent<BabyRhino>();
        if (babyRhino != null)
        {
            babyRhino.enabled = false;
            Debug.Log("已禁用 BabyRhino 脚本，停止成长逻辑。");
        }
    }

    private void InterruptCurrentAction()
    {
        isInterrupted = true;

        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
            Debug.Log("⚠️ 当前协程行为已被打断");
        }

        if (rhinoAgent != null)
        {
            if (rhinoAgent.hasPath || rhinoAgent.pathPending)
            {
                rhinoAgent.ResetPath();
                Debug.Log("🛑 Rhino导航已重置");
            }

            rhinoAgent.isStopped = true;
            rhinoAgent.velocity = Vector3.zero;
        }

        isAutoNavigating = false;
        isRecovering = false;
        currentTargetTag = ""; // ✅ 重要：否则 Update 会继续触发 Eat/Drink
    }


    private void UpdatePathVisualization()
    {
        if (pathLine == null || rhinoAgent == null)
            return;

        // 1) 没有路径 / 角点为 0：清空线 + 清理 marker
        if (!rhinoAgent.hasPath || rhinoAgent.path.corners == null || rhinoAgent.path.corners.Length == 0)
        {
            pathLine.positionCount = 0;
            DestroyMarkerIfExists();
            return;
        }

        // 2) 画线（抬高一点防穿地）
        var path = rhinoAgent.path;
        var corners = path.corners;

        pathLine.positionCount = corners.Length;
        for (int i = 0; i < corners.Length; i++)
            pathLine.SetPosition(i, corners[i] + Vector3.up * 0.05f);

        // 3) 终点 marker：只在不存在时创建，之后只移动，不销毁重建
        Vector3 endPos = corners[corners.Length - 1];
        if (currentMarker == null && destinationMarkerPrefab != null)
        {
            currentMarker = Instantiate(destinationMarkerPrefab, endPos, Quaternion.identity);
            lastMarkerPos = Vector3.positiveInfinity; // 确保下一步会更新位置
        }

        // 只在位置有明显变化时，更新 marker 的位置（避免每帧改 transform 也导致某些特效重启）
        if (currentMarker != null && (endPos - lastMarkerPos).sqrMagnitude > 0.01f)
        {
            currentMarker.transform.position = endPos;
            lastMarkerPos = endPos;
        }

        // 4) 如果已经到达目标：清空线 + 清理 marker
        if (!rhinoAgent.pathPending && rhinoAgent.remainingDistance <= rhinoAgent.stoppingDistance)
        {
            pathLine.positionCount = 0;
            DestroyMarkerIfExists();
        }
    }

    private void DestroyMarkerIfExists()
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null;
        }
    }

    private bool IsBreedUnlocked()
    {
        // 没有 TimeManager 的情况下，默认解锁（避免因丢失组件卡死）
        if (timeMgr == null) return true;
        return timeMgr.GetCurrentDay() >= 3;
    }

    private IEnumerator TryBreed()
    {
        Debug.Log("开始尝试繁殖");

        Collider[] hits = Physics.OverlapSphere(transform.position, breedRange, breedMask);
        Transform partner = null;

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue; // 忽略自己
            if (hit.CompareTag("Rhino")) // ✅ 只判断 Tag
            {
                partner = hit.transform;
                break;
            }
        }

        if (partner != null)
        {
            
            rhinoAgent.stoppingDistance = 1.5f;

            // 等待靠近
            while (Vector3.Distance(transform.position, partner.position) > 2f)
            {
                rhinoAgent.SetDestination(partner.position);
                yield return null;
            }

            // 播放爱心特效
            if (heartEffectPrefab != null)
                Instantiate(heartEffectPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);

            yield return new WaitForSeconds(2f);

            // 生成宝宝
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-1f, 1f), 0.3f, Random.Range(-1f, 1f));
            Instantiate(babyPrefab, spawnPos, Quaternion.identity);

            Debug.Log($"{name} 与 {partner.name} 成功繁殖出宝宝");

            canBreed = false;
            //Invoke(nameof(ResetBreed), breedCooldown);

            EndGame();
        }
        else
        {
            ThreatSubtitle.Instance?.Show("No Rhino beside you", new Color(1f, 0.8f, 0.2f, 1f));
        }
    }

    private bool gameEnding = false;

    private void EndGame()
    {
        if (gameEnding) return;
        StartCoroutine(EndGameRoutine());
    }
    private IEnumerator EndGameRoutine()
    {
        gameEnding = true;

        // 停止玩家可控的移动/输入，但暂时不暂停 Time
        if (rhinoAgent != null)
        {
            rhinoAgent.isStopped = true;
            rhinoAgent.ResetPath();
            rhinoAgent.velocity = Vector3.zero;
        }

        // 显示胜利 UI
        if (winCanvas != null) winCanvas.SetActive(true);

        // 播放胜利音效（不打断环境 BGM）
        if (winClip != null)
            VoiceManager.Instance?.PlayVoice(winClip, VoicePriority.Normal);

        // 等 5 秒（用 Realtime，不受 timeScale 影响）
        yield return new WaitForSecondsRealtime(5f);

        // 暂停游戏
        Time.timeScale = 0f;

        Debug.Log("🎉 Game Over - You Win! (paused after 5s)");
    }
    private void ResetBreed()
    {
        canBreed = true;
        Debug.Log($"{name} 的繁殖冷却已重置");
    }

    public void VibrateController(float amplitude, float duration)
    {
        if (controller != null)
        {
            controller.SendHapticImpulse(amplitude, duration);
        }
    }
}


