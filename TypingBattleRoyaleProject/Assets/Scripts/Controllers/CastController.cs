using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class CastController : MonoBehaviour
{
    public string CastText;
    public int indiceCorrecto = 0;
    public bool spellComplete=false;
    public int _stringIndex;


    public event Action OnSpellCast;

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

    [Header("ViewFeedback")]
    public SpellUIController uiController;

    //Timer para usar el BackSpace y evitar errores con el index del string
    private const float _backSpaceDelay = 0.4f;
    private const float _backSpaceCooldown = 0.05f;
    private float _backSpaceTimer = 0f;

    //Checker para que solo se borre el color en el UI sin romper el index
    private int _errorCount = 0;

    private void OnEnable()
    {
        _casting = true;
        _typingStats = new TypingStats();
        CombatLogic.SetText(spellText);
        _errorCount = 0;
        _cast.action.started += EvaluateAccuracy;
        wordsPerMinute = 0;
        accuracy = 0;
        StartCoroutine(CountTimeElapsed());
    }

    private void OnDisable()
    {
        StopCoroutine(CountTimeElapsed());
    }
    private void Start()
    {
        playerUI.InputSpellText.onValueChanged.AddListener(OnTextChanged);
        CastText = playerUI.InputSpellText.text;
    }
    private void Update()
    {
        CastText = playerUI.InputSpellText.text;
    }

    //Revision de Texto
    void ValidateCharacter(string cast, string spellText)
    {
        for (int i = 0; i < cast.Length; i++)
        {
            if (i < spellText.Length)
            {
                if (cast[i] == spellText[i])
                {
                    if (CastText.Length == spellText.Length)
                    {
                        SpellCompleted();
                    }
                }
                else
                {
                    incorrectInput++;
                }
                    
            }
            /*else
            {
                
            }*/
        }
    }
    void Escribir(char c)
    {
        if (c == '\n' || c == '\r') return;
        //Proceso
        CastText += c;
        //Variables
        _totalKeysPressed++;
        UpdateState();
    }
    void Borrar()
    {
        if (CastText.Length == 0)
            return;

        CastText = CastText.Substring(0, CastText.Length - 1);

        UpdateState();
    }

    //Evaluacion del Cast
    private void EvaluateAccuracy(InputAction.CallbackContext obj)
    {
        _typingStats.timeElapsed = _timeElapsed;
        _typingStats.hits = spellText.Length - incorrectInput;
        _typingStats.totalKeystrokes = _totalKeysPressed;
        wordsPerMinute = _typingStats.GetWPM();
        accuracy = _typingStats.GetAccuracy();
        _casting = false;

        OnSpellCast?.Invoke();
        //Se reinicia el script en el onDisable, aqui antes de eso iria la funcion que castea el hechizo y ensear el wpm y el accuracy en un display

        gameObject.SetActive(false);
    }
    public IEnumerator CountTimeElapsed()
    {
        while (_casting)
        {
            _timeElapsed++;
            yield return new WaitForSeconds(1f);
        }
    }
    public void OnTextChanged(string CastText)
    {
        ValidateCharacter(CastText,spellText);
    }
    public void UpdateState()
    {
        ValidateCharacter(CastText,spellText);
    }
    void SpellCompleted()
    {
        Debug.Log("Hechico Completado");
    }


    /*public void PressBackSpace()
    {
        if (_incorrectLetter<=0)
        {
            if (CombatLogic._stringIndex <= 0) return;

            CombatLogic._stringIndex--;

            if (_errorCount > 0)
            {
                _errorCount--;
                _incorrectLetter = Mathf.Max(0, _incorrectLetter - 1);
            }
            else
            {
                incorrectInput = Mathf.Max(0, incorrectInput - 1);
                CombatLogic.EraseChar();
            }

            uiController.UpdateDisplay(CombatLogic.CurrentIndex(), _errorCount > 0);
        }
        else if(_incorrectLetter >= 1)
        {
            _incorrectLetter--;
        }

        /*if (CombatLogic._stringIndex <= 0) return;

        CombatLogic._stringIndex--;

        if (_errorCount > 0)
        {
            _errorCount--;
            _incorrectLetter = Mathf.Max(0, _incorrectLetter - 1);
        }
        else
        {
            incorrectInput = Mathf.Max(0, incorrectInput - 1);
            CombatLogic.EraseChar();
        }

        uiController.UpdateDisplay(CombatLogic.CurrentIndex(), _errorCount > 0);
    }*/
}
