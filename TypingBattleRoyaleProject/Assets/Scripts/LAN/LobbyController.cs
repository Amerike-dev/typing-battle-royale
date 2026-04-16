using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    [Header("LAN Configuration")]
    [SerializeField] private ushort defaultPort = 7777;
    [SerializeField] private string gameScene = "GameplayScene";

    private void Start()
    {
        if (hostButton != null) hostButton.onClick.AddListener(OnJoinButtonClicked);

        if (joinButton != null) joinButton.onClick.AddListener(OnJoinButtonClicked);

        string localIp = GetLocalIPAdress();

        if(ipInputField != null && string.IsNullOrEmpty(ipInputField.text)) ipInputField.text = localIp;
    }

    private void OnHostButtonClicked()
    {
        Unity.Netcode.Transports.UTP.UnityTransport transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

        if (transport != null) transport.SetConnectionData("0.0.0.0", (ushort)defaultPort);

        bool success = NetworkManager.Singleton.StartHost();

        if (success)
            NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        else
            Debug.LogError("Error al iniciar el HOST");
    }

    private void OnJoinButtonClicked()
    {
        string targetIP = ipInputField.text;
        
        if (string.IsNullOrEmpty(targetIP)) return;
        
        Unity.Netcode.Transports.UTP.UnityTransport transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

        if (transport != null) transport.SetConnectionData(targetIP, (ushort)defaultPort );
        
        bool success = NetworkManager.Singleton.StartClient();

        if (success)
            Debug.Log($"<color=green>Iniciando conexión a {targetIP}...</color>");
        else
            Debug.LogError("<color=red>Error al iniciar el Cliente.</color>");
    }

    private string GetLocalIPAdress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    string ipStr = ip.ToString();
                    if (ipStr.StartsWith("192.168.") || ipStr.StartsWith("10.")) return ipStr;
                }
            }
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) return ip.ToString();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error al obtener IP local: {e.Message}");
        }

        return "127.0.0.1";
    }
    
    private void OnDestroy()
    {
        if (hostButton != null) hostButton.onClick.RemoveListener(OnHostButtonClicked);

        if (joinButton != null) joinButton.onClick.RemoveListener(OnJoinButtonClicked);
    }
}
