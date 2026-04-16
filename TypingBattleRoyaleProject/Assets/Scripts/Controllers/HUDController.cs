using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI timerText;
    public Slider healthUI;
    public GameObject[] lifeImages;

    private void OnEnable()
    {
        player.stats.OnLifeLost += UpdateLives;
        player.stats.OnDamageTaken += UpdateHealth;
        player.stats.OnEnemyKilled += UpdateKillCount;
        //player.stats.
    }
    private void OnDisable()
    {
        
    }
    public void UpdateLives()
    {

    }
    public void UpdateHealth()
    {
        
    }
    public void UpdateKillCount()
    {
        
    }

    public void UpdateTime()
    {
        
    }
}
