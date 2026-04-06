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

    private void Awake()
    {
        if (_playAgainButton != null)
            _playAgainButton.onClick.AddListener(() => SceneLoader.Reload());

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(() => SceneLoader.LoadScene("MainMenu"));

        gameObject.SetActive(false);
    }

    public void Populate(string winnerId, List<PlayerStats> players)
    {
        if (_winnerText != null)
            _winnerText.text = winnerId;

        if (_statsText != null)
            _statsText.text = BuildStats(players);

    }

    private string BuildStats(List<PlayerStats> players)
    {
        if (players == null || players.Count == 0)
            return string.Empty;

        StringBuilder sb = new StringBuilder();

        foreach (PlayerStats player in players)
        {
            if (player == null) continue;

            sb.AppendLine($"{player.ID} | Kills: {player.KillCount} | WPM: {player.WPM:0.0}");
        }

        return sb.ToString();
    }
}