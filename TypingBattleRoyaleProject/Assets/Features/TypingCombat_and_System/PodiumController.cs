using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PodiumController : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    [Header("Botones (solo host)")]
    [Tooltip("Contenedor de los botones de fin de partida (Jugar de nuevo / Ir al menú). Solo se " +
             "muestran y son interactuables en el HOST; los clientes ven el podio sin botones.")]
    [SerializeField] private GameObject hostButtonsRoot;

    // Evita que el callback de desconexión cargue la escena dos veces (host que apaga + su propio evento).
    private bool _returningToLobby;
    private bool _disconnectCallbackRegistered;

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

        // Los clientes (no host) escuchan la desconexión: si el host cierra el servidor al volver al
        // lobby, ellos vuelven solos al lobby también, para poder jugar otra partida sin reiniciar.
        RegisterClientDisconnectListener();

        StartCoroutine(RevealPodiumRoutine());
    }

    private void RegisterClientDisconnectListener()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;
        if (nm.IsServer) return;               // el host no se auto-escucha; controla el cierre a mano
        if (_disconnectCallbackRegistered) return;

        nm.OnClientDisconnectCallback += OnLocalClientDisconnected;
        _disconnectCallbackRegistered = true;
    }

    private void OnLocalClientDisconnected(ulong clientId)
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;
        if (clientId != nm.LocalClientId) return;   // solo reaccionamos a NUESTRA desconexión

        UnregisterClientDisconnectListener();
        ReturnToLobby();
    }

    private void UnregisterClientDisconnectListener()
    {
        if (!_disconnectCallbackRegistered) return;
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnLocalClientDisconnected;
        _disconnectCallbackRegistered = false;
    }

    private void OnDestroy()
    {
        UnregisterClientDisconnectListener();
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
        // Los botones de fin de partida son SOLO para el host: él decide volver a jugar o ir al menú.
        // Los clientes ven el podio sin botones y siguen al host (vuelven al lobby cuando él cierra).
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;

        if (hostButtonsRoot != null)
            hostButtonsRoot.SetActive(isHost);

        if (buttonsCanvasGroup == null)
            return;

        if (!isHost)
        {
            // Cliente: nos aseguramos de que no haya botones interactuables.
            buttonsCanvasGroup.alpha = 0f;
            buttonsCanvasGroup.interactable = false;
            buttonsCanvasGroup.blocksRaycasts = false;
            return;
        }

        buttonsCanvasGroup
            .DOFade(1f, 0.5f)
            .SetEase(Ease.OutCubic);

        buttonsCanvasGroup.interactable = true;
        buttonsCanvasGroup.blocksRaycasts = true;
    }

    /// <summary>Botón "Jugar de nuevo" (solo host): recarga la partida en red para todos.</summary>
    public void PlayAgain()
    {
        Time.timeScale = 1f;

        NetworkManager nm = NetworkManager.Singleton;
        if (nm != null && nm.IsServer)
        {
            // El servidor sigue vivo: usamos el SceneManager de red para llevar a TODOS a Gameplay.
            nm.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    /// <summary>
    /// Botón "Ir al menú principal" (solo host): cierra el servidor y vuelve al lobby. Al hacer
    /// Shutdown, los clientes reciben su OnClientDisconnect y vuelven al lobby por su cuenta, de modo
    /// que se puede empezar otra partida sin reiniciar la aplicación.
    /// </summary>
    public void MainMenu()
    {
        Time.timeScale = 1f;

        if (_returningToLobby) return;
        _returningToLobby = true;

        StartCoroutine(ShutdownAndReturnToLobby());
    }

    /// <summary>
    /// Cierra el servidor/host y espera a que NGO termine el apagado (Shutdown es asíncrono) ANTES de
    /// recargar el lobby. Si cargáramos en el mismo frame, la LobbyScene podría inicializarse mientras
    /// el NetworkManager aún está apagándose y volver a quedar en un estado inconsistente.
    /// </summary>
    private IEnumerator ShutdownAndReturnToLobby()
    {
        NetworkManager nm = NetworkManager.Singleton;

        if (nm != null && (nm.IsListening || nm.IsHost || nm.IsServer || nm.IsClient))
        {
            // Cierra host/servidor/cliente. Libera el puerto y limpia el estado de red para que la
            // LobbyScene recargada pueda volver a hostear sin chocar con la sesión anterior.
            nm.Shutdown();

            // Espera (con tope de seguridad) a que el apagado realmente termine.
            float timeout = Time.realtimeSinceStartup + 5f;
            while (nm != null && (nm.ShutdownInProgress || nm.IsListening) &&
                   Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }
        }

        ReturnToLobby();
    }

    private void ReturnToLobby()
    {
        if (!_returningToLobby) _returningToLobby = true;
        Time.timeScale = 1f;
        UnregisterClientDisconnectListener();
        SceneManager.LoadScene(lobbySceneName);
    }
}
