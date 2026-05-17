using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class CastInputController : MonoBehaviour
{
    public event Action<Spell> OnSpellCast;
    public event Action OnCastCancelled;

    [Header("UI References")]
    public TMP_InputField castSpell;
    public TextMeshProUGUI spell;
    public CanvasGroup uiCanvasGroup;
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Spells")]
    public Spell currentSpell;
    public Spell defaultSpell;
    public string spellText;

    [Header("VFX")]
    public Transform castOrigin;

    [Header("Typing Stats")]
    [SerializeField] private InputActionReference _cast;
    private TypingStats _typingStats;
    private float _timeElapsed;
    public int _totalKeysPressed;
    public float wordsPerMinute;
    public float accuracy;

    [Header("View Feedback")]
    public SpellUIController uiController;

    public int stringIndex;
    public int lastInInput;
    public int incorrectInput;

    private bool _casting;
    private bool _completed;
    private float _armingTime;
    private Coroutine _fadeRoutine;

    private float _castStartTime;

    private void OnEnable()
    {
        ResolveReferences();
        ResetCasting();

        _armingTime = Time.unscaledTime + 0.15f;

        if (_cast != null && _cast.action != null)
        {
            _cast.action.started += EvaluateAccuracy;
        }

        if (currentSpell == null && defaultSpell != null) currentSpell = defaultSpell;
        if (string.IsNullOrEmpty(spellText) && currentSpell != null) spellText = currentSpell.runeString;

        if (spell != null) spell.text = spellText;
        if (uiController != null) uiController.UpdateDisplay(0, false);

        if (castSpell != null)
        {
            castSpell.onValueChanged.AddListener(HandleValueChanged);
            castSpell.onEndEdit.AddListener(HandleEndEdit);
        }

        StartCoroutine(CountTimeElapsed());
        FadeTo(1f, null);
        StartCoroutine(FocusInputNextFrame());
    }

    private IEnumerator FocusInputNextFrame()
    {
        yield return null;
        if (!enabled || castSpell == null) yield break;

        if (!castSpell.gameObject.activeInHierarchy)
        {
            Transform t = castSpell.transform;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                t = t.parent;
            }
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(castSpell.gameObject);
        }
        castSpell.ActivateInputField();
        castSpell.Select();
        castSpell.caretPosition = castSpell.text != null ? castSpell.text.Length : 0;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _fadeRoutine = null;
        _casting = false;

        if (castSpell != null)
        {
            castSpell.onValueChanged.RemoveListener(HandleValueChanged);
            castSpell.onEndEdit.RemoveListener(HandleEndEdit);
            castSpell.DeactivateInputField();
        }

        if (_cast != null && _cast.action != null)
        {
            _cast.action.started -= EvaluateAccuracy;
        }

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (_completed) return;
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            CancelCast();
        }
    }

    private void ResolveReferences()
    {
        if (castSpell == null) castSpell = GetComponentInChildren<TMP_InputField>(true);
        if (uiController == null) uiController = GetComponentInChildren<SpellUIController>(true);

        if (spell == null)
        {
            if (uiController != null && uiController.displayText != null)
            {
                spell = uiController.displayText;
            }
            else
            {
                Transform spellUiRoot = FindSpellUiRoot();
                if (spellUiRoot != null)
                {
                    foreach (var t in spellUiRoot.GetComponentsInChildren<TextMeshProUGUI>(true))
                    {
                        if (t == null) continue;
                        if (castSpell != null && t.transform.IsChildOf(castSpell.transform)) continue;
                        spell = t;
                        break;
                    }
                }
            }
        }

        if (uiController != null && uiController.inputController == null)
        {
            uiController.inputController = this;
        }
        if (uiController != null && uiController.displayText == null && spell != null)
        {
            uiController.displayText = spell;
        }

        if (uiCanvasGroup == null)
        {
            Transform anchor = castSpell != null ? castSpell.transform : (spell != null ? spell.transform : null);
            if (anchor != null)
            {
                var groups = anchor.GetComponentsInParent<CanvasGroup>(true);
                foreach (var g in groups)
                {
                    if (g != null && g.name.IndexOf("Spell", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        uiCanvasGroup = g;
                        break;
                    }
                }
                if (uiCanvasGroup == null && groups != null && groups.Length > 0)
                    uiCanvasGroup = groups[groups.Length - 1];
            }
        }

        if (uiCanvasGroup != null)
        {
            Transform t = uiCanvasGroup.transform;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                if (t.GetComponent<Canvas>() != null) break;
                t = t.parent;
            }
        }
    }

    private Transform FindSpellUiRoot()
    {
        if (uiCanvasGroup != null) return uiCanvasGroup.transform;
        var groups = GetComponentsInChildren<CanvasGroup>(true);
        foreach (var g in groups)
        {
            if (g != null && g.name.IndexOf("Spell", StringComparison.OrdinalIgnoreCase) >= 0)
                return g.transform;
        }
        if (castSpell != null) return castSpell.transform.parent;
        return null;
    }

    private void ResetCasting()
    {
        _castStartTime = Time.time;
        _casting = true;
        _completed = false;
        _typingStats = new TypingStats();
        wordsPerMinute = 0;
        accuracy = 0;
        stringIndex = 0;
        lastInInput = 0;
        incorrectInput = 0;
        _totalKeysPressed = 0;
        _timeElapsed = 0;

        if (castSpell != null)
        {
            castSpell.onValueChanged.RemoveListener(HandleValueChanged);
            castSpell.onEndEdit.RemoveListener(HandleEndEdit);
            castSpell.text = string.Empty;
        }

        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }
    }

    private void HandleValueChanged(string typed)
    {
        if (_completed || string.IsNullOrEmpty(spellText)) return;

        _totalKeysPressed++;
        int length = typed.Length;

        int matched = 0;
        bool error = false;
        for (int i = 0; i < length && i < spellText.Length; i++)
        {
            if (typed[i] == spellText[i]) matched = i + 1;
            else { error = true; break; }
        }
        lastInInput = matched;

        if (error) incorrectInput++;

        if (uiController != null) uiController.UpdateDisplay(matched, error);

        if (!error && length == spellText.Length && matched == spellText.Length)
        {
            CompleteCast();
        }
    }

    private void HandleEndEdit(string typed)
    {
        if (_completed) return;
        if (castSpell != null && castSpell.isActiveAndEnabled) castSpell.ActivateInputField();
    }

    private void CompleteCast()
    {
        if (_completed) return;
        _completed = true;
        _casting = false;

        //_typingStats.timeElapsed = _timeElapsed;
        float elapsedSeconds = Time.time - _castStartTime;
        float elapsedMinutes = Mathf.Max(0.001f, elapsedSeconds / 60f);
        
        wordsPerMinute = (_totalKeysPressed / 5f) / elapsedMinutes; 
        _typingStats.hits = Mathf.Max(0, (spellText != null ? spellText.Length : 0) - incorrectInput);
        _typingStats.totalKeystrokes = _totalKeysPressed;
        //wordsPerMinute = _typingStats.GetWPM();
        accuracy = _typingStats.GetAccuracy();

        ApplyDamageToLockedTarget();

        OnSpellCast?.Invoke(currentSpell);

        FadeTo(0f, () =>
        {
            enabled = false;
            RequestExitBattle();
        });
    }

    private void ApplyDamageToLockedTarget()
    {
        var gm = GameplayManager.Instance;
        if (gm == null) return;

        TargetSystem targetSystem = gm.TargetSystem;

        if (targetSystem == null) return;
        if (currentSpell == null) return;

        float damage = currentSpell.damage;

        ulong attackerId = NetworkManager.Singleton != null
            ? NetworkManager.Singleton.LocalClientId
            : 0;

        Debug.Log($"[CastInputController] Spell completado. Aplicando daño={damage}, attackerId={attackerId}, spell={currentSpell.spellName}");

        targetSystem.ApplyDamageToCurrentTarget(damage, attackerId);
    }

    private void CancelCast()
    {
        if (_completed) return;
        _completed = true;
        _casting = false;

        OnCastCancelled?.Invoke();

        FadeTo(0f, () =>
        {
            enabled = false;
            RequestExitBattle();
        });
    }

    private void RequestExitBattle()
    {
        var gm = GameplayManager.Instance;
        if (gm == null || gm.stateMachine == null) return;
        if (gm.explorationState == null) return;
        if (gm.PlayerController != null) gm.PlayerController.onExplorationState = true;
        gm.stateMachine.ChangeState(gm.explorationState);
    }

    private void EvaluateAccuracy(InputAction.CallbackContext obj)
    {
        if (_completed) return;
        if (Time.unscaledTime < _armingTime) return;
        if (castSpell != null && string.IsNullOrEmpty(castSpell.text)) return;
        CompleteCast();
    }

    public IEnumerator CountTimeElapsed()
    {
        while (_casting)
        {
            _timeElapsed++;
            yield return new WaitForSeconds(1f);
        }
    }

    private void FadeTo(float target, Action onComplete = null)
    {
        if (uiCanvasGroup == null)
        {
            onComplete?.Invoke();
            return;
        }
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(target, onComplete));
    }

    private IEnumerator FadeRoutine(float target, Action onComplete)
    {
        bool show = target > 0.5f;
        uiCanvasGroup.interactable = show;
        uiCanvasGroup.blocksRaycasts = show;

        float start = uiCanvasGroup.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.0001f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            uiCanvasGroup.alpha = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        uiCanvasGroup.alpha = target;
        _fadeRoutine = null;
        onComplete?.Invoke();
    }
}
