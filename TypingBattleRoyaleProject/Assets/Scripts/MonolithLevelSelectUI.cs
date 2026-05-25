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
        // 1. Control de salida (este ya lo tenías bien)
        if (myCanvas.enabled && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Hide();
            return; // Salimos de la función inmediatamente
        }

        // 2. Control de navegación (solo si el canvas está visible)
        if (!myCanvas.enabled) return;

        // Usamos variables booleanas para mayor claridad y debug
        bool arrowUp = Keyboard.current.upArrowKey.wasPressedThisFrame;
        bool arrowDown = Keyboard.current.downArrowKey.wasPressedThisFrame;

        if (arrowUp || arrowDown)
        {
            // Debug.Log($"Navegación manual: {(arrowUp ? "Arriba" : "Abajo")}"); // Opcional, para testear
            Navegar(arrowUp ? -1 : 1);
        }
    }

    private void Navegar(int direccion)
    {
        // Obtenemos el objeto seleccionado actualmente en todo el juego
        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        
        // Buscamos si ese objeto es uno de nuestros botones de hechizos
        int currentIndex = -1;
        for (int i = 0; i < spellButtons.Length; i++)
        {
            if (spellButtons[i] != null && spellButtons[i].gameObject == currentSelected)
            {
                currentIndex = i;
                break;
            }
        }

        // Si no hay ninguno seleccionado (por ejemplo, al abrir el menú por primera vez)
        // o el que estaba seleccionado no es de este menú, forzamos la selección del primero.
        if (currentIndex == -1)
        {
            if (spellButtons.Length > 0 && spellButtons[0] != null)
            {
                EventSystem.current.SetSelectedGameObject(spellButtons[0].gameObject);
            }
            return; // No hay nada más que hacer en este frame.
        }

        // Calculamos el siguiente índice, asegurándonos de que no se salga de rango (con Clamp)
        int nextIndex = Mathf.Clamp(currentIndex + direccion, 0, spellButtons.Length - 1);
        
        // Si el siguiente índice es el mismo que el actual (llegamos al inicio o al final),
        // no hacemos nada para no forzar actualizaciones innecesarias.
        if (nextIndex == currentIndex) return;

        // Aseguramos que el botón existe (por seguridad)
        if (spellButtons[nextIndex] != null)
        {
            // Forzamos la selección del nuevo botón
            // Debug.Log($"Cambiando selección de {currentIndex} a {nextIndex}"); // Opcional
            EventSystem.current.SetSelectedGameObject(spellButtons[nextIndex].gameObject);
        }
    }

    public void Show(MonolithController monolith, PlayerController player)
    {
        _localPlayer = player;
        _localPlayer.NullMoveSpeed();

        int count = Mathf.Min(monolith.syncedSpellNames.Count, spellButtons.Length);
    
        for (int i = 0; i < count; i++)
        {
            string nameToFind = monolith.syncedSpellNames[i].ToString();
            Spell spell = monolith.allSpells.FirstOrDefault(s => s != null && s.spellName == nameToFind);
        
            if (spell != null)
                spellButtons[i].Setup(spell, () => SelectSpell(spell));
            else
                Debug.LogError($"[UI] No encontré el hechizo '{nameToFind}' en la lista allSpells.");
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        myCanvas.enabled = true;

        // Lanzamos la corrutina para asegurar que el sistema está listo
        StartCoroutine(SelectFirstButtonDelayed());
    }

    private System.Collections.IEnumerator SelectFirstButtonDelayed()
{
    // Esperamos al final del frame actual, donde Unity ya procesó la UI
    yield return new WaitForEndOfFrame();
    
    if (spellButtons.Length > 0 && spellButtons[0] != null)
    {
        EventSystem.current.SetSelectedGameObject(spellButtons[0].gameObject);
    }
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
