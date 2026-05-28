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
    public Image[] typeImages;
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
        typeImages = new Image[count];
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

            GameObject slotTypeGO = new GameObject("TypeElement");
            slotTypeGO.transform.SetParent(slotGO.transform, false);

            var TypeElement= slotTypeGO.AddComponent<Image>();
            //TypeElement.sprite = i;

            var rectType=slotTypeGO.AddComponent<RectTransform>();
            rectType.sizeDelta = new Vector2(100, 100);
            rectType.anchoredPosition = new Vector2(-700,0);


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

        UIMove(_showPos);
        UIAnimator.FadeIn(_canvasGroup, _time);
        currentPage = 0;
        selectedIndex = 0;
        Refresh(spells ?? new List<Spell>(), 0);
    }

    public void Hide()
    {
        if (!gameObject.activeInHierarchy) return;

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

                if ((int)spell.tier > (int)playerTier)
                {
                    images[i].color = Color.gray;
                }
                else
                {
                    images[i].color = Color.white;
                }
            }
            else
            {
                texts[i].text = "—";
                images[i].color = new Color(1f, 1f, 1f, 0.25f);
            }
        }

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

        if (scroll > 0f || pageUp || leftArrow)
        {
            currentPage = Mathf.Max(0, currentPage - 1);
            selectedIndex = 0;
            Refresh(currentSpells, currentPage);
        }
        else if (scroll < 0f || pageDown || rightArrow)
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
                images[i].color = new Color(1f, 1f, 1f, 0.25f);
                continue;
            }

            Spell spell = currentSpells[spellIndex];

            if ((int)spell.tier > (int)playerTier)
            {
                images[i].color = Color.gray;
                continue;
            }

            images[i].color = (i == selectedIndex) ? Color.yellow : Color.white;
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
}
