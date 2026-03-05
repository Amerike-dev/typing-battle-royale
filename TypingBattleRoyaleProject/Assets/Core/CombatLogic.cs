using UnityEngine;

public static class CombatLogic
{

    public static string SpellText;

    private static int _stringIndex = 0;


    public static void SetText(string spellText)
    {
        SpellText = spellText;
    }

    public static void ValidateCharacter()
    {
        foreach (char c in Input.inputString)
        {
            Debug.Log("Index no: " + _stringIndex);

            if (c == "\b"[0])
            {
                _stringIndex--;
                continue;
            }

            if (c == "\n"[0] || c == "\r"[0])
            {
                Debug.Log("Stop typing");
                break;
            }

            if (c == SpellText[_stringIndex])
            {
                Debug.Log(c);
            }
            else if (c != SpellText[_stringIndex])
            {
                Debug.Log("Error");
            }

            _stringIndex++;

        }
    }
}
