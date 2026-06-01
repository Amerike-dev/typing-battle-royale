using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpellBookUI : MonoBehaviour
{
    public GameObject[] slots;
    public Image[] images;
    public TMP_Text[] texts;
    public Sprite iconSprite;
    public TMP_FontAsset Gonserrat;

    public int spellsPerPage = 3;

    private IReadOnlyList<Spell> currentSpells;
    private int currentPage = 0;
    private int selectedIndex = 0;

    public SpellTiers playerTier = SpellTiers.T1;

    public event Action<Spell> OnSpellConfirmed;
    public event Action OnSelectionCancelled;

    [Header("UIanimation")]
    [SerializeField] RectTransform _panelUI;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Vector2 _hidePos;
    [SerializeField] Vector2 _showPos;
    [SerializeField] float _time = 0.2f;
    [SerializeField] GameObject _changeObjetive;
    private Coroutine _moveRoutine;
    private Coroutine _hideRoutine;
    Coroutine _spellBookCoroutine;

    [Header("Instrucciones (controles, una línea arriba al centro)")]
    [SerializeField] private float selectIconHeight = 44f;   // ws_0 (W/S)
    [SerializeField] private float pageIconHeight = 40f;     // ws_1 / ws_2 (A y D)
    [SerializeField] private float confirmIconHeight = 40f;  // enterGroup
    [SerializeField] private float instructionsFontSize = 22f;
    [Tooltip("Separación desde el borde superior del canvas (solo se usa Y).")]
    [SerializeField] private Vector2 instructionsPadding = new Vector2(0f, 14f);
    [Tooltip("Separación horizontal entre los tres grupos de instrucciones.")]
    [SerializeField] private float instructionsGroupGap = 40f;
    [SerializeField] private float instructionsIconTextGap = 12f;
    private InstructionIcons _iconSet;
    private GameObject _instructionsRoot;

    [Header("Selección (animación de escala)")]
    [Tooltip("Escala del slot seleccionado. Los no seleccionados quedan en 1.")]
    [SerializeField] private float _selectedScale = 1.12f;
    [Tooltip("Duración de la animación de escala al cambiar de selección.")]
    [SerializeField] private float _scaleAnimTime = 0.18f;
    private int _animatedIndex = -1;
    private int _animatedPage = -1;

    [Header("Mensaje de tier bloqueada")]
    [Tooltip("Texto donde se muestra el aviso. Si se deja vacío, se crea uno automáticamente bajo el panel.")]
    [SerializeField] private TMP_Text _lockedMessageText;
    [SerializeField] private string _lockedMessage = "Tier bloqueada, lanza mas hechizos de una tier menor para desbloquear";
    [SerializeField] private Color _lockedMessageColor = new Color(1f, 0.45f, 0.45f);
    [Tooltip("Segundos que el aviso permanece visible antes de desvanecerse.")]
    [SerializeField] private float _lockedMessageHold = 1.6f;
    private CanvasGroup _lockedMsgGroup;
    private Coroutine _lockedMsgRoutine;

    [Header("Cooldown (texto a la izquierda de cada botón)")]
    [Tooltip("Devuelve los segundos de cooldown restantes de un hechizo (0 = listo). Lo asigna BattleState.")]
    public Func<Spell, float> CooldownRemaining;
    [SerializeField] private float _cooldownFontSize = 22f;
    [SerializeField] private Color _cooldownColor = Color.white;
    [Tooltip("Separación del texto respecto al borde izquierdo del botón (px).")]
    [SerializeField] private float _cooldownLeftGap = 8f;
    private TMP_Text[] _cooldownLabels;

    void Awake()
    {
        if (slots == null || slots.Length == 0)
        {
            EnsurePlaceholderSlots();
        }
        _changeObjetive.SetActive(false);
    }

    private void EnsurePlaceholderSlots()
    {
        var rect = GetComponent<RectTransform>();
        if (rect == null) rect = gameObject.AddComponent<RectTransform>();

        if (rect.anchorMin == Vector2.zero && rect.anchorMax == Vector2.one && rect.sizeDelta == Vector2.zero)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(420f, 220f);
        }

        var layout = GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;
        }

        int count = spellsPerPage > 0 ? spellsPerPage : 3;
        slots = new GameObject[count];
        images = new Image[count];
        texts = new TMP_Text[count];

        for (int i = 0; i < count; i++)
        {
            GameObject slotGO = new GameObject($"Slot_{i}", typeof(RectTransform));
            slotGO.transform.SetParent(transform, false);

            var slotLE = slotGO.AddComponent<LayoutElement>();
            slotLE.preferredHeight = 56f;

            var slotImage = slotGO.AddComponent<Image>();
            slotImage.sprite = iconSprite;
            slotImage.color = Color.white;
            slotImage.rectTransform.sizeDelta = new Vector2(500, 80);
            images[i] = slotImage;

            GameObject textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(slotGO.transform, false);

            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 4f);
            textRect.offsetMax = new Vector2(-16f, -4f);

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "—";
            tmp.font = Gonserrat;
            tmp.UpdateFontAsset();
            tmp.ForceMeshUpdate();
            tmp.fontSize = 20f;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            texts[i] = tmp;

            slots[i] = slotGO;
        }
    }

    public void Show(IReadOnlyList<Spell> spells)
    {
        if (_hideRoutine != null)
        {
            StopCoroutine(_hideRoutine);
            _hideRoutine = null;
        }

        if (_canvasGroup != null) _canvasGroup.gameObject.SetActive(true);
        gameObject.SetActive(true);

        EnsureInstructions();
        if (_instructionsRoot != null) _instructionsRoot.SetActive(true);
        EnsureCooldownLabels();

        UIMove(_showPos);
        UIAnimator.FadeIn(_canvasGroup, _time);
        _changeObjetive.SetActive(true);
        currentPage = 0;
        selectedIndex = 0;
        ResetSelectionScale();
        HideLockedMessage();
        Refresh(spells ?? new List<Spell>(), 0);
    }

    public void Hide()
    {
        if (!gameObject.activeInHierarchy) return;

        if (_instructionsRoot != null) _instructionsRoot.SetActive(false);
        HideLockedMessage();

        UIMove(_hidePos);
        UIAnimator.FadeOut(_canvasGroup, _time);
        if (_hideRoutine != null) StopCoroutine(_hideRoutine);
        _hideRoutine = StartCoroutine(ChangeMode());
    }

    public void Refresh(IReadOnlyList<Spell> spells, int page)
    {
        currentSpells = spells;
        currentPage = Mathf.Max(0, page);

        int startIndex = currentPage * spellsPerPage;
        int spellCount = spells != null ? spells.Count : 0;

        for (int i = 0; i < slots.Length; i++)
        {
            int spellIndex = startIndex + i;

            slots[i].SetActive(true);

            if (spellIndex < spellCount)
            {
                Spell spell = spells[spellIndex];
                texts[i].text = spell.spellName + " " + spell.tier.ToString();
            }
            else
            {
                texts[i].text = "—";
            }
        }

        // El color del fondo de cada slot lo decide UpdateSelectionVisual (color del elemento).
        UpdateSelectionVisual();
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        HandlePageNavigation();
        HandleSelectionNavigation();

        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ConfirmSelection();
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnSelectionCancelled?.Invoke();
            return;
        }

        UpdateSelectionVisual();
        UpdateCooldownLabels();
    }

    private void HandlePageNavigation()
    {
        int spellCount = currentSpells != null ? currentSpells.Count : 0;
        int maxPage = Mathf.Max(0, Mathf.CeilToInt((float)spellCount / spellsPerPage) - 1);

        float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        bool pageUp = Keyboard.current != null && Keyboard.current.pageUpKey.wasPressedThisFrame;
        bool pageDown = Keyboard.current != null && Keyboard.current.pageDownKey.wasPressedThisFrame;
        bool leftArrow = Keyboard.current != null && Keyboard.current.leftArrowKey.wasPressedThisFrame;
        bool rightArrow = Keyboard.current != null && Keyboard.current.rightArrowKey.wasPressedThisFrame;
        bool aKey = Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame; // ws_1
        bool dKey = Keyboard.current != null && Keyboard.current.dKey.wasPressedThisFrame; // ws_2

        if (scroll > 0f || pageUp || leftArrow || aKey)
        {
            currentPage = Mathf.Max(0, currentPage - 1);
            selectedIndex = 0;
            Refresh(currentSpells, currentPage);
        }
        else if (scroll < 0f || pageDown || rightArrow || dKey)
        {
            currentPage = Mathf.Min(currentPage + 1, maxPage);
            selectedIndex = 0;
            Refresh(currentSpells, currentPage);
        }
    }

    private void HandleSelectionNavigation()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame) selectedIndex--;
        if (Keyboard.current.downArrowKey.wasPressedThisFrame) selectedIndex++;

        int spellCount = currentSpells != null ? currentSpells.Count : 0;
        int maxIndex;
        if (spellCount == 0)
        {
            maxIndex = 0;
        }
        else
        {
            int onThisPage = Mathf.Min(spellsPerPage, spellCount - currentPage * spellsPerPage);
            maxIndex = Mathf.Max(0, onThisPage - 1);
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, maxIndex);
    }

    private void ConfirmSelection()
    {
        int spellCount = currentSpells != null ? currentSpells.Count : 0;

        if (spellCount == 0) return;

        int spellIndex = currentPage * spellsPerPage + selectedIndex;
        if (spellIndex < 0 || spellIndex >= spellCount) return;

        Spell chosen = currentSpells[spellIndex];
        if (chosen == null) return;

        if ((int)chosen.tier > (int)playerTier)
        {
            ShowLockedMessage();
            return;
        }

        // En cooldown: bloqueado. El contador a la izquierda del botón indica cuánto falta.
        if (CooldownRemaining != null && CooldownRemaining(chosen) > 0f)
            return;

        OnSpellConfirmed?.Invoke(chosen);
    }

    void UpdateSelectionVisual()
    {
        int spellCount = currentSpells != null ? currentSpells.Count : 0;

        for (int i = 0; i < images.Length; i++)
        {
            int spellIndex = currentPage * spellsPerPage + i;

            if (spellIndex >= spellCount)
            {
                images[i].color = new Color(1f, 1f, 1f, 0.12f);
                continue;
            }

            Spell spell = currentSpells[spellIndex];

            if ((int)spell.tier > (int)playerTier)
            {
                images[i].color = new Color(0.32f, 0.32f, 0.32f, 1f); // bloqueado (tier mayor)
                continue;
            }

            if (CooldownRemaining != null && CooldownRemaining(spell) > 0f)
            {
                images[i].color = new Color(0.32f, 0.32f, 0.32f, 1f); // recargando (cooldown)
                continue;
            }

            // Fondo del panel = color del elemento del hechizo. El seleccionado va a color pleno;
            // los no seleccionados, atenuados, para que el resaltado siga siendo claro.
            Color e = TypingOverlay.ElementColor(spell.elementType);
            images[i].color = (i == selectedIndex)
                ? e
                : new Color(e.r * 0.5f, e.g * 0.5f, e.b * 0.5f, 1f);
        }

        // Solo animamos cuando realmente cambió la selección o la página (no cada frame).
        if (selectedIndex != _animatedIndex || currentPage != _animatedPage)
        {
            _animatedIndex = selectedIndex;
            _animatedPage = currentPage;
            RefreshSelectionScale();
        }
    }

    /// <summary>
    /// Anima la escala de los slots: el seleccionado crece (con un pequeño rebote) y el resto
    /// vuelve a 1. Esto deja claro de un vistazo en qué hechizo estás parado.
    /// </summary>
    private void RefreshSelectionScale()
    {
        if (slots == null) return;

        int spellCount = currentSpells != null ? currentSpells.Count : 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            int spellIndex = currentPage * spellsPerPage + i;
            bool selectable = spellIndex < spellCount;
            float target = (selectable && i == selectedIndex) ? _selectedScale : 1f;

            Transform t = slots[i].transform;
            t.DOKill();
            t.DOScale(target, _scaleAnimTime)
             .SetEase(target > 1f ? Ease.OutBack : Ease.OutQuad)
             .SetUpdate(true);
        }
    }

    /// <summary>Deja todos los slots en escala 1 al instante (al abrir el libro).</summary>
    private void ResetSelectionScale()
    {
        _animatedIndex = -1;
        _animatedPage = -1;
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            slots[i].transform.DOKill();
            slots[i].transform.localScale = Vector3.one;
        }
    }

    // ---------------- Cooldown por hechizo (contador a la izquierda) ----------------

    /// <summary>Crea (una sola vez) un texto de cooldown como hijo de cada slot, pegado a su izquierda.</summary>
    private void EnsureCooldownLabels()
    {
        if (slots == null) return;
        if (_cooldownLabels == null || _cooldownLabels.Length != slots.Length)
            _cooldownLabels = new TMP_Text[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || _cooldownLabels[i] != null) continue;

            GameObject go = new GameObject("CooldownLabel", typeof(RectTransform));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(slots[i].transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f); // borde izquierdo del botón
            rt.pivot = new Vector2(1f, 0.5f);                    // el texto queda a la izquierda del botón
            rt.sizeDelta = new Vector2(90f, 44f);
            rt.anchoredPosition = new Vector2(-_cooldownLeftGap, 0f);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (Gonserrat != null) tmp.font = Gonserrat;
            tmp.fontStyle = FontStyles.Normal; // regular
            tmp.fontSize = _cooldownFontSize;
            tmp.color = _cooldownColor;
            tmp.alignment = TextAlignmentOptions.MidlineRight;
            tmp.raycastTarget = false;
            tmp.text = "";
            _cooldownLabels[i] = tmp;
        }
    }

    /// <summary>Actualiza cada frame el contador: segundos restantes mientras recarga; vacío si está listo.</summary>
    private void UpdateCooldownLabels()
    {
        if (_cooldownLabels == null) return;

        int spellCount = currentSpells != null ? currentSpells.Count : 0;

        for (int i = 0; i < _cooldownLabels.Length; i++)
        {
            if (_cooldownLabels[i] == null) continue;

            int spellIndex = currentPage * spellsPerPage + i;
            float remaining = 0f;
            if (CooldownRemaining != null && spellIndex < spellCount && currentSpells[spellIndex] != null)
                remaining = CooldownRemaining(currentSpells[spellIndex]);

            _cooldownLabels[i].text = remaining > 0.05f ? $"{Mathf.CeilToInt(remaining)}s" : "";
        }
    }

    // ---------------- Aviso de tier bloqueada ----------------

    /// <summary>Crea (una sola vez) el texto del aviso si no se asignó uno en el Inspector.</summary>
    private void EnsureLockedMessage()
    {
        if (_lockedMessageText != null)
        {
            if (_lockedMsgGroup == null)
                _lockedMsgGroup = _lockedMessageText.GetComponent<CanvasGroup>()
                    ?? _lockedMessageText.gameObject.AddComponent<CanvasGroup>();
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.transform : transform;

        GameObject go = new GameObject("SpellBookLockedMessage", typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -190f); // debajo del centro del panel
        rt.sizeDelta = new Vector2(900f, 70f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (Gonserrat != null) tmp.font = Gonserrat;
        tmp.fontSize = 26f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        tmp.color = _lockedMessageColor;
        _lockedMessageText = tmp;

        _lockedMsgGroup = go.AddComponent<CanvasGroup>();
        _lockedMsgGroup.alpha = 0f;
    }

    /// <summary>Muestra el aviso de tier bloqueada con fade-in + sacudida, mantiene y desvanece.</summary>
    private void ShowLockedMessage()
    {
        EnsureLockedMessage();
        if (_lockedMessageText == null) return;

        _lockedMessageText.text = _lockedMessage;
        _lockedMessageText.color = _lockedMessageColor;
        _lockedMessageText.gameObject.SetActive(true);

        if (_lockedMsgRoutine != null) StopCoroutine(_lockedMsgRoutine);
        _lockedMsgRoutine = StartCoroutine(LockedMessageRoutine());
    }

    private void HideLockedMessage()
    {
        if (_lockedMsgRoutine != null)
        {
            StopCoroutine(_lockedMsgRoutine);
            _lockedMsgRoutine = null;
        }
        if (_lockedMsgGroup != null) _lockedMsgGroup.alpha = 0f;
        if (_lockedMessageText != null) _lockedMessageText.rectTransform.localRotation = Quaternion.identity;
    }

    private IEnumerator LockedMessageRoutine()
    {
        CanvasGroup cg = _lockedMsgGroup;
        RectTransform rt = _lockedMessageText.rectTransform;

        // Entrada: fade-in con una sacudida que se amortigua (como el feedback de fallo del monolito).
        float intro = 0.25f, t = 0f;
        while (t < intro)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / intro);
            cg.alpha = k;
            float angle = Mathf.Sin(k * Mathf.PI * 6f) * 6f * (1f - k);
            rt.localRotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        cg.alpha = 1f;
        rt.localRotation = Quaternion.identity;

        yield return new WaitForSecondsRealtime(_lockedMessageHold);

        // Salida: fade-out.
        float outro = 0.35f;
        t = 0f;
        while (t < outro)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = 1f - Mathf.Clamp01(t / outro);
            yield return null;
        }
        cg.alpha = 0f;
        _lockedMsgRoutine = null;
    }

    public void UIMove(Vector2 target)
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        if (!gameObject.activeInHierarchy)
        {
            if (_panelUI != null) _panelUI.anchoredPosition = target;
            return;
        }

        _moveRoutine = StartCoroutine(UIAnimator.PanelUIMove(_panelUI, target, _time));
    }
    public IEnumerator ChangeMode()
    {
        yield return new WaitForSeconds(_time);
        gameObject.SetActive(false);
        _hideRoutine = null;
    }

    // ---------------- Instrucciones de controles ----------------

    /// <summary>
    /// Crea (una sola vez) el overlay de controles en UNA sola línea horizontal, arriba al centro:
    /// [ws_0] "Selecciona hechizo"   [A] [D] "Cambia de pagina"   [enterGroup] "Confirmar".
    /// Los iconos se cargan desde el asset SpellBookInstructions (Resources), así no hay que
    /// cablearlos en la escena.
    /// </summary>
    private void EnsureInstructions()
    {
        if (_instructionsRoot != null) return;

        if (_iconSet == null) _iconSet = Resources.Load<InstructionIcons>("SpellBookInstructions");

        Canvas canvas = GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.transform : transform;

        GameObject root = new GameObject("SpellBookInstructions", typeof(RectTransform));
        _instructionsRoot = root;
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); // arriba, al centro
        rt.pivot = new Vector2(0f, 1f);
        rt.localScale = Vector3.one;

        float rowH = Mathf.Max(selectIconHeight, Mathf.Max(pageIconHeight, confirmIconHeight));
        rt.sizeDelta = new Vector2(0f, rowH); // alto para centrar verticalmente los hijos
        float x = 0f;

        // Grupo 1: Selecciona hechizo (ws_0 = W/S).
        x += AddInstrIcon(rt, _iconSet != null ? _iconSet.selectIcon : null, selectIconHeight, x);
        x += instructionsIconTextGap;
        x += AddInstrLabel(rt, "Selecciona hechizo", x);
        x += instructionsGroupGap;

        // Grupo 2: Cambia de pagina (A y D separadas por el ancho de un icono).
        float wA = AddInstrIcon(rt, _iconSet != null ? _iconSet.pageLeftIcon : null, pageIconHeight, x);
        x += wA + wA; // A + hueco del ancho de A
        float wD = AddInstrIcon(rt, _iconSet != null ? _iconSet.pageRightIcon : null, pageIconHeight, x);
        x += wD + instructionsIconTextGap;
        x += AddInstrLabel(rt, "Cambia de pagina", x);
        x += instructionsGroupGap;

        // Grupo 3: Confirmar (enterGroup).
        x += AddInstrIcon(rt, _iconSet != null ? _iconSet.confirmIcon : null, confirmIconHeight, x);
        x += instructionsIconTextGap;
        x += AddInstrLabel(rt, "Confirmar", x);

        // Centramos la línea completa y la pegamos arriba.
        rt.sizeDelta = new Vector2(x, rowH);
        rt.anchoredPosition = new Vector2(-x * 0.5f, -instructionsPadding.y);
    }

    /// <summary>Coloca un icono alineado a la izquierda y centrado verticalmente. Devuelve su ancho.</summary>
    private float AddInstrIcon(RectTransform parent, Sprite sprite, float height, float x)
    {
        float w = InstrIconWidth(sprite, height);
        GameObject g = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform r = g.GetComponent<RectTransform>();
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = new Vector2(0f, 0.5f);
        r.pivot = new Vector2(0f, 0.5f);
        r.sizeDelta = new Vector2(w, height);
        r.anchoredPosition = new Vector2(x, 0f);
        Image img = g.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return w;
    }

    /// <summary>Coloca una etiqueta a la izquierda, centrada verticalmente. Devuelve su ancho real.</summary>
    private float AddInstrLabel(RectTransform parent, string text, float x)
    {
        GameObject g = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform r = g.GetComponent<RectTransform>();
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = new Vector2(0f, 0.5f);
        r.pivot = new Vector2(0f, 0.5f);
        TextMeshProUGUI tmp = g.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        if (Gonserrat != null) tmp.font = Gonserrat;
        tmp.fontSize = instructionsFontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        float w = tmp.GetPreferredValues(text).x;
        if (w <= 0f) w = text.Length * instructionsFontSize * 0.55f; // respaldo si aún no hay layout
        r.sizeDelta = new Vector2(w, instructionsFontSize * 1.4f);
        r.anchoredPosition = new Vector2(x, 0f);
        return w;
    }

    private static float InstrIconWidth(Sprite icon, float height)
    {
        if (icon != null && icon.rect.height > 0f)
            return height * (icon.rect.width / icon.rect.height);
        return height;
    }
}
