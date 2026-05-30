using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PodiumController : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private PodiumSlot firstPlaceSlot;
    [SerializeField] private PodiumSlot secondPlaceSlot;
    [SerializeField] private PodiumSlot thirdPlaceSlot;

    [Header("Player Visual")]
    [SerializeField] private GameObject defaultPlayerVisualPrefab;

    [Header("Effects")]
    [SerializeField] private ParticleSystem confettiFirstPlace;

    [Header("Buttons Canvas")]
    [SerializeField] private CanvasGroup buttonsCanvasGroup;

    [Header("Animation")]
    [SerializeField] private float pedestalStartOffsetY = -4f;
    [SerializeField] private float riseDuration = 0.7f;
    [SerializeField] private float delayBetweenReveals = 0.7f;

    private void Start()
    {
        HideInitialState();
        StartCoroutine(RevealPodiumRoutine());
    }

    private void HideInitialState()
    {
        HideSlot(firstPlaceSlot);
        HideSlot(secondPlaceSlot);
        HideSlot(thirdPlaceSlot);

        if (buttonsCanvasGroup != null)
        {
            buttonsCanvasGroup.alpha = 0f;
            buttonsCanvasGroup.interactable = false;
            buttonsCanvasGroup.blocksRaycasts = false;
        }

        if (confettiFirstPlace != null)
            confettiFirstPlace.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private IEnumerator RevealPodiumRoutine()
    {
        List<PodiumPlayerResult> ranked = EndGameResultsData.RankedPlayers;

        if (ranked == null || ranked.Count == 0)
        {
            Debug.LogWarning("[PodiumController] No hay resultados para mostrar.");
            yield break;
        }

        if (ranked.Count >= 3)
            yield return RevealSlot(thirdPlaceSlot, ranked[2]);

        if (ranked.Count >= 2)
            yield return RevealSlot(secondPlaceSlot, ranked[1]);

        if (ranked.Count >= 1)
        {
            yield return RevealSlot(firstPlaceSlot, ranked[0]);

            if (confettiFirstPlace != null)
                confettiFirstPlace.Play();
        }

        ShowButtons();
    }

    private IEnumerator RevealSlot(PodiumSlot slot, PodiumPlayerResult result)
    {
        if (slot == null || result == null)
            yield break;

        slot.gameObject.SetActive(true);
        slot.SetData(result);

        SpawnPlayerVisual(slot);

        Transform pedestal = slot.Pedestal;
        Vector3 finalPosition = pedestal.localPosition;
        Vector3 startPosition = finalPosition + Vector3.up * pedestalStartOffsetY;

        pedestal.localPosition = startPosition;

        if (slot.StatsCanvasGroup != null)
        {
            slot.StatsCanvasGroup.alpha = 0f;
        }

        pedestal
            .DOLocalMove(finalPosition, riseDuration)
            .SetEase(Ease.OutBack);

        yield return new WaitForSeconds(riseDuration * 0.7f);

        if (slot.StatsCanvasGroup != null)
        {
            slot.StatsCanvasGroup
                .DOFade(1f, 0.4f)
                .SetEase(Ease.OutCubic);
        }

        yield return new WaitForSeconds(delayBetweenReveals);
    }

    private void SpawnPlayerVisual(PodiumSlot slot)
    {
        if (defaultPlayerVisualPrefab == null || slot.PlayerSpawnPoint == null)
            return;

        Instantiate(
            defaultPlayerVisualPrefab,
            slot.PlayerSpawnPoint.position,
            slot.PlayerSpawnPoint.rotation,
            slot.PlayerSpawnPoint
        );
    }

    private void HideSlot(PodiumSlot slot)
    {
        if (slot == null) return;

        slot.gameObject.SetActive(false);
    }

    private void ShowButtons()
    {
        if (buttonsCanvasGroup == null)
            return;

        buttonsCanvasGroup
            .DOFade(1f, 0.5f)
            .SetEase(Ease.OutCubic);

        buttonsCanvasGroup.interactable = true;
        buttonsCanvasGroup.blocksRaycasts = true;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameplayScene");
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LobbyScene");
    }
}
