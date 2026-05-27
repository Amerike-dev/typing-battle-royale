using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class MonolithTypingChallenge : MonoBehaviour
{
    public static MonolithTypingChallenge Instance;
    
    public Canvas myCanvas;
    [SerializeField] private TextMeshProUGUI runeDisplay;
    [SerializeField] private TMP_InputField hiddenInput;
    [SerializeField] private MonolithUIController uiController;

    private MonolithController _monolith;
    private Spell _spell;
    private int _spellIndex;
    private PlayerController _player;

    private void Awake() => Instance = this;

    private void Start()
    {
        if (myCanvas != null) myCanvas.enabled = false;
    }

    public void Begin(MonolithController monolith, Spell spell, int index, PlayerController player)
    {
        _monolith = monolith;
        _spell = spell;
        _spellIndex = index;
        _player = player;

        _player.NullMoveSpeed();
        
        hiddenInput.text = "";
        hiddenInput.onValueChanged.RemoveAllListeners();
        hiddenInput.onValueChanged.AddListener(OnType);

        myCanvas.enabled = true;
        hiddenInput.ActivateInputField();
        hiddenInput.Select();
        
        if (uiController != null)
        {
            uiController.UpdateDisplay(_spell.runeString, 0, false);
        }
    }

    private void OnType(string typed)
    {
        bool hasError = false;
        int currentIndex = typed.Length;
        
        for (int i = 0; i < typed.Length; i++)
        {
            if (i >= _spell.runeString.Length || typed[i] != _spell.runeString[i])
            {
                hasError = true;
                break;
            }
        }
        
        if (uiController != null) 
        {
            uiController.UpdateDisplay(_spell.runeString, hasError ? typed.Length - 1 : typed.Length, hasError);
        }

        if (hasError) 
        {
            FailChallenge();
            return;
        }

        if (typed.Length == _spell.runeString.Length)
        {
            Invoke(nameof(WinChallenge), 0.05f);
        }
    }

    private void FailChallenge()
    {
        Debug.Log("¡Fallaste el tipeo!");
        Close();
    }

    private void WinChallenge()
    {
        hiddenInput.onValueChanged.RemoveAllListeners();
        
        Debug.Log("¡Hechizo conseguido!");
        Close();
        _player.ClaimMonolithSpellServerRpc(_monolith.NetworkObjectId, _spellIndex, _spell.spellName);
    }

    private void Close()
    {
        myCanvas.enabled = false;
        _player.MoveSpeed();
        
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    void Update()
    {
        if (myCanvas.enabled && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            FailChallenge();
        }
    }
}
