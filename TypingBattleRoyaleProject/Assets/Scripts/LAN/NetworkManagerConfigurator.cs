using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkManagerConfigurator : MonoBehaviour
{
    [Header("Configuración de Transporte")]
    [SerializeField] private ushort port = 7777;
    [SerializeField] private string connectionAddress = "127.0.0.1";

    [Header("Prefabs de Red")]
    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        ConfigureNetworkManager();
    }
    
    private void ConfigureNetworkManager()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("<color=red>NetworkManager no encontrado en la escena.</color>");
            Debug.LogError("Por favor, agrega un GameObject con componente NetworkManager en LobbyScene.");
            return;
        }
        
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError("<color=red>UnityTransport no encontrado en el NetworkManager.</color>");
            Debug.LogError("Por favor, agrega el componente UnityTransport al NetworkManager.");
            return;
        }
        
        transport.SetConnectionData(connectionAddress, port);

        Debug.Log($"<color=green>NetworkManager configurado:</color>");
        Debug.Log($"  - Puerto: {port}");
        Debug.Log($"  - Dirección por defecto: {connectionAddress}");
        
        if (playerPrefab != null)
        {
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = playerPrefab;
            Debug.Log($"<color=green>Player Prefab asignado: {playerPrefab.name}</color>");
        }
    }
}
