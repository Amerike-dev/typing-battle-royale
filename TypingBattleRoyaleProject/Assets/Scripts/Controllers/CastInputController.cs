using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CastInputController : MonoBehaviour
{
    public event Action<Spell> OnSpellCast;

    public TMP_InputField castSpell;
    public TextMeshProUGUI spell;
    public Spell currentSpell;
    public string spellText;
    public int stringIndex = 0;
    public int lastInInput = 0;
    public int incorrectInput = 0;

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


    private void OnEnable()
    {
        _casting = true;
        _typingStats = new TypingStats();
        
        if (_cast != null && _cast.action != null)
        {
            _cast.action.started += EvaluateAccuracy;
        }

        wordsPerMinute = 0;
        accuracy = 0;
        StartCoroutine(CountTimeElapsed());
        
        if (castSpell != null)
        {
            castSpell.onEndEdit.AddListener(CastText);
        }
        
        if (spell != null)
        {
            spell.text = spellText;
        }

        stringIndex = 0;
        lastInInput = 0;
    }

    private void OnDisable()
    {
        StopCoroutine(CountTimeElapsed());
        
        if (castSpell != null)
        {
            castSpell.onEndEdit.RemoveListener(CastText);
        }

        if (_cast != null && _cast.action != null)
        {
            _cast.action.started -= EvaluateAccuracy;
        }
    }

    public void CastText(string cast)
    {
        stringIndex = Mathf.Min(cast.Length - 1, spellText.Length - 1);
        stringIndex = Mathf.Max(0, stringIndex);
        _totalKeysPressed++;
        Debug.Log("Current Text: " + cast);

        if (cast.Length == 0) return;

        if (cast[stringIndex] == spellText[stringIndex])
        {
            if (stringIndex <= lastInInput + 1)
            {
                lastInInput = stringIndex;
                uiController.UpdateDisplay(lastInInput + 1, false);
            }
        }

        if (cast[stringIndex] != spellText[stringIndex])
        {
            incorrectInput++;
            Debug.Log("Tsijni");
            uiController.UpdateDisplay(lastInInput + 1, true);
        }

        if (cast.Length == spellText.Length)
        {
            Debug.Log("Spell Complete Crash");
            OnDisable();
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

        OnSpellCast?.Invoke(currentSpell);
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
}
