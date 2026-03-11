using UnityEditor;
using UnityEngine;

public static class CombatLogic
{
    public static string SpellText;
    public static bool spellComplete => _stringIndex >= SpellText.Length;

    public static int _stringIndex = 0;

    public static void SetText(string spellText)
    {
        SpellText = spellText;
        _stringIndex = 0;
    }

    public static int CurrentIndex() => _stringIndex;

    public static void EraseChar()
    {
        if (_stringIndex > 0)
        {
            _stringIndex--;
        }
    }

    public static bool ValidateCharacter(char input)
    {
        if (input == SpellText[_stringIndex])
        {
            _stringIndex++;

            return true;
        }

        return false;
    }
}
