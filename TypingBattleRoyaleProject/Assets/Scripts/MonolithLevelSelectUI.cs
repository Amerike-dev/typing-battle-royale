using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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

    private RectTransform _instructionsRoot;

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
