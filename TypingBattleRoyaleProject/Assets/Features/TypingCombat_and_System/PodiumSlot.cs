using UnityEngine;
using TMPro;

public class PodiumSlot : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform pedestal;
    [SerializeField] private CanvasGroup statsCanvasGroup;

    [Header("Texts")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text wpmText;

    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public Transform Pedestal => pedestal;
    public CanvasGroup StatsCanvasGroup => statsCanvasGroup;

    public void SetData(PodiumPlayerResult result)
    {
        nameText.text = result.playerName;
        killsText.text = $"Kills: {result.kills}";
        damageText.text = $"Damage: {result.damageDealt:0}";
        wpmText.text = $"WPM: {result.wpm:0.0}";
    }
}
