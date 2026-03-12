using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CastInputController : MonoBehaviour
{
    
    public PlayerUI playerUI;
    public int incorrectInput = 0;
    public int lastInInput = 0;
    public string spellText;
    

    [Header("Typing Stats")]
    [SerializeField] private InputActionReference _cast;
    private bool _casting;
    private TypingStats _typingStats;
    private float _timeElapsed;
    private int _totalKeysPressed;
    public float wordsPerMinute;
    public float accuracy;
    

    //Toda la logica de reinicio de cambio de hechizos esta en onEnable  onDisable
    private void OnEnable()
    {
        _casting = true;
        _typingStats = new TypingStats();
        CombatLogic.SetText(spellText);
        Keyboard.current.onTextInput += TextInput;
        _cast.action.started += EvaluateAccuracy;
        wordsPerMinute = 0;
        accuracy = 0;
        StartCoroutine(CountTimeElapsed());
    }

    private void OnDisable()
    {
        StopCoroutine(CountTimeElapsed());
        Keyboard.current.onTextInput -= TextInput;
    }

    private void Update()
    {
        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            _totalKeysPressed++;
            incorrectInput--;
            CombatLogic.EraseChar();
        }
    }
   
    private void TextInput(char input)
    {
        _totalKeysPressed++;
        bool typed = CombatLogic.ValidateCharacter(input);

        if (input == '\n' || input == '\r') return;
        if (!typed)
        {
            incorrectInput++;
            Debug.Log("Correct letter: " + CombatLogic.SpellText[CombatLogic.CurrentIndex()] + " Mistypo: " + input);
            return;
        }

        if (CombatLogic.spellComplete)
        {
            Debug.Log("Spell Completed");
            OnDisable();
            return;
        }

        Debug.Log("Correct. Letter: " + input);

        if (CombatLogic.spellComplete)
        {
            Debug.Log("End of Spell");
        }
    }
    private void EvaluateAccuracy(InputAction.CallbackContext obj)
    {
        _typingStats.timeElapsed = _timeElapsed;
        _typingStats.hits = spellText.Length - incorrectInput;
        _typingStats.totalKeystrokes = _totalKeysPressed;
        wordsPerMinute = _typingStats.GetWPM();
        accuracy = _typingStats.GetAccuracy();
        _casting = false;

        //Se reinicia el script en el onDisable, aqui antes de eso iria la funcion que castea el hechizo y enseñar el wpm y el accuracy en un display

        gameObject.SetActive(false);
    }

    public IEnumerator CountTimeElapsed()
    {
        while(_casting)
        {
            _timeElapsed ++;
            yield return new WaitForSeconds(1f);
        }
    }
        
}
