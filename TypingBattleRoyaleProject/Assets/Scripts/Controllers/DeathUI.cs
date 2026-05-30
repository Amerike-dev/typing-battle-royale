using UnityEngine;
using TMPro;

public class DeathUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private TextMeshProUGUI _livesText;

    private void Awake()
    {
        HideInstant();
    }

    public void Show(string killerName, int respawnSeconds, int remainingLives)
    {
        if (_titleText != null)
        {
            _titleText.text = $"Te Elimino {killerName}";
        }

        SetCountdown(respawnSeconds);

        if (_livesText != null)
        {
            _livesText.text = $"Vidas restantes: {remainingLives}";
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha =1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        gameObject.SetActive(true);
    }

    public void SetCountdown(int seconds)
    {
        if (_countdownText != null)
        {
            _countdownText.text = $"Reaparición en {seconds}s";
        }
    }

    public void Hide()
    {
        HideInstant();
    }

    private void HideInstant()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }
}
