using UnityEngine;
using TMPro;
public class SpectatorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private TextMeshProUGUI _targetText;

    private void Awake()
    {
        Hide();
    }

    public void Show()
    {
        if(_canvasGroup != null)        
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        if(_messageText != null)
        {
            _messageText.text = "Estas muerto amigo. \nCon flechas cambias la vista a otro jugador.";
        }

        SetTargetName("Buscando jugador vivo...");
    }

    public void Hide()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }

    public void SetTargetName(string name)
    {
        if(_targetText != null)
        {
            _targetText.text = $"Viendo a: {name}";
        }
    }
}
