using TMPro;
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

    private void Start()
    {
       for(int i = 0; i < player.stats.maxLives; i++)
        {
            lifeImages[i].SetActive(true);
        }
    }
    private void OnEnable()
    {
        player.stats.OnLifeLost += UpdateLives;
        player.stats.OnDamageTaken += UpdateHealth;
        player.stats.OnEnemyKilled += UpdateKillCount;
        GameManager.Instance.gameTimer.OnSecondElapsed += UpdateTime;

    }
    private void OnDisable()
    {
        player.stats.OnLifeLost -= UpdateLives;
        player.stats.OnDamageTaken -= UpdateHealth;
        player.stats.OnEnemyKilled -= UpdateKillCount;
        GameManager.Instance.gameTimer.OnSecondElapsed -= UpdateTime;
    }
    public void UpdateLives()
    {
        for (int i = lifeImages.Length; i > player.stats.currentLifes; i--)
        {
            lifeImages[i].SetActive(false);
        }
    }
    public void UpdateHealth()
    {
        hpText.text = player.stats.currentHP.ToString();
    }
    public void UpdateKillCount()
    {
        killCountText.text = player.stats.killCount.ToString();
    }

    public void UpdateTime()
    {
        timerText.text = $"{GameManager.Instance.gameTimer.MinutesRemaining}:{GameManager.Instance.gameTimer.SecondsRemaining}";
    }
}
