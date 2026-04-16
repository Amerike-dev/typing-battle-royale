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
        _stringIndex = 0; //Este valor no necesita estar aqui
    }
    //Este metodo es innecesario
    public static int CurrentIndex() => _stringIndex;
    //Este metodo no se ocupa en ningun lugar
    public static void SetIndex(int index)
    {
        _stringIndex = Mathf.Clamp(index, 0, SpellText.Length);
    }
    //Este metodo es innecesario
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
