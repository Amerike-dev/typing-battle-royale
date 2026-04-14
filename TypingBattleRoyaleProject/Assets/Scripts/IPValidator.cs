using TMPro;
using UnityEngine;

public class IPValidator : MonoBehaviour
{
    [Header("Input field")]
    public TMP_InputField inputField;

    void Start()
    {
        if (inputField != null) inputField.onValidateInput += ValidateIPCharacter;
    }

    private char ValidateIPCharacter(string text, int charIndex, char addedChar)
    {
        if (char.IsDigit(addedChar) || addedChar == '.') return addedChar;

        return '\0';
    }
}
