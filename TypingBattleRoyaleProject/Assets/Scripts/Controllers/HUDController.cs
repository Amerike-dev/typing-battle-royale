using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI timerText;
    public Slider healthUI;
    public GameObject[] lifeImages;

    public PlayerStatsNet localStats;

    private void Start()
    {
       FindLocalPlayerStats();
    }
    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.gameTimer != null)
        {
            GameManager.Instance.gameTimer.OnSecondElapsed += UpdateTime;
        }

    }
    private void OnDisable()
    {
        UnsubscribeFromStats();

        if (GameManager.Instance != null && GameManager.Instance.gameTimer != null)
        {
            GameManager.Instance.gameTimer.OnSecondElapsed -= UpdateTime;
        }
    }

    private void FindLocalPlayerStats()
    {
        NetworkPlayerController[] players = FindObjectsByType<NetworkPlayerController>(FindObjectsSortMode.None);

        foreach (NetworkPlayerController player in players)
        {
            if (player.IsOwner)
            {
                localStats = player.GetComponent<PlayerStatsNet>();
                break;
            }
        }

        if (localStats == null)
        {
            Debug.LogWarning("HUDController no encontró PlayerStatsNet del jugador local.");
            return;
        }

        SubscribeToStats();
        RefreshAllUI();
    }

    private void SubscribeToStats()
    {
        localStats.currentHP.OnValueChanged += OnHPChanged;
        localStats.currentLifes.OnValueChanged += OnLivesChanged;
        localStats.killCount.OnValueChanged += OnKillCountChanged;
    }

    private void UnsubscribeFromStats()
    {
        if (localStats == null) return;

        localStats.currentHP.OnValueChanged -= OnHPChanged;
        localStats.currentLifes.OnValueChanged -= OnLivesChanged;
        localStats.killCount.OnValueChanged -= OnKillCountChanged;
    }

    private void OnHPChanged(float oldValue, float newValue)
    {
        UpdateHealth(newValue);
    }

    private void OnLivesChanged(int oldValue, int newValue)
    {
        UpdateLives(newValue);
    }

    private void OnKillCountChanged(int oldValue, int newValue)
    {
        UpdateKillCount(newValue);
    }

    private void RefreshAllUI()
    {
        UpdateHealth(localStats.currentHP.Value);
        UpdateLives(localStats.currentLifes.Value);
        UpdateKillCount(localStats.killCount.Value);
    }

    private void UpdateHealth(float currentHP)
    {
        hpText.text = currentHP.ToString("0");

        if (healthUI != null && localStats != null)
        {
            healthUI.value = currentHP / localStats.MaxHP;
        }
    }

    private void UpdateLives(int currentLives)
    {
        for (int i = 0; i < lifeImages.Length; i++)
        {
            lifeImages[i].SetActive(i < currentLives);
        }
    }

    private void UpdateKillCount(int kills)
    {
        killCountText.text = kills.ToString();
    }

    public void UpdateTime()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameTimer == null)
            return;

        timerText.text = $"{GameManager.Instance.gameTimer.MinutesRemaining}:{GameManager.Instance.gameTimer.SecondsRemaining}";
    }
}
