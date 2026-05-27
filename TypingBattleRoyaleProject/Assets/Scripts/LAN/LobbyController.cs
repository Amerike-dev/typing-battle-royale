using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LobbyController : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button _startMatchButton;
    [SerializeField] private LobbyUIController lobbyUI;

    [Header("LAN Configuration")]
    [SerializeField] private ushort defaultPort = 7777;
    [SerializeField] private string gameScene = "GameplayScene";

    private NetworkList<ulong> connectedPlayers;

    [Header("Lobby Setting")]
    [Range(2, 8)]
    [SerializeField] private int _maxPlayers = 4;

    private const string HostButtonIdleText = "Crear Partida";
    private const string HostButtonActiveText = "Creada:";
    private const float HostButtonLabelXIdle = 140f;
    private const float HostButtonLabelXActive = 86f;
    private const float HostButtonLabelWidthActive = 110f;

    [Header("Hosted IP display")]
    [SerializeField] private float _hostedIpFontSize = 24f;

    private TextMeshProUGUI _hostButtonLabel;
    private ColorBlock _hostButtonColorsIdle;
    private ColorBlock _hostButtonColorsActive;
    private float _hostButtonLabelWidthIdle;
    private bool _isHosting;

    private void Awake()
    {
        connectedPlayers = new NetworkList<ulong>();
    }

    private void Start()
    {
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(OnHostButtonClicked);
            _hostButtonLabel = hostButton.GetComponentInChildren<TextMeshProUGUI>(true);
            _hostButtonColorsIdle = hostButton.colors;
            _hostButtonColorsActive = _hostButtonColorsIdle;
            _hostButtonColorsActive.normalColor = _hostButtonColorsIdle.pressedColor;
            _hostButtonColorsActive.highlightedColor = _hostButtonColorsIdle.pressedColor;
            _hostButtonColorsActive.selectedColor = _hostButtonColorsIdle.pressedColor;
            if (_hostButtonLabel != null) _hostButtonLabelWidthIdle = _hostButtonLabel.rectTransform.sizeDelta.x;
        }

        if (joinButton != null) joinButton.onClick.AddListener(OnJoinButtonClicked);

        if (_startMatchButton != null)
        {
            _startMatchButton.onClick.AddListener(StartMatch);
            _startMatchButton.gameObject.SetActive(false);
        }

        string localIp = GetLocalIPAdress();

        if (ipInputField != null && string.IsNullOrEmpty(ipInputField.text)) ipInputField.text = localIp;

        ApplyHostButtonVisualState(false);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedLocal;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedLocal;
    }
    
    private void Update()
    {
        if ((Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame) ||
            (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
        {
            if (AttractModeController.Instance != null)
            {
                AttractModeController.Instance.ResetIdleTimer();
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        connectedPlayers.OnListChanged += (NetworkListEvent<ulong> changeEvent) => {
            UpdateLobbyUI();
        };

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnServerClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnServerClientDisconnected;
        }
    }

    private void OnHostButtonClicked()
    {
        if (_isHosting)
        {
            StopHosting();
            return;
        }

        if (NetworkManager.Singleton.IsClient)
        {
            lobbyUI.ShowStatusMessage("Ya estás conectado como cliente.", true);
            return;
        }

        Unity.Netcode.Transports.UTP.UnityTransport transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

        if (transport != null) transport.SetConnectionData("0.0.0.0", defaultPort);
        if (transport != null) {
            transport.ConnectionData.Address = "0.0.0.0";
            transport.ConnectionData.Port = defaultPort;
        }

        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

        bool success = NetworkManager.Singleton.StartHost();

        if (success)
        {
            ApplyHostButtonVisualState(true);
            lobbyUI.ShowStatusMessage("Host iniciado. Esperando jugadores");
        }
        else
        {
            lobbyUI.ShowStatusMessage("Error al iniciar el HOST", true);
        }
    }

    private void StopHosting()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (_startMatchButton != null) _startMatchButton.gameObject.SetActive(false);

        ApplyHostButtonVisualState(false);
        lobbyUI.ShowStatusMessage("Host detenido.");
    }

    private void ApplyHostButtonVisualState(bool hosting)
    {
        _isHosting = hosting;

        if (hostButton != null) hostButton.colors = hosting ? _hostButtonColorsActive : _hostButtonColorsIdle;

        if (_hostButtonLabel != null)
        {
            _hostButtonLabel.text = hosting ? HostButtonActiveText : HostButtonIdleText;

            var rt = _hostButtonLabel.rectTransform;
            Vector2 pos = rt.anchoredPosition;
            pos.x = hosting ? HostButtonLabelXActive : HostButtonLabelXIdle;
            rt.anchoredPosition = pos;

            Vector2 size = rt.sizeDelta;
            size.x = hosting ? HostButtonLabelWidthActive : _hostButtonLabelWidthIdle;
            rt.sizeDelta = size;
        }

        if (ipInputField != null)
        {
            ipInputField.gameObject.SetActive(hosting);

            if (hosting && ipInputField.textComponent != null)
            {
                ipInputField.textComponent.color = Color.white;
                ipInputField.textComponent.fontSize = _hostedIpFontSize;
            }
        }
    }

    private void OnServerClientConnected(ulong clientId)
    {
        if (!connectedPlayers.Contains(clientId))
            connectedPlayers.Add(clientId);
    }

    private void OnServerClientDisconnected(ulong clientId)
    {
        if (connectedPlayers.Contains(clientId))
            connectedPlayers.Remove(clientId);
    }

    private void OnJoinButtonClicked()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            lobbyUI.ShowStatusMessage("Ya estás conectado o iniciando una conexión.", true);
            return;
        }

        string targetIP = ipInputField.text;
        
        if (string.IsNullOrEmpty(targetIP)) return;
        
        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

        if (transport != null) transport.SetConnectionData(targetIP, defaultPort );
        if (transport != null)
        {
            transport.ConnectionData.Address = targetIP;
            transport.ConnectionData.Port = defaultPort;
        }
        
        bool success = NetworkManager.Singleton.StartClient();

        if (success)
            lobbyUI.ShowStatusMessage($"<color=green>Iniciando conexión a {targetIP}...</color>");
        else
            lobbyUI.ShowStatusMessage("<color=red>Error al iniciar el Cliente.</color>", true);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (connectedPlayers.Count >= _maxPlayers)
        {
            response.Approved = false;
            response.Reason = $"Sala llena ({_maxPlayers}/{_maxPlayers})";
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = true;
    }

    private void OnClientConnectedLocal(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId && !NetworkManager.Singleton.IsHost)
        {
            lobbyUI.ShowStatusMessage("Conectado al Host");
        }
    }

    private void OnClientDisconnectedLocal(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            string reason = NetworkManager.Singleton.DisconnectReason;
            if (string.IsNullOrEmpty(reason)) reason = "Conexión perdida";

            lobbyUI.ShowStatusMessage(reason, true);
        }
    }

    private void UpdateLobbyUI()
    {
        lobbyUI.UpdatePlayerSlots(connectedPlayers, _maxPlayers);

        if (IsServer && _startMatchButton != null)
        {
            _startMatchButton.gameObject.SetActive(true);
            _startMatchButton.interactable = connectedPlayers.Count >= 2;
        }
    }

    private void StartMatch()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        }
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
    
    public override void OnDestroy()
    {
        base.OnDestroy();

        if (hostButton != null) hostButton.onClick.RemoveListener(OnHostButtonClicked);
        if (joinButton != null) joinButton.onClick.RemoveListener(OnJoinButtonClicked);

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedLocal;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedLocal;

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnServerClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnServerClientDisconnected;
            }
        } 
    }
}
