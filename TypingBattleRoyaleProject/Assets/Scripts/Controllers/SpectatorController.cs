using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpectatorController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private SpectatorUI spectatorUI;

    private readonly List<PlayerStatsNet> aliveTargets = new();
    private int currentTargetIndex = -1;
    private bool isSpectating;
    private PlayerStatsNet currentTargetStats;

    private void Awake()
    {
        enabled = false;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!isSpectating) return;

        if (Keyboard.current == null) return;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            NextAlive();
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            PreviousAlive();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromCurrentTarget();
    }

    public void BeginSpectating(CameraController localCamera, SpectatorUI localSpectatorUI)
    {
        if (!IsOwner) return;

        cameraController = localCamera;
        spectatorUI = localSpectatorUI;

        isSpectating = true;
        enabled = true;

        RefreshAliveTargets();

        if (aliveTargets.Count > 0)
        {
            currentTargetIndex = 0;
            SetCurrentTarget();
        }
        else
        {
            currentTargetIndex = -1;

            if (spectatorUI != null)
            {
                spectatorUI.SetTargetName("Sin jugadores vivos");
            }
        }
    }

    public void NextAlive()
    {
        RefreshAliveTargets();

        if (aliveTargets.Count == 0) return;

        currentTargetIndex++;

        if (currentTargetIndex >= aliveTargets.Count)
        {
            currentTargetIndex = 0;
        }

        SetCurrentTarget();
    }

    public void PreviousAlive()
    {
        RefreshAliveTargets();

        if (aliveTargets.Count == 0) return;

        currentTargetIndex--;

        if (currentTargetIndex < 0)
        {
            currentTargetIndex = aliveTargets.Count - 1;
        }

        SetCurrentTarget();
    }

    private void RefreshAliveTargets()
    {
        aliveTargets.Clear();

        if (NetworkManager.Singleton == null) return;

        aliveTargets.AddRange(
            NetworkManager.Singleton.ConnectedClientsList
                .Select(client => client.PlayerObject != null
                    ? client.PlayerObject.GetComponent<PlayerStatsNet>()
                    : null)
                .Where(stats => stats != null)
                .Where(stats => stats.isAlive.Value)
                .Where(stats => !stats.isSpectating.Value)
        );

        if (currentTargetIndex >= aliveTargets.Count)
        {
            currentTargetIndex = aliveTargets.Count - 1;
        }
    }

    private void SetCurrentTarget()
    {
        if (currentTargetIndex < 0) return;
        if (currentTargetIndex >= aliveTargets.Count) return;

        PlayerStatsNet targetStats = aliveTargets[currentTargetIndex];

        if (targetStats == null) return;

        SubscribeToCurrentTarget(targetStats);

        if (cameraController != null)
        {
            cameraController.SetSpectatorTarget(targetStats.transform);
        }

        if (spectatorUI != null)
        {
            spectatorUI.SetTargetName(targetStats.ID);
        }

        Debug.Log($"[SpectatorController] Viendo a {targetStats.ID}");
    }

    private void SubscribeToCurrentTarget(PlayerStatsNet targetStats)
    {
        UnsubscribeFromCurrentTarget();

        currentTargetStats = targetStats;

        if (currentTargetStats != null)
        {
            currentTargetStats.OnAllLifeLost += HandleCurrentTargetDied;
        }
    }

    private void UnsubscribeFromCurrentTarget()
    {
        if (currentTargetStats != null)
        {
            currentTargetStats.OnAllLifeLost -= HandleCurrentTargetDied;
            currentTargetStats = null;
        }
    }

    private void HandleCurrentTargetDied()
    {
        if (!IsOwner) return;
        if (!isSpectating) return;

        Debug.Log("[SpectatorController] El target observado murió. Buscando siguiente vivo...");

        RefreshAliveTargets();

        if (aliveTargets.Count == 0)
        {
            currentTargetIndex = -1;
            UnsubscribeFromCurrentTarget();

            if (cameraController != null)
            {
                cameraController.ClearSpectatorTarget();
            }

            if (spectatorUI != null)
            {
                spectatorUI.SetTargetName("Sin jugadores vivos");
            }

        return;
        }

        if (currentTargetIndex >= aliveTargets.Count)
        {
            currentTargetIndex = 0;
        }

        SetCurrentTarget();
    }

    public void StopSpectating()
    {
        isSpectating = false;
        enabled = false;

        UnsubscribeFromCurrentTarget();

        if (cameraController != null)
        {
            cameraController.ClearSpectatorTarget();
        }

        if (spectatorUI != null)
        {
            spectatorUI.Hide();
        }

        Debug.Log("[SpectatorController] Saliendo del modo espectador.");
    }

}
