using UnityEngine;
using TMPro;

public class SpellUIController : MonoBehaviour
{
    public CastInputController inputController;
    [Header("References")]
    public TextMeshProUGUI displayText;

    [Header("Colors")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private Color pendingColor = Color.white;

    private string GetHex(Color color) => "#" + ColorUtility.ToHtmlStringRGB(color);

    public void UpdateDisplay(int currentIndex, bool hasError)
    {
        if (inputController == null) inputController = GetComponentInParent<CastInputController>(true);
        if (inputController == null || displayText == null) return;

        string originalText = inputController.spellText;

        if (string.IsNullOrEmpty(originalText))
        {
            displayText.text = string.Empty;
            return;
        }

        string formattedText = "";

        formattedText += $"<color={GetHex(correctColor)}>";
        for (int i = 0; i < currentIndex; i++)
        {
            formattedText += originalText[i];
        }
        formattedText += "</color>";
        
        if (currentIndex < originalText.Length)
        {
            if (hasError)
                formattedText += $"<color={GetHex(incorrectColor)}>{originalText[currentIndex]}</color>";
            else
                formattedText += originalText[currentIndex]; 
        }
        if (currentIndex < originalText.Length)
        {
            formattedText += $"<color={GetHex(pendingColor)}>";
            for (int i = currentIndex + 1; i < originalText.Length; i++)
            {
                formattedText += originalText[i];
            }
            formattedText += "</color>";
        }
        displayText.text = formattedText;
    }
}
