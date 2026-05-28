using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private TextMeshProUGUI _psAvgWordsPerMinuteText;
    [SerializeField] private TextMeshProUGUI _psAvgAccuracyText;
    [SerializeField] private TextMeshProUGUI _psBestSpellText;
    [SerializeField] private TextMeshProUGUI _psFastestCastText;

    [Header("UIanimation")]
    [SerializeField] RectTransform _panelUI;
    [SerializeField] CanvasGroup _canvasGroup;
    [SerializeField] Vector2 _showPos = new Vector2(0, 0);
    [SerializeField] float _time = 0.5f;
    [SerializeField] bool _advancedStats = false;
    private Coroutine _moveRoutine;

    [SerializeField] RectTransform _basicPanelUI;
    [SerializeField] CanvasGroup _basicCanvasGroup;
    [SerializeField] Vector2 _basicShowPos = new Vector2(0, 0);
    [SerializeField] Vector2 _basicHidePos = new Vector2(0, -80);
    private Coroutine _basicMoveRoutine;

    [SerializeField] RectTransform _advancedPanelUI;
    [SerializeField] CanvasGroup _advancedCanvasGroup;
    [SerializeField] Vector2 _advancedShowPos = new Vector2(0, 0);
    [SerializeField] Vector2 _advancedHidePos = new Vector2(0, -80);
    private Coroutine _advancedMoveRoutine;

    [SerializeField] RectTransform _midlePanelUI;
    [SerializeField] CanvasGroup _midleCanvasGroup;
    [SerializeField] Vector2 _midleShowPos;
    [SerializeField] Vector2 _midleHidePos;
    private Coroutine _midleMoveRoutine;

    [SerializeField] RectTransform _leftContainer;
    [SerializeField] RectTransform _rightContainer;
    [SerializeField] float _showHeight = 120f;
    [SerializeField] float _hideHeight = 350f;
    private Coroutine _leftContainerRoutine;
    private Coroutine _rightContainerRoutine;

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
        if (_statsText != null)
            _statsText.text = BuildStats(players);

    }

    public void PopulatePersonalStats(PlayerStatsNet localPlayerStats)
    {
        if (localPlayerStats == null) return;
        
        if (_personalStatsContainer != null) 
            _personalStatsContainer.SetActive(true);

        DOTween.To(() => 0f, x => _psKillsText.text = x.ToString("F0"), localPlayerStats.killCount.Value, 1.5f)
            .SetEase(Ease.OutCubic).SetUpdate(true);

        DOTween.To(() => 0f, x => _psDamageDealtText.text = x.ToString("F0"), localPlayerStats.damageDealt.Value, 1.5f)
            .SetEase(Ease.OutCubic).SetUpdate(true);

        DOTween.To(() => 0f, x => _psDamageTakenText.text = x.ToString("F0"), localPlayerStats.damageTaken.Value, 1.5f)
            .SetEase(Ease.OutCubic).SetUpdate(true);

        DOTween.To(() => 0f, x => _psSpellsCastText.text = x.ToString("F0"), localPlayerStats.spellsCast.Value, 1.5f)
            .SetEase(Ease.OutCubic).SetUpdate(true);

        DOTween.To(() => 0f, x => _psAvgWordsPerMinuteText.text = x.ToString("F1"), localPlayerStats.avgWpm, 1.5f)
            .SetEase(Ease.OutCubic).SetUpdate(true);

        DOTween.To(() => 0f, x => _psAvgAccuracyText.text = x.ToString("F1") + "%", localPlayerStats.avgAccuracy, 1.5f)
            .SetEase(Ease.OutCubic).SetUpdate(true);

        string bestSpell = "None";
        if (localPlayerStats.spellUsageCount.Count > 0)
        {
            bestSpell = localPlayerStats.spellUsageCount.OrderByDescending(kv => kv.Value).First().Key;
        }
        _psBestSpellText.text = bestSpell;

        _psFastestCastText.text = localPlayerStats.fastestCastSeconds == float.MaxValue ? "N/A" : $"{localPlayerStats.fastestCastSeconds:F2}s";
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("LobbyScene");
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
    public void Show()
    {
        UIMove(_showPos);
        UIAnimator.FadeIn(_canvasGroup, _time);
    }
    public void ChangeUIStats()
    {
        if (_advancedStats) HideBotton();
        else ShowBotton();
    }
    public void ShowBotton()
    {
        _advancedStats = true;
        BasicUIMove(_basicShowPos);
        MidleUIMove(_midleHidePos);
        AdvancedUIMove(_advancedShowPos);
        UIChangeHeightLeft(_showHeight);
        UIChangeHeightRight(_showHeight);
        UIAnimator.FadeIn(_advancedCanvasGroup, _time);
    }
    public void HideBotton()
    {
        _advancedStats = false;
        BasicUIMove(_basicHidePos);
        MidleUIMove(_midleHidePos);
        AdvancedUIMove(_advancedHidePos);
        UIChangeHeightLeft(_hideHeight);
        UIChangeHeightRight(_hideHeight);
        UIAnimator.FadeOut(_advancedCanvasGroup, _time);
    }
    public void UIMove(Vector2 target)
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
        }
        _moveRoutine = StartCoroutine(UIAnimator.PanelUIMove(_panelUI, target, _time));
    }

    public void BasicUIMove(Vector2 target)
    {
        if (_basicMoveRoutine != null)
        {
            StopCoroutine(_basicMoveRoutine);
        }
        _basicMoveRoutine = StartCoroutine(UIAnimator.PanelUIMove(_basicPanelUI, target, _time));
    }
    public void MidleUIMove(Vector2 target)
    {
        if (_midleMoveRoutine != null)
        {
            StopCoroutine(_midleMoveRoutine);
        }
        _midleMoveRoutine = StartCoroutine(UIAnimator.PanelUIMove(_midlePanelUI, target, _time));
    }
    public void AdvancedUIMove(Vector2 target)
    {
        if (_advancedMoveRoutine != null)
        {
            StopCoroutine(_advancedMoveRoutine);
        }
        _advancedMoveRoutine = StartCoroutine(UIAnimator.PanelUIMove(_advancedPanelUI, target, _time));
    }
    public void UIChangeHeightLeft(float target)
    {
        if (_leftContainerRoutine != null)
        {
            StopCoroutine(_leftContainerRoutine);
        }
        _leftContainerRoutine = StartCoroutine(UIAnimator.HeightUIChange(_leftContainer, target, _time));
    }
    public void UIChangeHeightRight(float target)
    {
        if (_rightContainerRoutine != null)
        {
            StopCoroutine(_rightContainerRoutine);
        }
        _rightContainerRoutine = StartCoroutine(UIAnimator.HeightUIChange(_rightContainer, target, _time));
    }
}