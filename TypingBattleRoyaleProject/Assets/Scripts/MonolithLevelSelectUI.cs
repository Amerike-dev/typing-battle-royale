using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MonolithLevelSelectUI : MonoBehaviour
{
    public static MonolithLevelSelectUI Instance;
    public Canvas myCanvas;
    [SerializeField] private MonolithSpellButton[] spellButtons;

    private PlayerController _localPlayer;
    
    private void Awake() => Instance = this;

    private void Start()
    {
        if (myCanvas != null) myCanvas.enabled = false;
    }

    void Update()
    {
        if (MonolithTypingChallenge.Instance != null && MonolithTypingChallenge.Instance.myCanvas.enabled) return;
        
        if (!myCanvas.enabled) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Hide();
        }
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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        myCanvas.enabled = true;
    }
    
    private void SelectSpell(MonolithController monolith, Spell spell, int index)
    {
        Hide();
        MonolithTypingChallenge.Instance.Begin(monolith, spell, index, _localPlayer);
    }

    private void Hide()
    {
        myCanvas.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (_localPlayer != null) _localPlayer.MoveSpeed();
    }
}
