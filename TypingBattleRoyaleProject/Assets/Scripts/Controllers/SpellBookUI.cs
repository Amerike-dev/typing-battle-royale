using System;
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

    public int spellsPerPage = 3;

    private IReadOnlyList<SpellData> currentSpells;
    private int currentPage = 0;
    private int selectedIndex = 0;

    public SpellTiers playerTier = SpellTiers.TierOne;

    public event Action<SpellData> OnSpellConfirmed;
    public event Action OnSelectionCancelled;

    void Awake()
    {
        if (slots == null || slots.Length == 0)
        {
            EnsurePlaceholderSlots();
        }
    }

    private void EnsurePlaceholderSlots()
    {
        if (GetComponent<RectTransform>() == null) gameObject.AddComponent<RectTransform>();

        var background = GetComponent<Image>();
        if (background == null)
        {
            background = gameObject.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);
        }

        var layout = GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(12, 12, 12, 12);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
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
            slotImage.color = Color.white;
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
            tmp.fontSize = 24f;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Left;
            texts[i] = tmp;

            slots[i] = slotGO;
        }
    }

    public void Show(IReadOnlyList<SpellData> spells)
    {
        gameObject.SetActive(true);
        currentPage = 0;
        selectedIndex = 0;
        Refresh(spells, 0);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Refresh(IReadOnlyList<SpellData> spells, int page)
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
                SpellData spell = spells[spellIndex];

                texts[i].text = spell.runeString + " " + spell.spellTier.ToString();

                if ((int)spell.spellTier > (int)playerTier)
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
                texts[i].text = spellCount == 0 && i == 0 ? "Sin hechizos — usa default" : "—";
                images[i].color = new Color(1f, 1f, 1f, 0.25f);
            }
        }

        UpdateSelectionVisual();
    }

    void Update()
    {
        if (currentSpells == null) return;

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
        int maxPage = Mathf.Max(0, Mathf.CeilToInt((float)currentSpells.Count / spellsPerPage) - 1);

        float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        bool pageUp = Keyboard.current != null && Keyboard.current.pageUpKey.wasPressedThisFrame;
        bool pageDown = Keyboard.current != null && Keyboard.current.pageDownKey.wasPressedThisFrame;

        if (scroll > 0f || pageUp)
        {
            currentPage = Mathf.Max(0, currentPage - 1);
            selectedIndex = 0;
            Refresh(currentSpells, currentPage);
        }
        else if (scroll < 0f || pageDown)
        {
            currentPage = Mathf.Min(currentPage + 1, maxPage);
            selectedIndex = 0;
            Refresh(currentSpells, currentPage);
        }
    }

    private void HandleSelectionNavigation()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            selectedIndex--;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            selectedIndex++;
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, slots.Length - 1);
    }

    private void ConfirmSelection()
    {
        if (currentSpells == null) return;

        int spellIndex = currentPage * spellsPerPage + selectedIndex;
        if (spellIndex < 0 || spellIndex >= currentSpells.Count) return;

        SpellData chosen = currentSpells[spellIndex];
        if (chosen == null) return;

        if ((int)chosen.spellTier > (int)playerTier) return;

        OnSpellConfirmed?.Invoke(chosen);
    }

    void UpdateSelectionVisual()
    {
        if (currentSpells == null) return;

        for (int i = 0; i < images.Length; i++)
        {
            int spellIndex = currentPage * spellsPerPage + i;

            if (spellIndex >= currentSpells.Count)
                continue;

            SpellData spell = currentSpells[spellIndex];

            if ((int)spell.spellTier > (int)playerTier)
            {
                images[i].color = Color.gray;
                continue;
            }

            if (i == selectedIndex)
            {
                images[i].color = Color.yellow;
            }
            else
            {
                images[i].color = Color.white;
            }
        }
    }
}
