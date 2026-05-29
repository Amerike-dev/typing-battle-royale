using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Muestra el panel de Instrucciones tras pulsar "Empezar" y corre un contador de
/// <see cref="duration"/> segundos antes de que el host transicione a la GameplayScene.
///
/// El texto del contador es blanco y se vuelve amarillo cuando quedan <= <see cref="yellowThreshold"/>
/// segundos, con un pequeño "pop" de escala cada vez que cambia el número.
///
/// Debe vivir en un objeto SIEMPRE ACTIVO (p. ej. el Canvas); el panel en sí puede empezar oculto.
/// </summary>
public class InstructionsPanelController : MonoBehaviour
{
    public static InstructionsPanelController Instance { get; private set; }

    [Header("Referencias")]
    [Tooltip("Panel de instrucciones (Canvas/Instrucciones). Empieza oculto y se muestra al iniciar.")]
    [SerializeField] private GameObject panel;
    [Tooltip("CanvasGroup del panel. Si está, su alpha pasa a 1 al mostrar (0 al ocultar). Si se deja vacío se busca en el panel.")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("Texto del contador. Si se deja vacío se crea uno automáticamente dentro del panel.")]
    [SerializeField] private TMP_Text timerText;
    [Tooltip("Fuente del contador (Gontserrat-Bold). Si se deja vacío intenta tomarla de un texto del panel.")]
    [SerializeField] private TMP_FontAsset timerFont;

    [Header("Contador")]
    [SerializeField] private float duration = 20f;
    [Tooltip("A partir de este valor (inclusive) el texto se pone amarillo.")]
    [SerializeField] private int yellowThreshold = 5;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;

    [Header("Animación (pop de escala por segundo)")]
    [SerializeField] private float popScale = 1.25f;
    [SerializeField] private float popDuration = 0.18f;

    private Coroutine _countdownRoutine;
    private Coroutine _popRoutine;
    private Vector3 _timerBaseScale = Vector3.one;

    public float Duration => duration;

    /// <summary>Se invoca (en cada cliente) cuando el contador llega a 0.</summary>
    public event Action OnCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        if (panel == null) panel = AutoFindPanel();
        if (canvasGroup == null && panel != null) canvasGroup = panel.GetComponent<CanvasGroup>();

        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Begin() => Begin(duration);

    public void Begin(float seconds)
    {
        SetVisible(true);
        EnsureTimerText();

        if (_countdownRoutine != null) StopCoroutine(_countdownRoutine);
        _countdownRoutine = StartCoroutine(CountdownRoutine(seconds));
    }

    /// <summary>
    /// Muestra/oculta el panel. Si hay CanvasGroup controla alpha + interactable + blocksRaycasts
    /// (manteniendo el GameObject activo para que corran las corrutinas); si no, hace SetActive.
    /// </summary>
    private void SetVisible(bool visible)
    {
        if (panel == null) return;

        if (canvasGroup != null)
        {
            // El panel queda activo siempre; la visibilidad la maneja el CanvasGroup.
            panel.SetActive(true);
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
        else
        {
            panel.SetActive(visible);
        }
    }

    private IEnumerator CountdownRoutine(float seconds)
    {
        int remaining = Mathf.Max(0, Mathf.CeilToInt(seconds));
        ShowNumber(remaining);

        while (remaining > 0)
        {
            yield return new WaitForSecondsRealtime(1f);
            remaining--;
            ShowNumber(remaining);
        }

        _countdownRoutine = null;
        OnCompleted?.Invoke();
    }

    private void ShowNumber(int remaining)
    {
        if (timerText == null) return;
        timerText.text = remaining.ToString();
        timerText.color = remaining <= yellowThreshold ? warningColor : normalColor;
        PlayPop();
    }

    private void PlayPop()
    {
        if (timerText == null || !isActiveAndEnabled) return;
        if (_popRoutine != null) StopCoroutine(_popRoutine);
        _popRoutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        Transform t = timerText.transform;
        float half = Mathf.Max(0.0001f, popDuration * 0.5f);
        Vector3 big = _timerBaseScale * popScale;

        float e = 0f;
        while (e < half) { e += Time.unscaledDeltaTime; t.localScale = Vector3.Lerp(_timerBaseScale, big, e / half); yield return null; }
        e = 0f;
        while (e < half) { e += Time.unscaledDeltaTime; t.localScale = Vector3.Lerp(big, _timerBaseScale, e / half); yield return null; }

        t.localScale = _timerBaseScale;
        _popRoutine = null;
    }

    private void EnsureTimerText()
    {
        if (timerText != null) { _timerBaseScale = timerText.transform.localScale; return; }
        if (panel == null) return;

        // Resolvemos la fuente ANTES de crear el texto (para no auto-detectarnos a nosotros mismos).
        TMP_FontAsset font = ResolveFont();

        var go = new GameObject("TimerText", typeof(RectTransform));
        go.transform.SetParent(panel.transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 60f);
        rt.sizeDelta = new Vector2(300f, 140f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = false;
        tmp.fontSize = 96f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = normalColor;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;

        timerText = tmp;
        _timerBaseScale = go.transform.localScale;
    }

    private TMP_FontAsset ResolveFont()
    {
        if (timerFont != null) return timerFont;
        if (panel != null)
        {
            var anyText = panel.GetComponentInChildren<TMP_Text>(true);
            if (anyText != null && anyText.font != null) return anyText.font;
        }
        return null;
    }

    private GameObject AutoFindPanel()
    {
        var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c == null) continue;
            var t = c.transform.Find("Instrucciones");
            if (t != null) return t.gameObject;
        }
        return null;
    }
}
