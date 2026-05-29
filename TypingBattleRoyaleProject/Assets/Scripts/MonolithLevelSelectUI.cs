using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class MonolithLevelSelectUI : MonoBehaviour
{
    public static MonolithLevelSelectUI Instance;
    public Canvas myCanvas;
    [SerializeField] private MonolithSpellButton[] spellButtons;

    [Header("Instrucciones (overlay esquina superior izquierda)")]
    [Tooltip("Sprite ws_0 (de ws.png, multiple).")]
    [SerializeField] private Sprite selectSpellIcon;
    [Tooltip("Sprite enterGroup.")]
    [SerializeField] private Sprite acceptIcon;
    [Tooltip("Fuente de los textos de instrucciones (Gontserrat Regular).")]
    [SerializeField] private TMP_FontAsset instructionsFont;
    [SerializeField] private float wsIconHeight = 200f;
    [SerializeField] private float acceptIconHeight = 110f;
    [SerializeField] private float instructionsFontSize = 30f;
    [Tooltip("Offset desde la esquina superior izquierda del canvas (x hacia la derecha, y hacia abajo si es negativo).")]
    [SerializeField] private Vector2 instructionsPadding = new Vector2(30f, -30f);
    [SerializeField] private float instructionsRowGap = 24f;
    [SerializeField] private float instructionsIconTextGap = 18f;

    [Header("Fondo opaco (atenúa el glow del monolito y el HUD)")]
    [Tooltip("Color/opacidad del panel de fondo que cubre toda la pantalla detrás de la UI.")]
    [SerializeField] private Color dimColor = new Color(0f, 0f, 0f, 0.5f);

    [Header("Cooldown por fallo (individual, por monolito)")]
    [Tooltip("Segundos que un jugador debe esperar para reintentar un monolito tras fallar el tipeo.")]
    [SerializeField] private float failCooldownSeconds = 15f;
    [Tooltip("Sprite del aviso de recarga (wpm.png).")]
    [SerializeField] private Sprite reloadIcon;
    [Tooltip("Segundos que se muestra el aviso 'recargando' por cada vez que el jugador presiona E.")]
    [SerializeField] private float cooldownNoticeDuration = 2.5f;
    [Tooltip("Fuente del aviso de recarga (Gontserrat Bold).")]
    [SerializeField] private TMP_FontAsset cooldownFont;

    private RectTransform _instructionsRoot;

    // Cooldown LOCAL por monolito (key = NetworkObjectId). Solo afecta a este jugador.
    private readonly Dictionary<ulong, float> _failCooldownUntil = new Dictionary<ulong, float>();
    private CanvasGroup _cooldownGroup;
    private TMP_Text _cooldownText;
    private Coroutine _cooldownRoutine;

    private PlayerController _localPlayer;
    private int _selectedIndex;

    private void Awake() => Instance = this;

    private void Start()
    {
        if (myCanvas != null) myCanvas.enabled = false;

        if (spellButtons != null)
        {
            foreach (var spellButton in spellButtons)
            {
                if (spellButton == null) continue;
                var btn = spellButton.GetComponent<Button>();
                if (btn == null) continue;
                var colors = btn.colors;
                colors.selectedColor = colors.highlightedColor;
                btn.colors = colors;
            }
        }
    }

    void Update()
    {
        if (MonolithTypingChallenge.Instance != null && MonolithTypingChallenge.Instance.myCanvas.enabled) return;

        if (!myCanvas.enabled) return;

        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Hide();
            return;
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame) MoveSelection(-1);
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame) MoveSelection(1);

        if (Keyboard.current.enterKey.wasPressedThisFrame) ConfirmSelection();
    }

    private void MoveSelection(int delta)
    {
        if (spellButtons == null || spellButtons.Length == 0) return;
        int n = spellButtons.Length;
        int next = _selectedIndex;
        for (int step = 0; step < n; step++)
        {
            next = (next + delta + n) % n;
            if (IsButtonSelectable(next))
            {
                _selectedIndex = next;
                ApplyFocus();
                return;
            }
        }
    }

    private bool IsButtonSelectable(int i)
    {
        if (i < 0 || i >= spellButtons.Length) return false;
        if (spellButtons[i] == null || !spellButtons[i].gameObject.activeInHierarchy) return false;
        var btn = spellButtons[i].GetComponent<Button>();
        return btn != null && btn.interactable;
    }

    private void ApplyFocus()
    {
        if (EventSystem.current == null) return;
        if (!IsButtonSelectable(_selectedIndex)) return;
        EventSystem.current.SetSelectedGameObject(spellButtons[_selectedIndex].gameObject);
    }

    private void ConfirmSelection()
    {
        if (!IsButtonSelectable(_selectedIndex)) return;
        var btn = spellButtons[_selectedIndex].GetComponent<Button>();
        if (btn != null) btn.onClick.Invoke();
    }

    public void Show(MonolithController monolith, PlayerController player)
    {
        _localPlayer = player;
        _localPlayer.NullMoveSpeed();

        // Bloqueamos el movimiento de cámara mientras la UI está abierta (igual que en pausa).
        if (_localPlayer.cameraController != null) _localPlayer.cameraController.OnCamaraMove = false;

        EnsureInstructionsOverlay();

        int count = Mathf.Min(monolith.syncedSpellNames.Count, spellButtons.Length);
    
        for (int i = 0; i < spellButtons.Length; i++)
        {
            if (i < count) 
            {
                string nameToFind = monolith.syncedSpellNames[i].ToString().Trim('\0');
                Spell spell = monolith.allSpells.FirstOrDefault(s => s != null && s.spellName == nameToFind);
            
                if (spell != null)
                {
                    spellButtons[i].gameObject.SetActive(true);
                    bool isClaimedByOthers = monolith.syncedSpellClaimed[i];
                    bool alreadyInInventory = _localPlayer.inventory != null && _localPlayer.inventory.HasSpell(nameToFind);

                    int buttonState = 0;
                    if (alreadyInInventory) buttonState = 2;
                    else if (isClaimedByOthers) buttonState = 1;
                    
                    int indexToPass = i;
                    spellButtons[i].Setup(spell, buttonState, () => SelectSpell(monolith, spell, indexToPass));
                }
                else
                {
                    spellButtons[i].Clear();
                }
            }
            else 
            {
                spellButtons[i].Clear();
            }
        }

        CursorManager.ShowCursor();
        myCanvas.enabled = true;

        _selectedIndex = -1;
        for (int i = 0; i < spellButtons.Length; i++)
        {
            if (IsButtonSelectable(i))
            {
                _selectedIndex = i;
                break;
            }
        }
        if (_selectedIndex < 0) _selectedIndex = 0;
        ApplyFocus();
    }
    
    private void SelectSpell(MonolithController monolith, Spell spell, int index)
    {
        myCanvas.enabled = false;
        MonolithTypingChallenge.Instance.Begin(monolith, spell, index, _localPlayer);
    }

    private void Hide()
    {
        myCanvas.enabled = false;
        CursorManager.HideCursor();

        if (_localPlayer != null)
        {
            _localPlayer.MoveSpeed();
            // Restauramos el movimiento de cámara al cerrar la UI.
            if (_localPlayer.cameraController != null) _localPlayer.cameraController.OnCamaraMove = true;
        }
    }

    /// <summary>
    /// Crea (una sola vez) el overlay de instrucciones en la esquina superior izquierda:
    /// fila 1 = icono ws_0 + "Seleccionar Hechizo"; fila 2 = icono enterGroup + "Aceptar".
    /// Como es hijo de myCanvas, se muestra/oculta junto con la UI del monolito.
    /// </summary>
    private void EnsureInstructionsOverlay()
    {
        if (_instructionsRoot != null) return;
        if (myCanvas == null) return;

        // --- Panel de fondo opaco: cubre toda la pantalla DETRÁS del resto de la UI ---
        GameObject dim = new GameObject("BackgroundDim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform dimRt = dim.GetComponent<RectTransform>();
        dimRt.SetParent(myCanvas.transform, false);
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;
        dimRt.localScale = Vector3.one;
        Image dimImg = dim.GetComponent<Image>();
        dimImg.color = dimColor;
        dimImg.raycastTarget = true; // actúa de backdrop modal
        dimRt.SetAsFirstSibling(); // detrás de los botones y del overlay

        // --- Overlay de instrucciones (esquina superior izquierda) ---
        GameObject root = new GameObject("InstructionsOverlay", typeof(RectTransform));
        _instructionsRoot = root.GetComponent<RectTransform>();
        _instructionsRoot.SetParent(myCanvas.transform, false);
        _instructionsRoot.anchorMin = new Vector2(0f, 1f);
        _instructionsRoot.anchorMax = new Vector2(0f, 1f);
        _instructionsRoot.pivot = new Vector2(0f, 1f);
        _instructionsRoot.anchoredPosition = instructionsPadding;
        _instructionsRoot.localScale = Vector3.one;

        // Columna de texto común para alinear ambas etiquetas: usamos el icono MÁS ANCHO de los dos
        // para que ningún texto se solape con su icono.
        float wsWidth = IconWidth(selectSpellIcon, wsIconHeight);
        float acceptWidth = IconWidth(acceptIcon, acceptIconHeight);
        float textColumnX = Mathf.Max(wsWidth, acceptWidth) + instructionsIconTextGap;

        BuildInstructionRow("Row_Select", selectSpellIcon, wsIconHeight, "Seleccionar Hechizo", 0f, textColumnX);
        BuildInstructionRow("Row_Accept", acceptIcon, acceptIconHeight, "Aceptar", -(wsIconHeight + instructionsRowGap), textColumnX);
    }

    // ---------------- Cooldown por fallo (individual) ----------------

    /// <summary>Registra el cooldown local para este monolito tras un fallo de tipeo del jugador.</summary>
    public void RegisterFailCooldown(MonolithController monolith)
    {
        if (monolith == null) return;
        _failCooldownUntil[monolith.NetworkObjectId] = Time.time + failCooldownSeconds;
    }

    /// <summary>Segundos restantes de cooldown para este monolito (0 si ya puede reintentar).</summary>
    public float GetRemainingCooldown(MonolithController monolith)
    {
        if (monolith == null) return 0f;
        if (_failCooldownUntil.TryGetValue(monolith.NetworkObjectId, out float until))
            return Mathf.Max(0f, until - Time.time);
        return 0f;
    }

    /// <summary>Muestra (solo localmente) el aviso "Monolito recargando..." con la cuenta regresiva.</summary>
    public void ShowCooldownNotice(MonolithController monolith)
    {
        if (monolith == null) return;
        EnsureCooldownUI();
        if (_cooldownRoutine != null) StopCoroutine(_cooldownRoutine);
        _cooldownRoutine = StartCoroutine(CooldownNoticeRoutine(monolith));
    }

    private IEnumerator CooldownNoticeRoutine(MonolithController monolith)
    {
        _cooldownGroup.gameObject.SetActive(true);
        _cooldownGroup.alpha = 1f;

        float shownFor = 0f;
        float remaining = GetRemainingCooldown(monolith);
        while (remaining > 0f && shownFor < cooldownNoticeDuration)
        {
            int secs = Mathf.CeilToInt(remaining);
            _cooldownText.text = $"Monolito recargando...\nRegresa en {secs} segundos";
            shownFor += Time.deltaTime;
            yield return null;
            remaining = GetRemainingCooldown(monolith);
        }

        float t = 0f;
        const float fade = 0.25f;
        float startAlpha = _cooldownGroup.alpha;
        while (t < fade)
        {
            t += Time.deltaTime;
            _cooldownGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fade);
            yield return null;
        }

        _cooldownGroup.alpha = 0f;
        _cooldownGroup.gameObject.SetActive(false);
        _cooldownRoutine = null;
    }

    private void EnsureCooldownUI()
    {
        if (_cooldownGroup != null) return;

        GameObject canvasGo = new GameObject("MonolithCooldownCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("CooldownNotice", typeof(RectTransform), typeof(CanvasGroup));
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.SetParent(canvas.transform, false);
        panelRt.anchorMin = panelRt.anchorMax = panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = new Vector2(0f, 120f);
        _cooldownGroup = panel.GetComponent<CanvasGroup>();

        float iconH = 140f;
        float iconW = IconWidth(reloadIcon, iconH);
        GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.SetParent(panelRt, false);
        iconRt.anchorMin = iconRt.anchorMax = iconRt.pivot = new Vector2(0.5f, 0.5f);
        iconRt.sizeDelta = new Vector2(iconW, iconH);
        iconRt.anchoredPosition = new Vector2(0f, iconH * 0.5f + 20f);
        Image img = iconGo.GetComponent<Image>();
        img.sprite = reloadIcon;
        img.preserveAspect = true;
        img.raycastTarget = false;

        GameObject txtGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.SetParent(panelRt, false);
        txtRt.anchorMin = txtRt.anchorMax = txtRt.pivot = new Vector2(0.5f, 0.5f);
        txtRt.sizeDelta = new Vector2(760f, 130f);
        txtRt.anchoredPosition = new Vector2(0f, -40f);
        _cooldownText = txtGo.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset font = cooldownFont != null ? cooldownFont : instructionsFont;
        if (font != null) _cooldownText.font = font;
        _cooldownText.fontSize = 36f;
        _cooldownText.color = new Color(1f, 0.85f, 0.4f);
        _cooldownText.alignment = TextAlignmentOptions.Center;
        _cooldownText.raycastTarget = false;

        _cooldownGroup.gameObject.SetActive(false);
    }

    /// <summary>Ancho que tendrá un icono a una altura dada, conservando el aspecto del sprite.</summary>
    private static float IconWidth(Sprite icon, float height)
    {
        if (icon != null && icon.rect.height > 0f)
            return height * (icon.rect.width / icon.rect.height);
        return height;
    }

    private void BuildInstructionRow(string rowName, Sprite icon, float iconHeight, string label, float topY, float textX)
    {
        GameObject row = new GameObject(rowName, typeof(RectTransform));
        RectTransform rowRt = row.GetComponent<RectTransform>();
        rowRt.SetParent(_instructionsRoot, false);
        rowRt.anchorMin = new Vector2(0f, 1f);
        rowRt.anchorMax = new Vector2(0f, 1f);
        rowRt.pivot = new Vector2(0f, 1f);
        rowRt.anchoredPosition = new Vector2(0f, topY);

        // Icono: alto fijo, ancho proporcional al aspecto del sprite.
        float iconWidth = IconWidth(icon, iconHeight);

        GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.SetParent(rowRt, false);
        iconRt.anchorMin = new Vector2(0f, 1f);
        iconRt.anchorMax = new Vector2(0f, 1f);
        iconRt.pivot = new Vector2(0f, 1f);
        iconRt.anchoredPosition = Vector2.zero;
        iconRt.sizeDelta = new Vector2(iconWidth, iconHeight);

        Image img = iconGo.GetComponent<Image>();
        img.sprite = icon;
        img.preserveAspect = true;
        img.raycastTarget = false;

        // Texto al lado del icono, centrado verticalmente respecto a su alto.
        GameObject txtGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.SetParent(rowRt, false);
        txtRt.anchorMin = new Vector2(0f, 1f);
        txtRt.anchorMax = new Vector2(0f, 1f);
        txtRt.pivot = new Vector2(0f, 1f);
        txtRt.anchoredPosition = new Vector2(textX, 0f);
        txtRt.sizeDelta = new Vector2(420f, iconHeight);

        TextMeshProUGUI tmp = txtGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        if (instructionsFont != null) tmp.font = instructionsFont;
        tmp.fontSize = instructionsFontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
    }
}
