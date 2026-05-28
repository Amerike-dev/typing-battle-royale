using System;
using System.Collections;
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

    [Header("Join IP Entry (auto-clonado de NameInputField si queda vacío)")]
    [SerializeField] private TMP_InputField _joinIpInput;
    [SerializeField] private TMP_InputField _nameInputTemplate;
    [SerializeField] private float _joinIpSlideDuration = 0.18f;
    [Tooltip("Offset desde la posición oculta (sobre el botón) a la visible. Por defecto se desliza hacia la derecha.")]
    [SerializeField] private Vector2 _joinIpSlideOffset = new Vector2(290f, 0f);

    [Header("Confirm Modal (auto-construido si queda vacío)")]
    [SerializeField] private LobbyConfirmModal _confirmModal;
    [SerializeField] private RectTransform _modalCanvasParent;

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

    private const string JoinButtonIdleText = "Unirse a Partida";
    private const string JoinButtonReadyText = "Unirse";
    private const string JoinButtonConnectedFormat = "Conectado a {0}";

    private const string HostStopConfirmMessage = "Terminarás la partida para todos, ¿quieres continuar?";
    private const string HostEndedClientMessage = "La partida en la que estabas unido se ha terminado";

    [Header("Join button label scaling")]
    [Range(0.1f, 1f)]
    [SerializeField] private float _connectedLabelScale = 0.6f;

    [Header("Hosted IP display")]
    [SerializeField] private float _hostedIpFontSize = 24f;

    [Header("Player count badge (visible cuando hosteás)")]
    [SerializeField] private Sprite _playerCountSprite;
    [SerializeField] private Vector2 _playerCountBadgeSize = new Vector2(50f, 50f);
    [Tooltip("Anchored position absoluta del badge (en el mismo padre que el HostButton).")]
    [SerializeField] private Vector2 _playerCountBadgePosition = new Vector2(400f, 70f);
    [SerializeField] private Color _playerCountTextColor = Color.black;
    [SerializeField] private float _playerCountFontSize = 20f;
    [SerializeField] private TMP_FontAsset _playerCountFont;
    [Tooltip("Padding superior del texto dentro del badge (px).")]
    [SerializeField] private float _playerCountTextTopPadding = 15f;

    private TextMeshProUGUI _hostButtonLabel;
    private TextMeshProUGUI _joinButtonLabel;
    private ColorBlock _hostButtonColorsIdle;
    private ColorBlock _hostButtonColorsActive;
    private float _hostButtonLabelWidthIdle;
    private bool _isHosting;

    private bool _joinInputOpen;
    private Vector2 _joinIpHiddenPos;
    private Vector2 _joinIpVisiblePos;
    private Coroutine _slideRoutine;
    private bool _registeredNetworkCallbacks;
    private bool _approvalCallbackRegistered;
    private string _attemptingIp;
    private string _connectedIp;
    private float _joinLabelDefaultFontSize;
    private float _joinLabelDefaultFontSizeMax;
    private float _joinLabelDefaultFontSizeMin;
    private bool _joinLabelDefaultAutoSize;
    private bool _joinLabelDefaultsCaptured;

    private RectTransform _playerCountBadgeRoot;
    private TextMeshProUGUI _playerCountBadgeText;

    private void Awake()
    {
        connectedPlayers = new NetworkList<ulong>();
    }

    private void Start()
    {
        SetupHostButton();
        SetupJoinButton();
        SetupStartMatchButton();
        SetupJoinIpInput();
        SetupPlayerCountBadge();
        SetupConfirmModal();

        string localIp = GetLocalIPAdress();
        if (ipInputField != null && string.IsNullOrEmpty(ipInputField.text)) ipInputField.text = localIp;

        ApplyHostButtonVisualState(false);
        ApplyJoinButtonVisualState(false);

        RegisterNetworkCallbacks();
    }

    private void SetupHostButton()
    {
        if (hostButton == null) return;

        hostButton.onClick.AddListener(OnHostButtonClicked);
        _hostButtonLabel = hostButton.GetComponentInChildren<TextMeshProUGUI>(true);
        _hostButtonColorsIdle = hostButton.colors;
        _hostButtonColorsActive = _hostButtonColorsIdle;
        _hostButtonColorsActive.normalColor = _hostButtonColorsIdle.pressedColor;
        _hostButtonColorsActive.highlightedColor = _hostButtonColorsIdle.pressedColor;
        _hostButtonColorsActive.selectedColor = _hostButtonColorsIdle.pressedColor;
        if (_hostButtonLabel != null) _hostButtonLabelWidthIdle = _hostButtonLabel.rectTransform.sizeDelta.x;
    }

    private void SetupJoinButton()
    {
        if (joinButton == null) return;

        joinButton.onClick.AddListener(OnJoinButtonClicked);
        _joinButtonLabel = joinButton.GetComponentInChildren<TextMeshProUGUI>(true);

        if (_joinButtonLabel != null)
        {
            _joinLabelDefaultFontSize = _joinButtonLabel.fontSize;
            _joinLabelDefaultFontSizeMax = _joinButtonLabel.fontSizeMax;
            _joinLabelDefaultFontSizeMin = _joinButtonLabel.fontSizeMin;
            _joinLabelDefaultAutoSize = _joinButtonLabel.enableAutoSizing;
            _joinLabelDefaultsCaptured = true;
        }
    }

    private void SetupStartMatchButton()
    {
        if (_startMatchButton == null) return;

        _startMatchButton.onClick.AddListener(StartMatch);
        _startMatchButton.gameObject.SetActive(false);
    }

    private void SetupJoinIpInput()
    {
        if (_joinIpInput == null && _nameInputTemplate == null)
        {
            _nameInputTemplate = FindNameInputFieldInScene();
        }

        if (_joinIpInput == null && _nameInputTemplate != null && joinButton != null)
        {
            _joinIpInput = CloneInputAsJoinIp(_nameInputTemplate, joinButton.transform.parent);
        }

        if (_joinIpInput == null)
        {
            Debug.LogWarning("[LobbyController] No se encontró NameInputField para clonar como entrada de IP. Asigna _joinIpInput o _nameInputTemplate en el Inspector.");
            return;
        }

        RectTransform joinRt = joinButton != null ? joinButton.GetComponent<RectTransform>() : null;
        RectTransform inputRt = _joinIpInput.GetComponent<RectTransform>();

        if (joinRt != null)
        {
            inputRt.SetParent(joinRt.parent, false);
            inputRt.anchorMin = joinRt.anchorMin;
            inputRt.anchorMax = joinRt.anchorMax;
            inputRt.pivot = joinRt.pivot;
            inputRt.sizeDelta = new Vector2(joinRt.sizeDelta.x, joinRt.sizeDelta.y);
            _joinIpHiddenPos = joinRt.anchoredPosition;
            _joinIpVisiblePos = _joinIpHiddenPos + _joinIpSlideOffset;
            inputRt.anchoredPosition = _joinIpHiddenPos;
            inputRt.SetSiblingIndex(joinRt.GetSiblingIndex());
        }

        _joinIpInput.text = string.Empty;
        _joinIpInput.characterValidation = TMP_InputField.CharacterValidation.None;
        _joinIpInput.onValidateInput = ValidateIpChar;

        if (_joinIpInput.placeholder is TMP_Text placeholderText)
        {
            placeholderText.text = "Colocar IP...";
        }

        _joinIpInput.gameObject.SetActive(false);
    }

    private void SetupPlayerCountBadge()
    {
        if (hostButton == null) return;
        if (_playerCountBadgeRoot != null) return;

        RectTransform hostRt = hostButton.GetComponent<RectTransform>();
        if (hostRt == null) return;

        GameObject badge = new GameObject("HostPlayerCountBadge", typeof(RectTransform), typeof(Image));
        badge.transform.SetParent(hostRt.parent, false);

        _playerCountBadgeRoot = badge.GetComponent<RectTransform>();
        _playerCountBadgeRoot.anchorMin = hostRt.anchorMin;
        _playerCountBadgeRoot.anchorMax = hostRt.anchorMax;
        _playerCountBadgeRoot.pivot = new Vector2(0.5f, 0.5f);
        _playerCountBadgeRoot.sizeDelta = _playerCountBadgeSize;
        _playerCountBadgeRoot.anchoredPosition = _playerCountBadgePosition;

        Image img = badge.GetComponent<Image>();
        img.sprite = _playerCountSprite;
        img.preserveAspect = true;
        img.color = _playerCountSprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
        img.raycastTarget = false;

        GameObject textGo = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(badge.transform, false);

        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = new Vector2(0f, -Mathf.Max(0f, _playerCountTextTopPadding));

        _playerCountBadgeText = textGo.GetComponent<TextMeshProUGUI>();
        _playerCountBadgeText.text = "0";
        _playerCountBadgeText.alignment = TextAlignmentOptions.Center;
        _playerCountBadgeText.fontSize = _playerCountFontSize;
        _playerCountBadgeText.enableAutoSizing = false;
        _playerCountBadgeText.raycastTarget = false;
        _playerCountBadgeText.fontStyle = FontStyles.Bold;

        TMP_FontAsset font = _playerCountFont != null ? _playerCountFont : ResolveFontFromScene();
        if (font != null) _playerCountBadgeText.font = font;

        _playerCountBadgeText.color = _playerCountTextColor;
        _playerCountBadgeText.faceColor = _playerCountTextColor;
        _playerCountBadgeText.ForceMeshUpdate();

        badge.SetActive(false);
    }

    private void UpdatePlayerCountBadge()
    {
        if (_playerCountBadgeRoot == null || _playerCountBadgeText == null) return;
        if (connectedPlayers == null) return;

        bool visible = _isHosting;
        _playerCountBadgeRoot.gameObject.SetActive(visible);

        if (visible)
        {
            _playerCountBadgeText.text = connectedPlayers.Count.ToString();
        }
    }

    private void SetupConfirmModal()
    {
        if (_confirmModal != null) return;

        Transform parent = _modalCanvasParent != null ? (Transform)_modalCanvasParent : FindCanvasParent();
        if (parent == null) return;

        TMP_FontAsset font = ResolveFontFromScene();
        Sprite buttonSprite = ResolveButtonSprite();

        _confirmModal = LobbyConfirmModal.BuildRuntime(parent, font, buttonSprite);
    }

    private Transform FindCanvasParent()
    {
        if (hostButton != null)
        {
            Canvas c = hostButton.GetComponentInParent<Canvas>();
            if (c != null) return c.transform;
        }
        Canvas anyCanvas = FindAnyObjectByType<Canvas>();
        return anyCanvas != null ? anyCanvas.transform : null;
    }

    private TMP_InputField FindNameInputFieldInScene()
    {
        foreach (TMP_InputField candidate in FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (candidate == null) continue;
            if (candidate == _joinIpInput) continue;
            if (candidate == ipInputField) continue;
            if (candidate.gameObject.name == "NameInputField") return candidate;
        }
        return null;
    }

    private TMP_FontAsset ResolveFontFromScene()
    {
        if (_hostButtonLabel != null && _hostButtonLabel.font != null) return _hostButtonLabel.font;
        if (_joinButtonLabel != null && _joinButtonLabel.font != null) return _joinButtonLabel.font;
        if (ipInputField != null && ipInputField.textComponent is TMP_Text t && t.font != null) return t.font;
        return null;
    }

    private Sprite ResolveButtonSprite()
    {
        if (hostButton != null && hostButton.targetGraphic is Image img && img.sprite != null) return img.sprite;
        if (joinButton != null && joinButton.targetGraphic is Image img2 && img2.sprite != null) return img2.sprite;
        return null;
    }

    private TMP_InputField CloneInputAsJoinIp(TMP_InputField source, Transform parent)
    {
        GameObject clone = Instantiate(source.gameObject, parent);
        clone.name = "JoinIPInputField";
        TMP_InputField field = clone.GetComponent<TMP_InputField>();
        field.text = string.Empty;
        field.onValueChanged.RemoveAllListeners();
        field.onEndEdit.RemoveAllListeners();
        return field;
    }

    private static char ValidateIpChar(string text, int charIndex, char addedChar)
    {
        if (addedChar >= '0' && addedChar <= '9') return addedChar;
        if (addedChar == '.') return addedChar;
        return '\0';
    }

    private void RegisterNetworkCallbacks()
    {
        if (_registeredNetworkCallbacks || NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedLocal;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedLocal;
        _registeredNetworkCallbacks = true;
    }

    private void UnregisterNetworkCallbacks()
    {
        if (!_registeredNetworkCallbacks || NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedLocal;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectedLocal;
        _registeredNetworkCallbacks = false;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            QuitApplication();
            return;
        }

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

    private void QuitApplication()
    {
        CleanupNetworkState();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void OnNetworkSpawn()
    {
        connectedPlayers.OnListChanged += OnConnectedPlayersChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnServerClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnServerClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        connectedPlayers.OnListChanged -= OnConnectedPlayersChanged;

        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnServerClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnServerClientDisconnected;
        }
    }

    private void OnConnectedPlayersChanged(NetworkListEvent<ulong> change) => UpdateLobbyUI();

    private void OnHostButtonClicked()
    {
        if (_isHosting)
        {
            if (_confirmModal != null)
            {
                _confirmModal.Show(
                    HostStopConfirmMessage,
                    onConfirm: () => StopHostingAndNotifyClients());
                return;
            }

            StopHostingAndNotifyClients();
            return;
        }

        if (IsClientActiveOrAttempting())
        {
            string ipToShow = GetActiveOrAttemptingIp();
            if (_confirmModal != null)
            {
                _confirmModal.Show(
                    $"Te desconectarás de {ipToShow}, ¿estás seguro?",
                    onConfirm: () =>
                    {
                        CleanupNetworkState();
                        CloseJoinInput(instant: true);
                        StartHostInternal();
                    });
                return;
            }

            CleanupNetworkState();
        }

        CloseJoinInput(instant: true);
        StartHostInternal();
    }

    private void StopHostingAndNotifyClients()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm != null && nm.IsServer)
        {
            ulong localId = nm.LocalClientId;
            System.Collections.Generic.List<ulong> ids = new System.Collections.Generic.List<ulong>(nm.ConnectedClientsIds);
            foreach (ulong id in ids)
            {
                if (id == localId) continue;
                nm.DisconnectClient(id, HostEndedClientMessage);
            }
        }

        StopHosting();
    }

    private bool IsClientActiveOrAttempting()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return false;
        if (nm.IsClient || nm.IsConnectedClient) return true;
        return !string.IsNullOrEmpty(_attemptingIp);
    }

    private string GetActiveOrAttemptingIp()
    {
        if (!string.IsNullOrEmpty(_attemptingIp)) return _attemptingIp;

        Unity.Netcode.Transports.UTP.UnityTransport transport = GetTransport();
        if (transport == null) return "?";

        string addr = transport.ConnectionData.Address;
        return string.IsNullOrEmpty(addr) ? "?" : addr;
    }

    private Unity.Netcode.Transports.UTP.UnityTransport GetTransport()
    {
        if (NetworkManager.Singleton == null) return null;
        return NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
    }

    private void StartHostInternal()
    {
        Unity.Netcode.Transports.UTP.UnityTransport transport = GetTransport();
        if (transport != null)
        {
            transport.SetConnectionData("0.0.0.0", defaultPort);
            transport.ConnectionData.Address = "0.0.0.0";
            transport.ConnectionData.Port = defaultPort;
        }

        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        _approvalCallbackRegistered = true;

        bool success = NetworkManager.Singleton.StartHost();
        if (success)
        {
            ApplyHostButtonVisualState(true);
            lobbyUI.ShowStatusMessage("Host iniciado. Esperando jugadores");
        }
        else
        {
            CleanupNetworkState();
            ApplyHostButtonVisualState(false);
            lobbyUI.ShowStatusMessage("Error al iniciar el HOST", true);
        }
    }

    private void StopHosting()
    {
        CleanupNetworkState();

        if (_startMatchButton != null) _startMatchButton.gameObject.SetActive(false);

        ApplyHostButtonVisualState(false);
        lobbyUI.ShowStatusMessage("Host detenido.");
    }

    private void CleanupNetworkState()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;

        if (nm.IsListening || nm.IsHost || nm.IsServer || nm.IsClient)
        {
            nm.Shutdown();
        }

        if (_approvalCallbackRegistered)
        {
            nm.ConnectionApprovalCallback = null;
            _approvalCallbackRegistered = false;
        }

        _attemptingIp = null;
        _connectedIp = null;
        _isHosting = false;
    }

    private void ApplyHostButtonVisualState(bool hosting)
    {
        _isHosting = hosting;

        if (hostButton != null) hostButton.colors = hosting ? _hostButtonColorsActive : _hostButtonColorsIdle;

        if (_hostButtonLabel != null)
        {
            _hostButtonLabel.text = hosting ? HostButtonActiveText : HostButtonIdleText;

            RectTransform rt = _hostButtonLabel.rectTransform;
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

        UpdatePlayerCountBadge();
    }

    private void ApplyJoinButtonVisualState(bool inputOpen)
    {
        _joinInputOpen = inputOpen;
        if (_joinButtonLabel == null) return;

        if (inputOpen)
        {
            _joinButtonLabel.text = JoinButtonReadyText;
            RestoreJoinLabelFontSize();
            return;
        }

        if (!string.IsNullOrEmpty(_connectedIp))
        {
            _joinButtonLabel.text = string.Format(JoinButtonConnectedFormat, _connectedIp);
            ApplyConnectedLabelFontSize();
            return;
        }

        _joinButtonLabel.text = JoinButtonIdleText;
        RestoreJoinLabelFontSize();
    }

    private void ApplyConnectedLabelFontSize()
    {
        if (_joinButtonLabel == null || !_joinLabelDefaultsCaptured) return;

        float scale = Mathf.Clamp(_connectedLabelScale, 0.1f, 1f);
        _joinButtonLabel.enableAutoSizing = false;
        _joinButtonLabel.fontSize = _joinLabelDefaultFontSize * scale;
        _joinButtonLabel.fontSizeMax = _joinLabelDefaultFontSizeMax * scale;
        _joinButtonLabel.fontSizeMin = _joinLabelDefaultFontSizeMin * scale;
    }

    private void RestoreJoinLabelFontSize()
    {
        if (_joinButtonLabel == null || !_joinLabelDefaultsCaptured) return;

        _joinButtonLabel.enableAutoSizing = _joinLabelDefaultAutoSize;
        _joinButtonLabel.fontSize = _joinLabelDefaultFontSize;
        _joinButtonLabel.fontSizeMax = _joinLabelDefaultFontSizeMax;
        _joinButtonLabel.fontSizeMin = _joinLabelDefaultFontSizeMin;
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
        NetworkManager nm = NetworkManager.Singleton;

        if (nm != null && (nm.IsHost || nm.IsServer))
        {
            lobbyUI.ShowStatusMessage("Detén el host antes de unirte a otra partida.", true);
            return;
        }

        if (nm != null && nm.IsClient)
        {
            string suffix = string.IsNullOrEmpty(_attemptingIp) ? string.Empty : $" a {_attemptingIp}";
            lobbyUI.ShowStatusMessage($"Ya estás conectado o conectándote{suffix}.", true);
            return;
        }

        if (!_joinInputOpen)
        {
            OpenJoinInput();
            return;
        }

        string targetIp = _joinIpInput != null ? _joinIpInput.text.Trim() : string.Empty;
        if (!IsValidIp(targetIp))
        {
            lobbyUI.ShowStatusMessage("IP inválida. Usa formato 192.168.X.X", true);
            return;
        }

        TryStartClient(targetIp);
    }

    private bool IsValidIp(string s) => !string.IsNullOrEmpty(s) && System.Net.IPAddress.TryParse(s, out _);

    private void OpenJoinInput()
    {
        if (_joinIpInput == null)
        {
            lobbyUI.ShowStatusMessage("Campo de IP no configurado.", true);
            return;
        }

        ApplyJoinButtonVisualState(true);
        _joinIpInput.gameObject.SetActive(true);
        _joinIpInput.text = string.Empty;

        StartSlide(_joinIpVisiblePos);
        _joinIpInput.ActivateInputField();
    }

    private void CloseJoinInput(bool instant = false)
    {
        if (_joinIpInput == null)
        {
            ApplyJoinButtonVisualState(false);
            return;
        }

        ApplyJoinButtonVisualState(false);

        if (instant)
        {
            if (_slideRoutine != null) { StopCoroutine(_slideRoutine); _slideRoutine = null; }
            _joinIpInput.GetComponent<RectTransform>().anchoredPosition = _joinIpHiddenPos;
            _joinIpInput.gameObject.SetActive(false);
            return;
        }

        StartSlide(_joinIpHiddenPos, deactivateAfter: true);
    }

    private void StartSlide(Vector2 target, bool deactivateAfter = false)
    {
        if (_joinIpInput == null) return;
        if (_slideRoutine != null) StopCoroutine(_slideRoutine);
        _slideRoutine = StartCoroutine(SlideTo(target, deactivateAfter));
    }

    private IEnumerator SlideTo(Vector2 target, bool deactivateAfter)
    {
        RectTransform rt = _joinIpInput.GetComponent<RectTransform>();
        Vector2 from = rt.anchoredPosition;
        float t = 0f;
        float dur = Mathf.Max(0.01f, _joinIpSlideDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, target, eased);
            yield return null;
        }

        rt.anchoredPosition = target;
        if (deactivateAfter) _joinIpInput.gameObject.SetActive(false);
        _slideRoutine = null;
    }

    private void TryStartClient(string ip)
    {
        CleanupNetworkState();

        Unity.Netcode.Transports.UTP.UnityTransport transport = GetTransport();
        if (transport != null)
        {
            transport.SetConnectionData(ip, defaultPort);
            transport.ConnectionData.Address = ip;
            transport.ConnectionData.Port = defaultPort;
        }

        _attemptingIp = ip;
        bool success = NetworkManager.Singleton.StartClient();

        if (success)
        {
            lobbyUI.ShowStatusMessage($"<color=green>Iniciando conexión a {ip}...</color>");
            CloseJoinInput();
        }
        else
        {
            _attemptingIp = null;
            CleanupNetworkState();
            lobbyUI.ShowStatusMessage("<color=red>Error al iniciar el Cliente.</color>", true);
        }
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
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;

        if (clientId == nm.LocalClientId && !nm.IsHost)
        {
            string ip = !string.IsNullOrEmpty(_attemptingIp) ? _attemptingIp : GetActiveOrAttemptingIp();
            _connectedIp = ip;
            _attemptingIp = null;
            ApplyJoinButtonVisualState(false);
            lobbyUI.ShowStatusMessage($"Conectado a {ip}");
        }
    }

    private void OnClientDisconnectedLocal(ulong clientId)
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;

        if (clientId == nm.LocalClientId)
        {
            string reason = nm.DisconnectReason;
            bool hostEnded = reason == HostEndedClientMessage;
            if (string.IsNullOrEmpty(reason)) reason = "Conexión perdida";

            CleanupNetworkState();
            _connectedIp = null;
            ApplyJoinButtonVisualState(false);

            if (hostEnded && _confirmModal != null)
            {
                _confirmModal.ShowInfo(reason);
            }
            else
            {
                lobbyUI.ShowStatusMessage(reason, true);
            }
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

        UpdatePlayerCountBadge();
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
            System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    string ipStr = ip.ToString();
                    if (ipStr.StartsWith("192.168.") || ipStr.StartsWith("10.")) return ipStr;
                }
            }
            foreach (System.Net.IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) return ip.ToString();
            }
        }
        catch (Exception e)
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

        UnregisterNetworkCallbacks();

        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnServerClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnServerClientDisconnected;
        }
    }
}
