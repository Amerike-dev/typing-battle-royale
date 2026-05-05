using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CastInputController : MonoBehaviour
{
    public event Action OnSpellCast;
    public int incorrectInput = 0;
    public int lastInInput = 0;
    public string spellText;


    public TMP_InputField castSpell;
    public TextMeshProUGUI spell;
    public int stringIndex = 0;
    //public string CastSpell = "";

    public int currentIndex = 0;

    [Header("Typing Stats")]
    [SerializeField] private InputActionReference _cast;
    private bool _casting;
    private TypingStats _typingStats;
    private float _timeElapsed;
    public int _totalKeysPressed; //esta era priivada
    public float wordsPerMinute;
    public float accuracy;

    //Toda la logica de reinicio de cambio de hechizos esta en onEnable  onDisable
    [Header("ViewFeedback")]
    public SpellUIController uiController;

    //Timer para usar el BackSpace y evitar errores con el index del string
    private const float _backSpaceDelay = 0.4f;
    private const float _backSpaceCooldown = 0.5f;
    private float _backSpaceTimer = 0f;

    //Checker para que solo se borre el color en el UI sin romper el index
    public int _errorCount = 0; //esta era priivada

    private void OnEnable()
    {
        _casting = true;
        _typingStats = new TypingStats();
        //CombatLogic.SetText(spellText);
        _errorCount = 0;
        //Keyboard.current.onTextInput += TextInput;
        _cast.action.started += EvaluateAccuracy;
        wordsPerMinute = 0;
        accuracy = 0;
        StartCoroutine(CountTimeElapsed());

        stringIndex = 0;
        lastInInput = 0;
        castSpell.onEndEdit.AddListener(CastText);
        spell.text = spellText;
    }

    private void OnDisable()
    {
        StopCoroutine(CountTimeElapsed());
        //Keyboard.current.onTextInput -= TextInput;
        castSpell.onEndEdit.RemoveListener(CastText);
    }

    private void Update()
    {
        //BackspaceLogic();
        //HandleBackspace();
        //currentIndex = CombatLogic._stringIndex;
    }


    /*private void BackspaceLogic()
    {
        var backspaceKey = Keyboard.current.backspaceKey;

        if (backspaceKey.wasPressedThisFrame)
        {
            if (_errorCount <= 0)
            {
                _errorCount = 0;
                //BackspaceBehaviour();
                if (CombatLogic._stringIndex <= 0)
                {
                    CombatLogic._stringIndex = 0;
                }
                if (CombatLogic._stringIndex > 0)
                {
                    _backSpaceTimer = _backSpaceDelay;
                    BackspaceBehaviour();
                    //CombatLogic._stringIndex--;
                }
            }
            else
            {
                _errorCount--;
            }
        }
        else if (backspaceKey.isPressed) //Esto es para que la UI no implosione. Es un contador que se vale de Update por si se deja presionado
        {
            _backSpaceTimer -= Time.deltaTime;

            if (_backSpaceTimer <= 0)
            {
                _errorCount--;
                _backSpaceTimer = _backSpaceCooldown;
                if(_errorCount <= 0)
                {
                    _errorCount = 0;
                    BackspaceBehaviour();
                }
                //BackspaceBehaviour();
            }

        }
    }

    private void BackspaceBehaviour()
    {
        if (_errorCount > 0)
        {
            _errorCount--;
            bool stillError = _errorCount > 0;
            uiController.UpdateDisplay(CombatLogic.CurrentIndex(), stillError);
        }
        else if (CombatLogic.CurrentIndex() > 0)
        {
            incorrectInput = Mathf.Max(0, incorrectInput - 1);
            CombatLogic.EraseChar();
            uiController.UpdateDisplay(CombatLogic.CurrentIndex(), false);
        }
    }*/
   
    /*private void TextInput(char input)
    {
        _totalKeysPressed++;
        if (input == '\n' || input == '\r') return;

        bool typed = CombatLogic.ValidateCharacter(input);

        if (!typed)
        {
            _errorCount++;
            incorrectInput++;
            uiController.UpdateDisplay(CombatLogic.CurrentIndex(), true);
            Debug.Log("Correct letter: " + CombatLogic.SpellText[CombatLogic.CurrentIndex()] + " Mistypo: " + input);
            return;
        }

        _errorCount = 0;
        uiController.UpdateDisplay(CombatLogic.CurrentIndex(), false);

        if (CombatLogic.spellComplete)
        {
            Debug.Log("Spell Completed");
            OnDisable();
            return;
        }

        Debug.Log("Correct. Letter: " + input);
    }*/



    private void EvaluateAccuracy(InputAction.CallbackContext obj)
    {
        _typingStats.timeElapsed = _timeElapsed;
        _typingStats.hits = spellText.Length - incorrectInput;
        _typingStats.totalKeystrokes = _totalKeysPressed;
        wordsPerMinute = _typingStats.GetWPM();
        accuracy = _typingStats.GetAccuracy();
        _casting = false;

        OnSpellCast?.Invoke();
        //Se reinicia el script en el onDisable, aqui antes de eso iria la funcion que castea el hechizo y ense�ar el wpm y el accuracy en un display

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

    public void CastText(string cast)
    {
        stringIndex = Mathf.Min(cast.Length-1,spellText.Length-1);
        stringIndex = Mathf.Max(0, stringIndex);
        _totalKeysPressed++;
        Debug.Log("Current Text: "+ cast);

        if (cast.Length == 0) return;

        if (cast[stringIndex] == spellText[stringIndex])
        {
            if (stringIndex <= lastInInput + 1)
            {
                lastInInput = stringIndex;
                uiController.UpdateDisplay(lastInInput+1, false);
            }
        }
        
        if (cast[stringIndex] != spellText[stringIndex])
        {
            incorrectInput++;
            Debug.Log("Tsijni");
            uiController.UpdateDisplay(lastInInput+1, true);
        }

        if (cast.Length == spellText.Length)
        {
            Debug.Log("Spell Complete Crash");
            OnDisable();
        }
    }
}
