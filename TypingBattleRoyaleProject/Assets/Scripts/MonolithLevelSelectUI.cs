using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;

public class MonolithLevelSelectUI : MonoBehaviour
{
    public static MonolithLevelSelectUI Instance;
    public Canvas myCanvas;
    [SerializeField] private MonolithSpellButton[] spellButtons;

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

        if (_localPlayer != null) _localPlayer.MoveSpeed();
    }
}
