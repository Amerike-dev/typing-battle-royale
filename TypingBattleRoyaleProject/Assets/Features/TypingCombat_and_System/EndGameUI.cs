using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private TextMeshProUGUI _statsText;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _mainMenuButton;

    [Header("UIanimation")]
    [SerializeField] RectTransform _panelUI;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Vector2 _hidePos;
    [SerializeField] Vector2 _showPos;
    [SerializeField] float _time = 0.2f;
    Coroutine _animationCoroutine;

    private void Awake()
    {
        if (_playAgainButton != null)
            _playAgainButton.onClick.AddListener(() => SceneLoader.Reload());

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(() => SceneLoader.LoadScene("LobbyScene"));

        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        ShowAnimation();
    }

    public void Populate(string winnerId, List<PlayerStatsNet> players)
    {
        if (_winnerText != null)
            _winnerText.text = winnerId;

        if (_statsText != null)
            _statsText.text = BuildStats(players);

    }

    private string BuildStats(List<PlayerStatsNet> players)
    {
        if (players == null || players.Count == 0)
            return string.Empty;

        StringBuilder sb = new StringBuilder();

        foreach (PlayerStatsNet player in players)
        {
            if (player == null)
            {
                sb.AppendLine("Desconectado");
                continue;
            }
            string displayId = player.ID;
            
            if (player.TryGetComponent<IDController>(out var idController))
            {
                string customName = idController.playerName.Value.ToString();
                
                if (!string.IsNullOrEmpty(customName)) displayId = customName;
            }
            sb.AppendLine($"{displayId} | Kills: {player.killCount.Value} | WPM: {player.wPM.Value:0.0}");
        }
        return sb.ToString();
    }

    private void ShowAnimation()
    {
        _panelUI.anchoredPosition = _hidePos;
        _canvasGroup.alpha = 0f;

        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        _animationCoroutine = StartCoroutine(UIAnimator.PanelUIMove(_panelUI, _showPos, _time));

        UIAnimator.FadeIn(_canvasGroup, _time);
    }
}