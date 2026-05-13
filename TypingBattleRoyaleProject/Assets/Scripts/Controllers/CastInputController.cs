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
    public Spell defaultSpell;
    public string spellText;
    public int stringIndex = 0;
    public int lastInInput = 0;
    public int incorrectInput = 0;

    [Header("Typing Stats")]
    [SerializeField] private InputActionReference _cast;
    private bool _casting;
    private TypingStats _typingStats;
    private float _timeElapsed;
    public int _totalKeysPressed;
    public float wordsPerMinute;
    public float accuracy;

    [Header("ViewFeedback")]
    public SpellUIController uiController;

    private float _armingTime;
    private CanvasGroup _uiCanvasGroup;


    private void OnEnable()
    {
        _casting = true;
        _typingStats = new TypingStats();
        _armingTime = Time.unscaledTime + 0.15f;

        if (_cast != null && _cast.action != null)
        {
            _cast.action.started += EvaluateAccuracy;
        }

        wordsPerMinute = 0;
        accuracy = 0;
        stringIndex = 0;
        lastInInput = 0;
        incorrectInput = 0;
        _totalKeysPressed = 0;
        _timeElapsed = 0;
        StartCoroutine(CountTimeElapsed());

        if (castSpell != null)
        {
            castSpell.text = string.Empty;
            castSpell.onEndEdit.AddListener(CastText);
        }

        if (spell != null)
        {
            spell.text = spellText;
        }

        SetUIVisible(true);

        if (castSpell != null)
        {
            castSpell.ActivateInputField();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (castSpell != null)
        {
            castSpell.onEndEdit.RemoveListener(CastText);
        }

        if (_cast != null && _cast.action != null)
        {
            _cast.action.started -= EvaluateAccuracy;
        }

        SetUIVisible(false);
    }

    private void SetUIVisible(bool visible)
    {
        if (_uiCanvasGroup == null)
        {
            Transform anchor = castSpell != null ? castSpell.transform : (spell != null ? spell.transform : null);
            if (anchor != null) _uiCanvasGroup = anchor.GetComponentInParent<CanvasGroup>();
        }

        if (_uiCanvasGroup != null)
        {
            _uiCanvasGroup.alpha = visible ? 1f : 0f;
            _uiCanvasGroup.interactable = visible;
            _uiCanvasGroup.blocksRaycasts = visible;
        }
    }

    public void CastText(string cast)
    {
        stringIndex = Mathf.Min(cast.Length - 1, spellText.Length - 1);
        stringIndex = Mathf.Max(0, stringIndex);
        _totalKeysPressed++;
        Debug.Log("Current Text: " + cast);

        if (cast.Length == 0) return;
        if (string.IsNullOrEmpty(spellText)) return;

        if (cast[stringIndex] == spellText[stringIndex])
        {
            if (stringIndex <= lastInInput + 1)
            {
                lastInInput = stringIndex;
                if (uiController != null) uiController.UpdateDisplay(lastInInput + 1, false);
            }
        }

        if (cast[stringIndex] != spellText[stringIndex])
        {
            incorrectInput++;
            Debug.Log("Tsijni");
            if (uiController != null) uiController.UpdateDisplay(lastInInput + 1, true);
        }

        if (cast.Length == spellText.Length)
        {
            Debug.Log("Spell Complete");
            enabled = false;
        }
    }

    private void EvaluateAccuracy(InputAction.CallbackContext obj)
    {
        if (Time.unscaledTime < _armingTime) return;
        if (castSpell != null && string.IsNullOrEmpty(castSpell.text)) return;

        _typingStats.timeElapsed = _timeElapsed;
        _typingStats.hits = Mathf.Max(0, (spellText != null ? spellText.Length : 0) - incorrectInput);
        _typingStats.totalKeystrokes = _totalKeysPressed;
        wordsPerMinute = _typingStats.GetWPM();
        accuracy = _typingStats.GetAccuracy();
        _casting = false;

        OnSpellCast?.Invoke(currentSpell);

        enabled = false;
    }

    public IEnumerator CountTimeElapsed()
    {
        while (_casting)
        {
            _timeElapsed++;
            yield return new WaitForSeconds(1f);
        }
    }
}
