using System.Collections;
using UnityEngine;
using TMPro;

public class ThreatSubtitle : MonoBehaviour
{
    public static ThreatSubtitle Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI subtitleText; // �� ThreatSubtitleText
    [SerializeField] private CanvasGroup canvasGroup;       // �� CanvasGroup����Textͬ�����ϼ��ɣ�

    [Header("Defaults")]
    [SerializeField] private float defaultFadeIn = 0.12f;
    [SerializeField] private float defaultHold = 1.4f;
    [SerializeField] private float defaultFadeOut = 0.25f;
    [SerializeField] private Color defaultColor = new Color(1, 0.2f, 0.2f, 1f); // ƫ��

    private Coroutine playCo;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!subtitleText) subtitleText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (!canvasGroup) canvasGroup = GetComponentInChildren<CanvasGroup>(true);

        if (canvasGroup) canvasGroup.alpha = 0f;
    }

    public void Show(string message, Color? color = null, float? fadeIn = null, float? hold = null, float? fadeOut = null)
    {
        if (!subtitleText || !canvasGroup) return;

        // �����һ��
        if (playCo != null) StopCoroutine(playCo);

        subtitleText.text = message;
        subtitleText.color = color ?? defaultColor;

        playCo = StartCoroutine(PlayRoutine(
            fadeIn ?? defaultFadeIn,
            hold ?? defaultHold,
            fadeOut ?? defaultFadeOut
        ));
    }

    private IEnumerator PlayRoutine(float fadeIn, float hold, float fadeOut)
    {
        // ����
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeIn);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // ͣ��
        yield return new WaitForSeconds(hold);

        // ����
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOut);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        playCo = null;
    }
}
