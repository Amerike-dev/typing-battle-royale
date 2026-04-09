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

    public void Refresh(IReadOnlyList<SpellData> spells, int page)
    {
        currentSpells = spells;
        currentPage = Mathf.Max(0, page);

        int startIndex = currentPage * spellsPerPage;

        for (int i = 0; i < slots.Length; i++)
        {
            int spellIndex = startIndex + i;

            if (spellIndex < spells.Count)
            {
                SpellData spell = spells[spellIndex];

                slots[i].SetActive(true);

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
                slots[i].SetActive(false);
            }
        }

        UpdateSelectionVisual();
    }

    void Update()
    {
        if (currentSpells == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll > 0f)
        {
            currentPage--;
            currentPage = Mathf.Max(0, currentPage);
            selectedIndex = 0;
            Refresh(currentSpells, currentPage);
        }
        else if (scroll < 0f)
        {
            int maxPage = Mathf.CeilToInt((float)currentSpells.Count / spellsPerPage) - 1;

            currentPage++;
            currentPage = Mathf.Min(currentPage, maxPage);
            selectedIndex = 0;
            Refresh(currentSpells, currentPage);
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            selectedIndex--;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            selectedIndex++;
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, slots.Length - 1);

        UpdateSelectionVisual();
    }

    void UpdateSelectionVisual()
    {
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