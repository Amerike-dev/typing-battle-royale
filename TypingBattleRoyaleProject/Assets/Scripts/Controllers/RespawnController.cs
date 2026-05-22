using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class RespawnController : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Free Spawn Check")]
    [SerializeField] private float spawnCheckRadius = 1.5f;
    [SerializeField] private LayerMask playerLayerMask;

    private int selectedIndex;

    private bool IsServerActive()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    }

    public Vector3 GetFreeSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[RespawnController] No hay spawn points asignados.");
            return Vector3.zero;
        }

        int startIndex = Random.Range(0, spawnPoints.Length);

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int index = (startIndex + i) % spawnPoints.Length;
            Transform spawn = spawnPoints[index];

            if (spawn == null) continue;

            bool occupied = Physics.CheckSphere(
                spawn.position,
                spawnCheckRadius,
                playerLayerMask
            );

            if (!occupied)
            {
                selectedIndex = index;
                return spawn.position;
            }
        }

        selectedIndex = startIndex;
        return spawnPoints[selectedIndex].position;
    }

    public void RespawnPlayerServer(PlayerStatsNet stats)
    {
        if (!IsServerActive()) return;

        if (stats == null)
        {
            Debug.LogWarning("[RespawnController] PlayerStatsNet null en RespawnPlayerServer.");
            return;
        }

        Vector3 targetPosition = MovePlayerToSpawn(stats);

        stats.FinishServerRespawn(targetPosition);

        Debug.Log($"[RespawnController] Respawn por combate de {stats.ID} en spawn index {selectedIndex}");
    }

    public void RespawnPlayerFromFallServer(PlayerStatsNet stats)
    {
        if (!IsServerActive()) return;

        if (stats == null)
        {
            Debug.LogWarning("[RespawnController] PlayerStatsNet null en RespawnPlayerFromFallServer.");
            return;
        }

        if (!stats.isAlive.Value)
        {
            Debug.Log("[RespawnController] El jugador cayó, pero está muerto/inactivo. Se ignora.");
            return;
        }

        Vector3 targetPosition = MovePlayerToSpawn(stats);

        stats.ForceMoveOwnerClientRpc(targetPosition);

        Debug.Log($"[RespawnController] Respawn por caída de {stats.ID} en spawn index {selectedIndex}");
    }

    private Vector3 MovePlayerToSpawn(PlayerStatsNet stats)
    {
    Vector3 targetPosition = GetFreeSpawnPosition();

    CharacterController characterController = stats.GetComponent<CharacterController>();

    if (characterController != null)
    {
        characterController.enabled = false;
    }

    stats.transform.position = targetPosition;

    if (characterController != null)
    {
        characterController.enabled = true;
    }

    return targetPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[RespawnController] Trigger detectó: {other.name}");

    PlayerStatsNet stats = other.GetComponent<PlayerStatsNet>();

    if (stats == null)
    {
        stats = other.GetComponentInParent<PlayerStatsNet>();
    }

    if (stats == null)
    {
        stats = other.GetComponentInChildren<PlayerStatsNet>();
    }

    if (stats == null)
    {
        Debug.LogWarning($"[RespawnController] No se encontró PlayerStatsNet en {other.name}.");
        return;
    }

    if (IsServerActive())
    {
        RespawnPlayerFromFallServer(stats);
        return;
    }

    if (stats.IsOwner)
    {
        Debug.Log("[RespawnController] Cliente dueño detectó caída. Pidiendo respawn al servidor.");
        stats.RequestFallRespawnServerRpc();
    }
    else
    {
        Debug.Log("[RespawnController] Cliente no dueño detectó caída. Se ignora.");
    }
    }
}
