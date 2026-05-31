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
    [SerializeField] private SkinInfo[] arraySkins;
    [SerializeField] private GameObject fallbackPrefab;

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
        PrepareSlot(firstPlaceSlot);
        PrepareSlot(secondPlaceSlot);
        PrepareSlot(thirdPlaceSlot);

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
            yield return RevealSlot(thirdPlaceSlot, ranked[2], 2);

        if (ranked.Count >= 2)
            yield return RevealSlot(secondPlaceSlot, ranked[1], 1);

        if (ranked.Count >= 1)
        {
            yield return RevealSlot(firstPlaceSlot, ranked[0], 0);

            if (confettiFirstPlace != null)
                confettiFirstPlace.Play();
        }

        ShowButtons();
    }

    private IEnumerator RevealSlot(PodiumSlot slot, PodiumPlayerResult result, int rankIndex)
    {
        if (slot == null || result == null)
            yield break;

        slot.gameObject.SetActive(true);
        slot.SetData(result);

        SpawnPlayerVisual(slot, result, rankIndex);

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

        slot.ShowStats(0.4f);

        yield return new WaitForSeconds(delayBetweenReveals);
    }

    private GameObject SpawnPlayerVisual(PodiumSlot slot, PodiumPlayerResult result, int rankIndex)
    {
        if (slot == null || result == null) return null;

        if (slot.PlayerSpawnPoint == null)
        {
            Debug.LogWarning("[PodiumController] El slot no tiene PlayerSpawnPoint asignado.");
            return null;
        }

        SkinInfo skin = GetSkinInfo(result.skinIndex);

        GameObject prefab = null;

        if (skin != null && skin.previewModel != null)
        {
            prefab = skin.previewModel;   
        }
        else
        {
            prefab = fallbackPrefab;    
        }

        if (prefab == null)
        {
            Debug.LogWarning($"[PodiumController] No hay previewModel ni fallback para {result.playerName}.");
            return null;
        }

        GameObject visual = Instantiate(
            prefab,
            slot.PlayerSpawnPoint.position,
            slot.PlayerSpawnPoint.rotation,
            slot.PlayerSpawnPoint
        );

        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        if (skin != null)
        {
            ApplySkinMaterial(visual, skin, result.colorIndex);
            ApplySkinAnimator(visual, skin);
        }

        PlayRankAnimation(visual, rankIndex);

        Debug.Log($"[PodiumController] Spawn de {result.playerName} | Skin {result.skinIndex} | Color {result.colorIndex}");

        return visual;
    }

    private void ApplySkinMaterial(GameObject visual, SkinInfo skin, int colorIndex)
    {
        if (visual == null || skin == null) return;

        if (skin.skins == null || skin.skins.Length == 0)
        {
            Debug.LogWarning($"[PodiumController] La skin {skin.skinName} no tiene materiales.");
            return;
        }

        int safeColorIndex = Mathf.Clamp(colorIndex, 0, skin.skins.Length - 1);
        Material material = skin.skins[safeColorIndex];

        if (material == null)
        {
            Debug.LogWarning($"[PodiumController] Material nulo en {skin.skinName}, color {safeColorIndex}.");
            return;
        }

        PlayerSkin.ApplyTo(visual, material);
    }

    private void ApplySkinAnimator(GameObject visual, SkinInfo skin)
    {
        if (visual == null || skin == null || skin.animator == null) return;

        Animator animator = visual.GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            Debug.LogWarning($"[PodiumController] El modelo {visual.name} no tiene Animator.");
            return;
        }

        animator.runtimeAnimatorController = skin.animator;
    }

    private void PlayRankAnimation(GameObject visual, int rankIndex)
    {
        if (visual == null) return;

        Animator animator = visual.GetComponentInChildren<Animator>(true);

        if (animator == null)
        {
            Debug.LogWarning($"[PodiumController] El modelo {visual.name} no tiene Animator.");
            return;
        }

        animator.applyRootMotion = false;

        switch (rankIndex)
        {
            case 0:
                PlayAnimatorTriggerIfExists(animator, "Jump");
                break;

            case 1:
                animator.Play("Idle");
                break;

            case 2:
                PlayAnimatorTriggerIfExists(animator, "Death");
                break;
        }
    }

    private void PlayAnimatorTriggerIfExists(Animator animator, string triggerName)
    {
        if (animator == null) return;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger &&
            parameter.name == triggerName)
            {
                animator.SetTrigger(triggerName);
                return;
            }
        }

        Debug.LogWarning($"[PodiumController] El Animator no tiene trigger '{triggerName}'.");
    }

    private SkinInfo GetSkinInfo(int skinIndex)
    {
        if (arraySkins == null || arraySkins.Length == 0)
        {
            Debug.LogWarning("[PodiumController] arraySkins está vacío.");
            return null;
        }

        if (skinIndex < 0 || skinIndex >= arraySkins.Length)
        {
            Debug.LogWarning($"[PodiumController] skinIndex inválido: {skinIndex}.");
            return null;
        }

        SkinInfo skin = arraySkins[skinIndex];

        if (skin == null)
        {
            Debug.LogWarning($"[PodiumController] SkinInfo nulo en índice {skinIndex}.");
            return null;
        }

        return skin;
    }

    private void PrepareSlot(PodiumSlot slot)
    {
        if (slot == null) return;

        slot.HideStats();
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
