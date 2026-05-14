using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI timerText;
    public Slider healthUI;
    public GameObject[] lifeImages;

    public PlayerStatsNet localStats;

    private void Update()
    {
        // Busca continuamente al jugador local hasta que se encuentre.
        if (localStats == null) {
            FindLocalPlayerStats();
        }
    }
    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.gameTimer != null)
        {
            GameManager.Instance.gameTimer.OnSecondElapsed += UpdateTimerUI;
        }

    }
    private void OnDisable()
    {
        UnsubscribeFromStats();
        
        if (GameManager.Instance != null && GameManager.Instance.gameTimer != null)
        {
            GameManager.Instance.gameTimer.OnSecondElapsed -= UpdateTimerUI;
        }
    }

    private void FindLocalPlayerStats()
    {
        // Asegurarse de que el NetworkManager está listo y el cliente local tiene un PlayerObject.
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient || NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            return; // Aún no está listo, se intentará en el próximo frame.
        }

        localStats = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerStatsNet>();

        if (localStats == null)
        {
            // El componente puede no estar listo todavía.
            return;
        }

        Debug.Log("HUDController encontró las estadísticas del jugador local y se suscribe a los eventos.");
        SubscribeToStats();
        RefreshAllUI();
    }

    private void SubscribeToStats()
    {
        if (localStats == null) return;

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
        if (hpText != null) hpText.text = currentHP.ToString("0");

        if (healthUI != null && localStats != null)
        {
            healthUI.value = currentHP / localStats.MaxHP;
        }
    }

    private void UpdateLives(int currentLives)
    {
        if (lifeImages == null) return;

        for (int i = 0; i < lifeImages.Length; i++)
        {
            lifeImages[i].SetActive(i < currentLives);
        }
    }

    private void UpdateKillCount(int kills)
    {
        if (killCountText != null) killCountText.text = kills.ToString();
    }

    private void UpdateTimerUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.gameTimer == null)
            return;

        if (timerText != null)
        {
            timerText.text = $"{GameManager.Instance.gameTimer.MinutesRemaining:D2}:{GameManager.Instance.gameTimer.SecondsRemaining:D2}";
        }
    }
}
