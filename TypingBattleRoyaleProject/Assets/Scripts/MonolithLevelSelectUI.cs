using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class MonolithLevelSelectUI : MonoBehaviour
{
    public static MonolithLevelSelectUI Instance;
    [SerializeField] private Canvas myCanvas;
    [SerializeField] private MonolithSpellButton[] spellButtons;

    private PlayerController _localPlayer;
    
    private void Awake() => Instance = this;

    private void Start()
    {
        if (myCanvas != null) myCanvas.enabled = false;
    }

    void Update()
    {
        if (myCanvas.enabled && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Hide();
            return;
        }

        if (!myCanvas.enabled) return;
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
                // 2. EL BUG INVISIBLE: Hay que limpiar los caracteres nulos que deja Netcode
                string nameToFind = monolith.syncedSpellNames[i].ToString().Trim('\0');
                
                Spell spell = monolith.allSpells.FirstOrDefault(s => s != null && s.spellName == nameToFind);
            
                if (spell != null)
                {
                    spellButtons[i].gameObject.SetActive(true); // Aseguramos que se vea
                    spellButtons[i].Setup(spell, () => SelectSpell(spell));
                }
                else
                {
                    Debug.LogError($"[UI] No encontré el hechizo '{nameToFind}' en allSpells.");
                    spellButtons[i].Clear(); // Lo limpiamos si hubo error
                }
            }
            else 
            {
                // 3. LIMPIEZA: Si sobran botones, los limpiamos/apagamos para que no queden datos viejos
                spellButtons[i].Clear();
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        myCanvas.enabled = true;
    }

    private void SelectSpell(Spell spell)
    {
        Hide();
        Debug.Log($"Seleccionaste: {spell.spellName}");
    }

    private void Hide()
    {
        myCanvas.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (_localPlayer != null) _localPlayer.MoveSpeed();
    }
}
