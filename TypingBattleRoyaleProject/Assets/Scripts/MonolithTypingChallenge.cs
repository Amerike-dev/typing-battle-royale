using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class MonolithTypingChallenge : MonoBehaviour
{
    public static MonolithTypingChallenge Instance;

    public Canvas myCanvas;
    [SerializeField] private TextMeshProUGUI runeDisplay;
    [SerializeField] private TMP_InputField hiddenInput;
    [SerializeField] private MonolithUIController uiController;

    [Header("Feedback de desbloqueo (solo jugador local)")]
    [Tooltip("Icono de éxito (card_outline_place): el hechizo se agregó al inventario.")]
    [SerializeField] private Sprite successIcon;
    [Tooltip("Icono de fallo (card_remove): el desbloqueo falló.")]
    [SerializeField] private Sprite failIcon;
    [Tooltip("Fuente de los textos de feedback (Gontserrat Bold).")]
    [SerializeField] private TMP_FontAsset feedbackFont;
    [SerializeField] private float feedbackIconSize = 170f;
    [SerializeField] private float feedbackFontSize = 42f;
    [Tooltip("Segundos que el feedback permanece visible antes de desvanecerse.")]
    [SerializeField] private float feedbackHoldSeconds = 1.1f;

    private Canvas _feedbackCanvas;
    private Coroutine _feedbackRoutine;

    private MonolithController _monolith;
    private Spell _spell;
    private int _spellIndex;
    private PlayerController _player;

    private TypingOverlay _typingOverlay;

    private void Awake() => Instance = this;

    private TypingOverlay GetOverlay()
    {
        if (_typingOverlay == null)
            _typingOverlay = new GameObject("MonolithTypingOverlay").AddComponent<TypingOverlay>();
        return _typingOverlay;
    }

    /// <summary>
    /// Apaga los gráficos viejos del canvas del monolito (panel, "Concentrate", texto) excepto el
    /// InputField oculto, que sigue capturando. El overlay de teclas los reemplaza, y como el fondo
    /// ahora es translúcido ya no los tapa.
    /// </summary>
    private void HideOldMonolithVisuals()
    {
        if (myCanvas == null) return;
        foreach (var g in myCanvas.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
        {
            if (hiddenInput != null && g.transform.IsChildOf(hiddenInput.transform)) continue;
            g.enabled = false;
        }
    }

    private void Start()
    {
        if (myCanvas != null) myCanvas.enabled = false;
    }

    public void Begin(MonolithController monolith, Spell spell, int index, PlayerController player)
    {
        _monolith = monolith;
        _spell = spell;
        _spellIndex = index;
        _player = player;

        _player.NullMoveSpeed();
        // Bloqueamos el giro de cámara y el cursor mientras se tipea (se restaura en Close).
        if (_player.cameraController != null) _player.cameraController.OnCamaraMove = false;
        CursorManager.HideCursor();

        hiddenInput.text = "";
        hiddenInput.onValueChanged.RemoveAllListeners();
        hiddenInput.onValueChanged.AddListener(OnType);

        // El InputField sigue capturando (oculto); el typeo se ve en el overlay de teclas.
        myCanvas.enabled = true;
        HideOldMonolithVisuals(); // ocultamos panel/"Concentrate"/texto viejos (el overlay los reemplaza)
        hiddenInput.ActivateInputField();
        hiddenInput.Select();

        // Monolito: fondo translúcido (50%) y SIN panel de texto crudo (tolerancia cero al error).
        GetOverlay().Show("Escribe y desbloquea el hechizo", _spell.runeString, TypingOverlay.ElementColor(_spell.elementType), 0.5f, false);
    }

    private void OnType(string typed)
    {
        bool hasError = false;
        int currentIndex = typed.Length;
        
        for (int i = 0; i < typed.Length; i++)
        {
            if (i >= _spell.runeString.Length || typed[i] != _spell.runeString[i])
            {
                hasError = true;
                break;
            }
        }
        
        GetOverlay().UpdateProgress(hasError ? typed.Length - 1 : typed.Length, hasError);

        if (hasError)
        {
            FailChallenge(byTypo: true);
            return;
        }

        if (typed.Length == _spell.runeString.Length)
        {
            Invoke(nameof(WinChallenge), 0.05f);
        }
    }

    /// <param name="byTypo">true = falló tipeando (penaliza con cooldown + feedback); false = canceló con ESC (sin penalización).</param>
    private void FailChallenge(bool byTypo)
    {
        Debug.Log(byTypo ? "¡Fallaste el tipeo!" : "Desafío cancelado.");
        Close();

        if (byTypo)
        {
            ShowSpellFeedback(false);
            // Cooldown individual: no puede reintentar ESTE monolito por unos segundos.
            if (MonolithLevelSelectUI.Instance != null && _monolith != null)
                MonolithLevelSelectUI.Instance.RegisterFailCooldown(_monolith);
        }
    }

    private void WinChallenge()
    {
        hiddenInput.onValueChanged.RemoveAllListeners();

        Debug.Log("¡Hechizo conseguido!");
        Close();
        ShowSpellFeedback(true);
        _player.ClaimMonolithSpellServerRpc(_monolith.NetworkObjectId, _spellIndex, _spell.spellName);
    }

    private void Close()
    {
        myCanvas.enabled = false;
        if (_typingOverlay != null) _typingOverlay.Hide();
        _player.MoveSpeed();
        // Restauramos el movimiento de cámara (se desactivó al abrir la UI del monolito).
        if (_player != null && _player.cameraController != null) _player.cameraController.OnCamaraMove = true;
        CursorManager.HideCursor();

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void Update()
    {
        if (myCanvas.enabled && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            FailChallenge(byTypo: false);
        }
    }

    // ---------------- Feedback de desbloqueo (per-player) ----------------

    /// <summary>
    /// Muestra una animación local (solo para este jugador) según el resultado del desbloqueo:
    /// éxito = icono baja de arriba a abajo + "¡Hechizo agregado!"; fallo = icono aparece y
    /// se sacude/rota + "¡Fallaste, intenta de nuevo!".
    /// </summary>
    private void ShowSpellFeedback(bool success)
    {
        Sprite icon = success ? successIcon : failIcon;
        string message = success ? "¡Hechizo agregado!" : "¡Fallaste, intenta de nuevo!";

        if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
        _feedbackRoutine = StartCoroutine(FeedbackRoutine(success, icon, message));
    }

    private Transform EnsureFeedbackCanvas()
    {
        if (_feedbackCanvas != null) return _feedbackCanvas.transform;

        GameObject go = new GameObject("SpellFeedbackCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _feedbackCanvas = go.GetComponent<Canvas>();
        _feedbackCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _feedbackCanvas.sortingOrder = 50; // por encima del HUD y la UI del monolito

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return _feedbackCanvas.transform;
    }

    private IEnumerator FeedbackRoutine(bool success, Sprite icon, string message)
    {
        Transform canvasTransform = EnsureFeedbackCanvas();

        // Contenedor centrado.
        GameObject containerGo = new GameObject(success ? "Feedback_Success" : "Feedback_Fail",
            typeof(RectTransform), typeof(CanvasGroup));
        RectTransform root = containerGo.GetComponent<RectTransform>();
        root.SetParent(canvasTransform, false);
        root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = new Vector2(0f, 80f);
        CanvasGroup cg = containerGo.GetComponent<CanvasGroup>();
        cg.alpha = 0f;

        // Icono.
        GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.SetParent(root, false);
        iconRt.anchorMin = iconRt.anchorMax = iconRt.pivot = new Vector2(0.5f, 0.5f);
        iconRt.sizeDelta = new Vector2(feedbackIconSize, feedbackIconSize);
        Image img = iconGo.GetComponent<Image>();
        img.sprite = icon;
        img.preserveAspect = true;
        img.raycastTarget = false;

        // Texto debajo del icono.
        GameObject txtGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.SetParent(root, false);
        txtRt.anchorMin = txtRt.anchorMax = txtRt.pivot = new Vector2(0.5f, 0.5f);
        txtRt.sizeDelta = new Vector2(700f, feedbackFontSize * 1.6f);
        txtRt.anchoredPosition = new Vector2(0f, -(feedbackIconSize * 0.5f) - feedbackFontSize);
        TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = message;
        if (feedbackFont != null) tmp.font = feedbackFont;
        tmp.fontSize = feedbackFontSize;
        tmp.color = success ? new Color(0.55f, 1f, 0.65f) : new Color(1f, 0.45f, 0.45f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        // --- Animación de entrada ---
        float intro = 0.45f;
        float restY = 0f;
        float dropFrom = 220f;   // éxito: el icono baja desde arriba
        float t = 0f;
        while (t < intro)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / intro);
            cg.alpha = k;

            if (success)
            {
                // De arriba hacia abajo con ease-out.
                float eased = 1f - (1f - k) * (1f - k);
                iconRt.anchoredPosition = new Vector2(0f, Mathf.Lerp(dropFrom, restY, eased));
            }
            else
            {
                // Sacudida/rotación de error que se va amortiguando.
                float angle = Mathf.Sin(k * Mathf.PI * 6f) * 22f * (1f - k);
                iconRt.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
            yield return null;
        }

        cg.alpha = 1f;
        iconRt.anchoredPosition = new Vector2(0f, restY);
        iconRt.localRotation = Quaternion.identity;

        // --- Permanece visible ---
        yield return new WaitForSecondsRealtime(feedbackHoldSeconds);

        // --- Desvanecido ---
        float outro = 0.35f;
        t = 0f;
        while (t < outro)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = 1f - Mathf.Clamp01(t / outro);
            yield return null;
        }

        Destroy(containerGo);
        _feedbackRoutine = null;
    }
}
