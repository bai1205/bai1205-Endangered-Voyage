using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public float dayLengthInSeconds = 30f;
    private float timer = 0f;

    public static event System.Action OnBreedDay;

    [Header("Time Display")]
    public int currentDay = 1;
    public Text dayText;

    [Header("Lighting")]
    public Light directionalLight;
    public Gradient lightColorOverDay;

    [Header("Skybox")]
    public Material daySkybox;
    public Material nightSkybox;

    private bool isNight = false;
    private bool isNightDay = false;

    [Header("Start Time")]
    public int startHour = 8;
    private const int totalMinutesInDay = 1440;

    [Header("Human Building")]
    public BuildingSpawner builder;
    private bool isTrue = true;

    [Header("Voice_Phase")]
    public AudioClip Phase_1;
    public AudioClip Phase_2;
    public AudioClip Phase_3;
    private AudioSource audioSource;
    private bool waitingToPlayVoice = false;

    [Header("Hunter Spawning")]
    public GameObject hunterPrefab;  // 猎人预制体
    public Transform[] hunterSpawnPoints; // 猎人生成点

    [Header("Expanding Area Day 2")]
    public GameObject[] expandingAreas; // 拖入驱赶源物体（初始设为Inactive）

    [Header("Forest BGM")]
    public AudioSource bgmSource;       // 独立的BGM音源
    public AudioClip dayBGM;
    public AudioClip nightBGM;

    [Header("Mission UI")]
    public TMP_Text missionText; // 拖进 Inspector

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Voice_Phase(currentDay);
        PlayBGM(isNightDay ? nightBGM : dayBGM);
    }
    void Update()
    {
        if (isTrue){
          
            isTrue = false;
        }

        //OnBreedDay.Invoke(); // For test

        timer += Time.deltaTime;

        float normalizedTime = timer / dayLengthInSeconds;
        int gameMinutes = Mathf.FloorToInt(normalizedTime * totalMinutesInDay);
        int totalMinutes = (startHour * 60 + gameMinutes) % totalMinutesInDay;

        int hour = totalMinutes / 60;
        int minute = totalMinutes % 60;

        string timeFormatted = hour.ToString("D2") + ":" + minute.ToString("D2");
        //Debug.Log("Day " + currentDay + " - Time: " + timeFormatted);

        if (dayText != null)
        {
            dayText.text = "Day " + currentDay;
        }

        RotateSun((float)totalMinutes / totalMinutesInDay);

        UpdateSkybox(hour);
        
        // 检查白天夜晚切换并切BGM
        bool shouldBeNight = (hour >= 19 || hour < 6);
        if (shouldBeNight != isNightDay)
        {
            isNightDay = shouldBeNight;
            PlayBGM(isNightDay ? nightBGM : dayBGM);
        }

       
        UpdateMissionText();

        if (timer >= dayLengthInSeconds)
        {
            timer = 0f;
            currentDay++;
            waitingToPlayVoice = true; // 等待在08:00播放语音
            if (currentDay == 2 && OnBreedDay != null)
            {
                Debug.Log("Broadcasting Breed Event on Day 2");
                BuildHumanBuilding();
                OnBreedDay.Invoke();

                SpawnHunters(2);
                Debug.Log("Day 2: 生成 2 个猎人");

                EnableExpandingAreas();

                var wm = FindObjectOfType<WaterManager>();
                if (wm != null)
                {
                    wm.StartPollutionSequence();
                }

            }
            else if (currentDay == 3)
            {
                int count = Random.Range(3, 5); // 3-5 个
                Debug.Log($"Day 3: 生成 {count} 个猎人");
                SpawnHunters(count);
            }


            Debug.Log("New day started: Day " + currentDay);
        }
        if (waitingToPlayVoice && hour == 8 && minute == 0)
        {
            Voice_Phase(currentDay);
            waitingToPlayVoice = false;
        }

    }


    private void UpdateMissionText()
    {
        if (missionText == null) return;
        string dayInfo = $"Day {currentDay}";

        switch (currentDay)
        {
            case 1:
                missionText.text = $"{dayInfo} - Mission: Survive"; ;
                break;
            case 2:
                missionText.text = $"{dayInfo} -Mission: Avoid hunters";
                break;
            case 3:
                missionText.text = $"{dayInfo} -Mission: Get a baby";
                break;
            default:
                missionText.text = $"{dayInfo} - Mission: Survive!";
                break;
        }
    }
    void RotateSun(float dayPercent)
    {
        if (directionalLight != null)
        {
            float angle = dayPercent * 360f - 90f;
            directionalLight.transform.rotation = Quaternion.Euler(new Vector3(angle, 170f, 0f));

            if (lightColorOverDay != null)
            {
                directionalLight.color = lightColorOverDay.Evaluate(dayPercent);
            }
        }
    }

    void UpdateSkybox(int hour)
    {
        bool shouldBeNight = (hour >= 19 || hour < 6);

        if (shouldBeNight != isNight)
        {
            isNight = shouldBeNight;

            if (isNight && nightSkybox != null)
            {
                RenderSettings.skybox = nightSkybox;
                Debug.Log("Switched to night skybox");
            }
            else if (!isNight && daySkybox != null)
            {
                RenderSettings.skybox = daySkybox;
                Debug.Log("Switched to day skybox");
            }
        }
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }

    public void BuildHumanBuilding(){
        builder.Spawn();
    }
    #region Phase Voice
    public void Voice_Phase(int currentday)
    {
        switch (currentday)
        {
            case 1:
                audioSource.clip = Phase_1;
                VoiceManager.Instance.PlayVoice(Phase_1, VoicePriority.Normal);
                break;
            case 2:
                audioSource.clip = Phase_2;
                VoiceManager.Instance.PlayVoice(Phase_2, VoicePriority.Normal);
                break;
            case 3:
                audioSource.clip = Phase_3;
                VoiceManager.Instance.PlayVoice(Phase_3, VoicePriority.Normal);
                break;
            default:
                Debug.Log("now it is four");
                break;
        }
    }
    #endregion

    #region Hunter Spawning
    private void SpawnHunters(int count)
    {
        if (hunterPrefab == null || hunterSpawnPoints.Length == 0)
        {
            Debug.LogWarning("Hunter prefab 或 spawn points 未设置！");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint = hunterSpawnPoints[Random.Range(0, hunterSpawnPoints.Length)];
            Instantiate(hunterPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
    #endregion


    private void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) 
        {
            Debug.Log("bgmSource or clip is null");
            return;
        }
        if (bgmSource.clip == clip && bgmSource.isPlaying) 
        {
            Debug.Log("bgm is playing");
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }


    private void EnableExpandingAreas()
    {
        foreach (var area in expandingAreas)
        {
            if (area != null)
            {
                area.SetActive(true);
                Debug.Log($"驱赶源启用：{area.name}");
            }
        }
    }
}


