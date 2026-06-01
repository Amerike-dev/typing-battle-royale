using UnityEngine;
using TMPro;
using DG.Tweening;

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
        if (result == null)
        {
            Debug.LogWarning($"[PodiumSlot] Result llego null en {name}");
            return;
        }

        Debug.Log($"[PodiumSlot] SetData en {name}: {result.playerName} | Kills {result.kills} | Damage: {result.damageDealt} | WPM {result.wpm}");

        if (nameText != null)
            nameText.text = result.playerName;

        if (killsText != null)
            killsText.text = $"Muertes: {result.kills}";

        if (damageText != null)
            damageText.text = $"Daño: {result.damageDealt:0}";

        if (wpmText != null)
            wpmText.text = $"WPM: {result.wpm:0.0}";
    }

    public void HideStats()
    {
        if (statsCanvasGroup == null) return;

        statsCanvasGroup.alpha = 0f;
        statsCanvasGroup.interactable = false;
        statsCanvasGroup.blocksRaycasts = false;
    }

    public void ShowStats(float duration = 0.4f)
    {
        if (statsCanvasGroup == null) return;

        statsCanvasGroup
            .DOFade(1f, duration)
            .SetEase(Ease.OutCubic);

        statsCanvasGroup.interactable = true;
        statsCanvasGroup.blocksRaycasts = true;
    }
}
