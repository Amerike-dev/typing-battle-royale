using UnityEngine;
using Unity.Netcode;
<<<<<<< HEAD
using Unity.Services.Lobbies.Models;
=======
using UnityEngine.InputSystem;
>>>>>>> TBR-007_BugJugadoresNoSeVen


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(PlayerStatsNet))]
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Referencias")]
    private CharacterController characterController;
    private NetworkObject networkObject;
    private PlayerStatsNet statsNet;

    [Header("Input")]
    private Vector2 moveInput;
    private bool jumpInput;
    private bool isGrounded;
    private Vector3 velocity;
    private PlayerInput actions;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        networkObject = GetComponent<NetworkObject>();
        statsNet = GetComponent<PlayerStatsNet>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        Debug.Log($"<color=cyan>Player {OwnerClientId} spawnado. IsOwner: {IsOwner}</color>");
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        MovePlayer();
    }
    

    
    private void MovePlayer()
    {
        isGrounded = characterController.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        
        characterController.Move(moveDirection * (moveSpeed * Time.deltaTime));
        
        if (jumpInput && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            jumpInput = false;
        }
        
        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void TakeDamageServerRpc(float damage, ulong attackerId)
    {
        if (!statsNet.isAlive.Value)
            return;

        if (statsNet.currentHP.Value <= 0)
            return;

        statsNet.currentHP.Value = Mathf.Max(0, statsNet.currentHP.Value - damage);

        Debug.Log($"Player {OwnerClientId} recibió {damage} de daño. HP actual: {statsNet.currentHP.Value}");

        if (statsNet.currentHP.Value <= 0)
        {
            HandleDeathServer(attackerId);
        }
    }

    private void HandleDeathServer(ulong attackerId)
    {
        if (!IsServer) return;

        statsNet.currentLifes.Value--;

        Debug.Log($"Player {OwnerClientId} perdió una vida. Vidas restantes: {statsNet.currentLifes.Value}");

        GiveKillToAttacker(attackerId);

        if (statsNet.currentLifes.Value > 0)
        {
            statsNet.currentHP.Value = statsNet.MaxHP;
            statsNet.isAlive.Value = true;

            Debug.Log($"Player {OwnerClientId} respawnea con HP lleno.");
        }
        else
        {
            statsNet.isAlive.Value = false;

            Debug.Log($"Player {OwnerClientId} se quedó sin vidas.");
        }
    }

    private void GiveKillToAttacker(ulong attackerId)
    {
        if (!IsServer) return;

        if (attackerId == OwnerClientId)
        {
            Debug.Log("El atacante es el mismo jugador. No se suma kill.");
            return;
        }

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(attackerId, out var attackerClient))
        {
            Debug.LogWarning($"No se encontró al atacante con ID {attackerId}");
            return;
        }

        if (attackerClient.PlayerObject == null)
        {
            Debug.LogWarning("El atacante no tiene PlayerObject.");
            return;
        }

        PlayerStatsNet attackerStats = attackerClient.PlayerObject.GetComponent<PlayerStatsNet>();

        if (attackerStats == null)
        {
            Debug.LogWarning("El atacante no tiene PlayerStatsNet.");
            return;
        }

        attackerStats.killCount.Value++;

        Debug.Log($"Player {attackerId} ganó una kill. Total: {attackerStats.killCount.Value}");
    }
    
    [ClientRpc]
    public void SetPlayerColorClientRpc(Color newColor)
    {
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = newColor;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            Debug.Log($"<color=yellow>Player {OwnerClientId} desconectado.</color>");
        }
    }

    private void OnGUI()
    {
        if (!IsOwner) return;
        
        GUI.Label(new Rect(10, 10, 300, 20), $"ID: {OwnerClientId}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Position: {transform.position}");

        if (statsNet != null)
        {
            GUI.Label(new Rect(10, 50, 300, 20), $"HP: {statsNet.currentHP.Value}");
            GUI.Label(new Rect(10, 70, 300, 20), $"Lives: {statsNet.currentLifes.Value}");
            GUI.Label(new Rect(10, 90, 300, 20), $"Kills: {statsNet.killCount.Value}");
        }
    }
}
