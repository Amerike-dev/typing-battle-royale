using UnityEngine;
using Unity.Netcode;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Referencias")]
    private CharacterController characterController;
    private NetworkObject networkObject;

    [Header("Input")]
    private Vector2 moveInput;
    private bool jumpInput;
    private bool isGrounded;
    private Vector3 velocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        networkObject = GetComponent<NetworkObject>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

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
    }
}
