using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MonolithSpellButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI spellNameText;
    [SerializeField] private TextMeshProUGUI tierText;
    [SerializeField] private Image elementIcon;
    
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
    
        _onClickAction = onClick;
    }
}