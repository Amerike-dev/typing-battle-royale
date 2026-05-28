using UnityEngine;
using TMPro;

public class MonolithUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI displayText;

    [Header("Colors")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color incorrectColor = Color.red;
    [SerializeField] private Color pendingColor = Color.white;

    private string GetHex(Color color) => "#" + ColorUtility.ToHtmlStringRGB(color);

    public void UpdateDisplay(string runeText, int currentIndex, bool hasError)
    {
        if (displayText == null) return;
        
        if (string.IsNullOrEmpty(runeText))
        {
            displayText.text = "";
            return;
        }
        
        int safeIndex = Mathf.Clamp(currentIndex, 0, runeText.Length);

        string formattedText = "";
        
        if (safeIndex > 0)
        {
            formattedText += $"<color={GetHex(correctColor)}>{runeText.Substring(0, safeIndex)}</color>";
        }
        
        if (safeIndex < runeText.Length)
        {
            string currentColor = hasError ? GetHex(incorrectColor) : GetHex(pendingColor);
            formattedText += $"<color={currentColor}>{runeText[safeIndex]}</color>";
        }
        
        if (safeIndex + 1 < runeText.Length)
        {
            formattedText += $"<color={GetHex(pendingColor)}>{runeText.Substring(safeIndex + 1)}</color>";
        }

        displayText.text = formattedText;
    }
}
