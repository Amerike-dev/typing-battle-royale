using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MonolithSpellButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI spellNameText;
    [SerializeField] private TextMeshProUGUI tierText;

    [Header("Iconos que representan Elemento y Tier")]
    [SerializeField] private Image[] tierElementIcons;

    [Header("Iconos de Elementos")]
    [SerializeField] private Sprite fireIcon;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite earthIcon;
    [SerializeField] private Sprite windIcon;
    [SerializeField] private Sprite natureIcon;
    [SerializeField] private Sprite thunderIcon;
    [SerializeField] private Sprite darkIcon;
    [SerializeField] private Sprite lightIcon;
    [SerializeField] private Sprite iceIcon;
    [SerializeField] private Sprite lavaIcon;
    [SerializeField] private Sprite defaultIcon;
    
    private Button _button;
    private Action _onClickAction;
    

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => _onClickAction?.Invoke());
    }

    public void Setup(Spell spell, Action onClick)
    {
        if (spell == null) return;
        
        if (spellNameText != null)
        {
            spellNameText.text = spell.spellName;
            spellNameText.ForceMeshUpdate();
        }

        if (tierText != null)
        {
            tierText.text = spell.tier.ToString();
        }

        UpdateElementAndTier(spell.elementType, spell.tier);
    
        _onClickAction = onClick;
    }

    private void UpdateElementAndTier(Elements elementType, SpellTiers tier)
    {
        Sprite selectedSprite = defaultIcon;

        switch (elementType)
        {
            case Elements.Fire: selectedSprite = fireIcon; break;
            case Elements.Water: selectedSprite = waterIcon; break;
            case Elements.Earth: selectedSprite = earthIcon; break;
            case Elements.Wind: selectedSprite = windIcon; break;
            case Elements.Nature: selectedSprite = natureIcon; break;
            case Elements.Thunder: selectedSprite = thunderIcon; break;
            case Elements.Dark: selectedSprite = darkIcon; break;
            case Elements.Light: selectedSprite = lightIcon; break;
            case Elements.Ice: selectedSprite = iceIcon; break;
            case Elements.Lava: selectedSprite = lavaIcon; break;
            case Elements.None:
            default: selectedSprite = defaultIcon; break;
        }

        int targetAmount = 1; 
        if (tier == SpellTiers.TierTwo) targetAmount = 2;
        else if (tier == SpellTiers.TierThree) targetAmount = 3;

        for (int i = 0; i < tierElementIcons.Length; i++)
        {
            if (tierElementIcons[i] != null)
            {
                tierElementIcons[i].sprite = selectedSprite;
                tierElementIcons[i].gameObject.SetActive(i < targetAmount);
            }
        }
    }

    public void Clear()
    {
        gameObject.SetActive(false); 
    }
}