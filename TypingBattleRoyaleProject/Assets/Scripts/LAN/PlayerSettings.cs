using UnityEngine;
using Unity.Netcode.Components;

[CreateAssetMenu(fileName = "NewPlayerSettings", menuName = "Multiplayer/Player Settings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de movimiento horizontal (WASD)")]
    [Range(1f, 20f)]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Velocidad de rotación hacia la dirección de movimiento")]
    [Range(1f, 30f)]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Configuración de Salto")]
    [Tooltip("Fuerza aplicada al saltar")]
    [Range(5f, 15f)]
    [SerializeField] private float jumpForce = 8f;

    [Tooltip("Fuerza de gravedad personalizada. Usa Physics.gravity si es 0")]
    [Range(-20f, -5f)]
    [SerializeField] private float customGravity = 0f;

    [Header("Características Físicas")]
    [Tooltip("Distancia máxima de detección del suelo")]
    [Range(0.1f, 1f)]
    [SerializeField] private float groundCheckDistance = 0.4f;

    [Tooltip("Radio de la esfera de detección del suelo")]
    [Range(0.1f, 1f)]
    [SerializeField] private float groundCheckRadius = 0.3f;

    [Header("Características del CharacterController")]
    [Tooltip("Altura de la cápsula del CharacterController")]
    [Range(1f, 3f)]
    [SerializeField] private float characterHeight = 2f;

    [Tooltip("Radio de la cápsula del CharacterController")]
    [Range(0.3f, 1f)]
    [SerializeField] private float characterRadius = 0.5f;

    [Header("Características de Red")]
    [Tooltip("Interpolación de posición en NetworkTransform")]
    [Range(0f, 1f)]
    [SerializeField] private float positionThreshold = 0.01f;

    [Tooltip("Interpolación de rotación en NetworkTransform")]
    [Range(0.1f, 5f)]
    [SerializeField] private float rotationThreshold = 1f;
    
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public float JumpForce => jumpForce;
    public float Gravity => customGravity != 0f ? customGravity : Physics.gravity.y;
    public float GroundCheckDistance => groundCheckDistance;
    public float GroundCheckRadius => groundCheckRadius;
    public float CharacterHeight => characterHeight;
    public float CharacterRadius => characterRadius;
    public float PositionThreshold => positionThreshold;
    public float RotationThreshold => rotationThreshold;
    
    public void ConfigureCharacterController(CharacterController controller)
    {
        if (controller == null)
        {
            Debug.LogWarning("CharacterController es null en ConfigureCharacterController");
            return;
        }

        controller.height = characterHeight;
        controller.radius = characterRadius;
        controller.center = new Vector3(0, characterHeight / 2f, 0);
    }
    
    public void ConfigureNetworkTransform(Unity.Netcode.Components.NetworkTransform networkTransform)
    {
        if (networkTransform == null)
        {
            Debug.LogWarning("NetworkTransform es null en ConfigureNetworkTransform");
            return;
        }
        
        networkTransform.SyncPositionX = true;
        networkTransform.SyncPositionY = true;
        networkTransform.SyncPositionZ = true;
        
        networkTransform.SyncScaleX = false;
        networkTransform.SyncScaleY = false;
        networkTransform.SyncScaleZ = false;
    }
}
