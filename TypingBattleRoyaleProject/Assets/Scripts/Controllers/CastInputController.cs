using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CastInputController : MonoBehaviour
{
    public PlayerUI playerUI;
    public int incorrectInput = 0;
    public int lastInInput = 0;
    public int BInput = 0;
    public string spellText;

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
        CombatLogic.SetText(spellText);
        _errorCount = 0;
        Keyboard.current.onTextInput += TextInput;
    }

    private void OnDisable()
    {
        Keyboard.current.onTextInput -= TextInput;
    }

    private void Update()
    {
        BackspaceLogic();
    }
    private void BackspaceLogic()
    {
        var backspaceKey = Keyboard.current.backspaceKey;

        if (backspaceKey.wasPressedThisFrame)
        {
            _backSpaceTimer = _backSpaceDelay;
            BackspaceBehaviour();
        }
        else if (backspaceKey.isPressed) //Esto es para que la UI no implosione. Es un contador que se vale de Update por si se deja presionado
        {
            _backSpaceTimer -= Time.deltaTime;

            if (_backSpaceTimer <= 0)
            {
                _backSpaceTimer = _backSpaceCooldown;
                BackspaceBehaviour();
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
    }

    private void TextInput(char input)
    {
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
    }

}
