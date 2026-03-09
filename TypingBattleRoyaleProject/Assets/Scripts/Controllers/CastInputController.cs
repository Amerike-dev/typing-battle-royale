using UnityEngine;
using UnityEngine.InputSystem;

public class CastInputController : MonoBehaviour
{
    public string spellText;

    private void OnEnable()
    {
        CombatLogic.SetText(spellText);
        Keyboard.current.onTextInput += TextInput;
    }

    private void OnDisable()
    {
        Keyboard.current.onTextInput -= TextInput;
    }

    private void Update()
    {
        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            CombatLogic.EraseChar();
            Debug.Log("Current Index now is: " + CombatLogic.CurrentIndex());
        }
    }

    private void TextInput(char input)
    {
        bool typed = CombatLogic.ValidateCharacter(input);

        if (input == '\n' || input == '\r') return;
        if (!typed)
        {
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
}
