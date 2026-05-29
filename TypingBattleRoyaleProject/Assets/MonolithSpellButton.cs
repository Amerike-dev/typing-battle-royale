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

    [Header("Layout de iconos de Tier")]
    [Tooltip("Alto/ancho de cada icono en px.")]
    [SerializeField] private float iconSize = 96f;
    [Tooltip("Tamaño de fuente del nombre del hechizo.")]
    [SerializeField] private float spellNameFontSize = 36f;
    [Tooltip("Cuánto se superponen los iconos en px (se aplica como spacing negativo). Mayor = más encimados, tipo 'CCO'.")]
    [SerializeField] private float iconOverlap = 26f;
    [Tooltip("Empuje de los iconos hacia la izquierda (padding izquierdo del layout, en px).")]
    [SerializeField] private float iconsLeftPadding = 22f;
    [Tooltip("Fracción del alto del botón que ocupa la franja de iconos (0-1), centrada verticalmente.")]
    [Range(0.1f, 1f)]
    [SerializeField] private float iconsAreaHeight = 0.8f;

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

    public void Setup(Spell spell, int state, Action onClick)
    {
        if (spell == null) return;
        
        if (spellNameText != null)
        {
            spellNameText.text = spell.spellName;
            spellNameText.ForceMeshUpdate();
        }
        if (tierText != null) tierText.text = spell.tier.ToString();

        UpdateElementAndTier(spell.elementType, spell.tier);
        ConfigureIconLayout();
        _onClickAction = onClick;

        SetButtonState(state);
    }

    private void SetButtonState(int state)
    {
        _button.interactable = (state == 0);

        Color targetColor = Color.white;
        string textOverride = null;

        if (state == 1)
        {
            targetColor = new Color(0.2f, 0.2f, 0.2f, 1f); 
        }
        else if (state == 2)
        {
            targetColor = new Color(1f, 0.8f, 0f, 1f);
            textOverride = "Spell ya desbloqueado";
        }

        if (spellNameText != null) 
        {
            spellNameText.color = targetColor;
            if (textOverride != null) spellNameText.text = textOverride;
        }
        
        if (tierText != null) tierText.color = targetColor;

        for (int i = 0; i < tierElementIcons.Length; i++)
        {
            if (tierElementIcons[i] != null) tierElementIcons[i].color = targetColor;
        }
    }

    private void UpdateElementAndTier(Elements elementType, SpellTiers tier)
    {
        Sprite selectedSprite = defaultIcon;
        Color selectedColor = Color.white;

        switch (elementType)
        {
            case Elements.Fire: 
                selectedSprite = fireIcon;
                selectedColor = Color.red; 
                break;
            case Elements.Water: 
                selectedSprite = waterIcon; 
                selectedColor = Color.blue;
                break;
            case Elements.Earth: 
                selectedSprite = earthIcon;
                selectedColor = new Color(0.45f, 0.25f, 0.1f, 1f); 
                break;
            case Elements.Wind: 
                selectedSprite = windIcon;
                selectedColor = new Color(0.6f, 1f, 0.8f, 1f); 
                break;
            case Elements.Nature: 
                selectedSprite = natureIcon;
                selectedColor = Color.green;
                break;
            case Elements.Thunder: 
                selectedSprite = thunderIcon;
                selectedColor = Color.yellow;
                break;
            case Elements.Dark: 
                selectedSprite = darkIcon;
                selectedColor = new Color(0.15f, 0.1f, 0.25f, 1f); 
                break;
            case Elements.Light: 
                selectedSprite = lightIcon;
                selectedColor = new Color(1f, 0.95f, 0.6f, 1f);
                break;
            case Elements.Ice: 
                selectedSprite = iceIcon;
                selectedColor = Color.cyan; 
                break;
            case Elements.Lava: 
                selectedSprite = lavaIcon;
                selectedColor = new Color(1f, 0.35f, 0f, 1f); 
                break;
            case Elements.None:
            default: 
                selectedSprite = defaultIcon;
                selectedColor = Color.white;
                break;
        }

        SetButtonNormalColor(selectedColor);

        int targetAmount = 1; 
        if (tier == SpellTiers.T2) targetAmount = 2;
        else if (tier == SpellTiers.T3) targetAmount = 3;

        for (int i = 0; i < tierElementIcons.Length; i++)
        {
            if (tierElementIcons[i] != null)
            {
                tierElementIcons[i].sprite = selectedSprite;
                tierElementIcons[i].gameObject.SetActive(i < targetAmount);
            }
        }
    }

    /// <summary>
    /// Configura el HorizontalLayoutTier para que los iconos sean cuadrados de tamaño fijo,
    /// se superpongan un poco (spacing negativo), queden alineados a la izquierda y ocupen
    /// buena parte del alto del botón (franja centrada verticalmente).
    /// </summary>
    private void ConfigureIconLayout()
    {
        if (tierElementIcons == null) return;

        Image first = null;
        foreach (var ic in tierElementIcons)
        {
            if (ic != null) { first = ic; break; }
        }
        if (first == null || first.transform.parent == null) return;

        Transform container = first.transform.parent;

        var hlg = container.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = -Mathf.Abs(iconOverlap);

            RectOffset pad = hlg.padding;
            pad.left = Mathf.RoundToInt(iconsLeftPadding);
            pad.right = 0;
            pad.top = 0;
            pad.bottom = 0;
            hlg.padding = pad;
        }

        // La franja de iconos ocupa una fracción del alto del botón, centrada verticalmente.
        if (container is RectTransform containerRt)
        {
            float half = Mathf.Clamp01(iconsAreaHeight) * 0.5f;
            containerRt.anchorMin = new Vector2(containerRt.anchorMin.x, 0.5f - half);
            containerRt.anchorMax = new Vector2(containerRt.anchorMax.x, 0.5f + half);
            Vector2 offMin = containerRt.offsetMin; offMin.y = 0f; containerRt.offsetMin = offMin;
            Vector2 offMax = containerRt.offsetMax; offMax.y = 0f; containerRt.offsetMax = offMax;
        }

        foreach (var ic in tierElementIcons)
        {
            if (ic == null) continue;
            ic.preserveAspect = true;
            ic.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);
        }

        // El texto debe quedar DELANTE de los iconos. En UGUI el orden de hermanos define el
        // z-order (el último se dibuja encima), así que mandamos el texto al final.
        if (spellNameText != null)
        {
            spellNameText.fontSize = spellNameFontSize;
            spellNameText.enableAutoSizing = false;
            spellNameText.transform.SetAsLastSibling();
        }
    }

    // Color de reposo fijo para TODOS los botones (#D4E9E9), independiente del elemento.
    // Así contrasta con el highlight, que conserva el color del elemento.
    private static readonly Color NormalColor = new Color(0.8313726f, 0.9137255f, 0.9137255f, 1f);

    private void SetButtonNormalColor(Color elementColor)
    {
        if (_button == null) return;

        ColorBlock colors = _button.colors;

        colors.normalColor = NormalColor;        // gris para todos
        colors.highlightedColor = elementColor;  // color del elemento (el highlight actual) -> contrasta
        colors.selectedColor = elementColor;
        colors.pressedColor = elementColor;

        _button.colors = colors;

    }

    public void Clear()
    {
        gameObject.SetActive(false); 
    }
}