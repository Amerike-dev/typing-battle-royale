using System;
using UnityEditor;
using UnityEngine;

public static class CombatLogic
{
    /*public static string SpellText;
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
    }*/

    public static string SpellText;
    public static string UserInput = "";

    public static void SetText(string text)
    {
        SpellText = text;
        UserInput = "";
    }

    public static int AddChar(char c)
    {
        if (string.IsNullOrEmpty(SpellText))
            return -1;

        if (UserInput.Length < SpellText.Length)
        {
            UserInput += c;
            return UserInput.Length - 1;
        }

        return -1;
    }

    public static void RemoveChar()
    {
        if (UserInput.Length > 0)
            UserInput = UserInput.Substring(0, UserInput.Length - 1);
    }

    public static bool IsComplete()
    {
        return UserInput == SpellText;
    }

    public static int CurrentIndex()
    {
        return UserInput.Length;
    }

    public static bool IsCorrectAt(int index)
    {
        if (index >= UserInput.Length) return true;
        return UserInput[index] == SpellText[index];
    }
}
