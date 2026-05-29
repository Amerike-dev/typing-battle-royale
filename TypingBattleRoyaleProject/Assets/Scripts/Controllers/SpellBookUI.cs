using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] Vector2 _hidePos=new Vector2(50,0);
    [SerializeField] Vector2 _showPos=new Vector2(0,0);
    [SerializeField] float _time = 0.2f;
    private Coroutine _moveRoutine;
    private Coroutine _hideRoutine;
    Coroutine _spellBookCoroutine;

    [Header("Instrucciones (controles, esquina inferior izquierda)")]
    [SerializeField] private float selectIconHeight = 96f;   // ws_0
    [SerializeField] private float pageIconHeight = 80f;     // ws_1 / ws_2 (A y D)
    [SerializeField] private float confirmIconHeight = 64f;  // enterGroup
    [SerializeField] private float instructionsFontSize = 28f;
    [Tooltip("Separación desde la esquina inferior izquierda del canvas.")]
    [SerializeField] private Vector2 instructionsPadding = new Vector2(40f, 40f);
    [SerializeField] private float instructionsRowGap = 18f;
    [SerializeField] private float instructionsIconTextGap = 16f;
    private InstructionIcons _iconSet;
    private GameObject _instructionsRoot;

    void Awake()
    {
        if (slots == null || slots.Length == 0)
        {
            EnsurePlaceholderSlots();
        }
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

        UIMove(_showPos);
        UIAnimator.FadeIn(_canvasGroup, _time);
        currentPage = 0;
        selectedIndex = 0;
        Refresh(spells ?? new List<Spell>(), 0);
    }

    public void Hide()
    {
        if (!gameObject.activeInHierarchy) return;

        if (_instructionsRoot != null) _instructionsRoot.SetActive(false);

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
                texts[i].text = spell.runeString + " " + spell.tier.ToString();
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

        if ((int)chosen.tier > (int)playerTier) return;

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

            // Fondo del panel = color del elemento del hechizo. El seleccionado va a color pleno;
            // los no seleccionados, atenuados, para que el resaltado siga siendo claro.
            Color e = TypingOverlay.ElementColor(spell.elementType);
            images[i].color = (i == selectedIndex)
                ? e
                : new Color(e.r * 0.5f, e.g * 0.5f, e.b * 0.5f, 1f);
        }
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
    /// Crea (una sola vez) el overlay de controles en la esquina inferior izquierda del canvas:
    /// fila 1 = ws_0 + "Selecciona hechizo"; fila 2 = A (ws_1) y D (ws_2) separadas por su propio
    /// ancho + "Cambia de pagina"; fila 3 = enterGroup + "Confirmar". Los iconos se cargan desde el
    /// asset SpellBookInstructions (Resources), así no hay que cablearlos en la escena.
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
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0f); // esquina inferior izquierda
        rt.anchoredPosition = instructionsPadding;
        rt.localScale = Vector3.one;

        float gap = instructionsRowGap;
        // Apilamos de abajo hacia arriba: Confirmar (abajo), Cambia de pagina, Selecciona (arriba).
        float yConfirm = 0f;
        float yPage = confirmIconHeight + gap;
        float ySelect = confirmIconHeight + gap + pageIconHeight + gap;

        // Fila "Selecciona hechizo".
        RectTransform rowSel = BuildInstrRow("Row_Select", ySelect, selectIconHeight);
        float xSel = AddInstrIcon(rowSel, _iconSet != null ? _iconSet.selectIcon : null, selectIconHeight, 0f);
        AddInstrLabel(rowSel, "Selecciona hechizo", xSel + instructionsIconTextGap);

        // Fila "Cambia de pagina": A y D a la misma altura, separadas por su propio ancho.
        RectTransform rowPage = BuildInstrRow("Row_Page", yPage, pageIconHeight);
        float wA = AddInstrIcon(rowPage, _iconSet != null ? _iconSet.pageLeftIcon : null, pageIconHeight, 0f);
        float xD = wA + wA; // hueco entre A y D = ancho de un icono
        float wD = AddInstrIcon(rowPage, _iconSet != null ? _iconSet.pageRightIcon : null, pageIconHeight, xD);
        AddInstrLabel(rowPage, "Cambia de pagina", xD + wD + instructionsIconTextGap);

        // Fila "Confirmar".
        RectTransform rowConf = BuildInstrRow("Row_Confirm", yConfirm, confirmIconHeight);
        float wC = AddInstrIcon(rowConf, _iconSet != null ? _iconSet.confirmIcon : null, confirmIconHeight, 0f);
        AddInstrLabel(rowConf, "Confirmar", wC + instructionsIconTextGap);
    }

    private RectTransform BuildInstrRow(string rowName, float y, float rowHeight)
    {
        GameObject row = new GameObject(rowName, typeof(RectTransform));
        RectTransform r = row.GetComponent<RectTransform>();
        r.SetParent(_instructionsRoot.transform, false);
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 0f);
        r.anchoredPosition = new Vector2(0f, y);
        r.sizeDelta = new Vector2(600f, rowHeight);
        return r;
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

    private void AddInstrLabel(RectTransform parent, string text, float x)
    {
        GameObject g = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform r = g.GetComponent<RectTransform>();
        r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = new Vector2(0f, 0.5f);
        r.pivot = new Vector2(0f, 0.5f);
        r.sizeDelta = new Vector2(380f, parent.sizeDelta.y);
        r.anchoredPosition = new Vector2(x, 0f);
        TextMeshProUGUI tmp = g.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        if (Gonserrat != null) tmp.font = Gonserrat;
        tmp.fontSize = instructionsFontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
    }

    private static float InstrIconWidth(Sprite icon, float height)
    {
        if (icon != null && icon.rect.height > 0f)
            return height * (icon.rect.width / icon.rect.height);
        return height;
    }
}
