using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class EndGameUI : MonoBehaviour
{
    [Header("General Match Stats")]
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private TextMeshProUGUI _statsText;

    [Header("Buttons")]
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _mainMenuButton;

    [Header("Personal Stats Panel")]
    [SerializeField] private GameObject _personalStatsContainer;
    [SerializeField] private TextMeshProUGUI _psKillsText;
    [SerializeField] private TextMeshProUGUI _psDamageDealtText;
    [SerializeField] private TextMeshProUGUI _psDamageTakenText;
    [SerializeField] private TextMeshProUGUI _psSpellsCastText;
    [SerializeField] private TextMeshProUGUI _psAvgWPMText;
    [SerializeField] private TextMeshProUGUI _psAvgAccuracyText;
    [SerializeField] private TextMeshProUGUI _psBestSpellText;
    [SerializeField] private TextMeshProUGUI _psFastestCastText;

    private void Awake()
    {
        if (_playAgainButton != null)
            _playAgainButton.onClick.AddListener(() => SceneLoader.Reload());

        if (_mainMenuButton != null)
            _mainMenuButton.onClick.AddListener(() => SceneLoader.LoadScene("LobbyScene"));

        gameObject.SetActive(false);
    }

    public void Populate(string winnerId, List<PlayerStatsNet> players)
    {
        if (_winnerText != null)
            _winnerText.text = winnerId;

        if (_statsText != null)
            _statsText.text = BuildStats(players);

    }

    public void PopulatePersonalStats(PlayerStatsNet localPlayerStats)
    {
        if (localPlayerStats == null) return;
        
        if (_personalStatsContainer != null) 
            _personalStatsContainer.SetActive(true);

        DOTween.To(() => 0, x => _psKillsText.text = x.ToString(), localPlayerStats.killCount.Value, 1.5f)
            .SetEase(Ease.OutCubic);

        DOTween.To(() => 0f, x => _psDamageDealtText.text = x.ToString("F0"), localPlayerStats.damageDealt.Value, 1.5f)
            .SetEase(Ease.OutCubic);

        DOTween.To(() => 0f, x => _psDamageTakenText.text = x.ToString("F0"), localPlayerStats.damageTaken.Value, 1.5f)
            .SetEase(Ease.OutCubic);

        DOTween.To(() => 0, x => _psSpellsCastText.text = x.ToString(), localPlayerStats.spellsCast.Value, 1.5f)
            .SetEase(Ease.OutCubic);

        DOTween.To(() => 0f, x => _psAvgWPMText.text = x.ToString("F1"), localPlayerStats.avgWpm, 1.5f)
            .SetEase(Ease.OutCubic);

        DOTween.To(() => 0f, x => _psAvgAccuracyText.text = x.ToString("F1") + "%", localPlayerStats.avgAccuracy, 1.5f)
            .SetEase(Ease.OutCubic);

        string bestSpell = "None";
        if (localPlayerStats.spellUsageCount.Count > 0)
        {
            bestSpell = localPlayerStats.spellUsageCount.OrderByDescending(kv => kv.Value).First().Key;
        }
        _psBestSpellText.text = bestSpell;

        _psFastestCastText.text = localPlayerStats.fastestCastSeconds == float.MaxValue ? "N/A" : $"{localPlayerStats.fastestCastSeconds:F2}s";
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
}