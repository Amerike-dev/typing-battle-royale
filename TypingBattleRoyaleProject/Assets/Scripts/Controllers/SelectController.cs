using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectController : MonoBehaviour
{
    public static SelectController Instance;
    public Transform[] wizardDisplayGO;
    public Button startButton;
    public GameObject[] readyTexts;
    private IDController localPlayerScript;

    [Header("Puntos de spawn del modelo 3D (opcional)")]
    [Tooltip("Si asignás un Transform por jugador, el modelo 3D se spawnea ahí (posición + rotación). Si lo dejás vacío, cae al wizardDisplayGO[i] con el comportamiento legacy.")]
    [SerializeField] private Transform[] _modelSpawnPoints;

    [Header("Fade al dar Listo (FinishButton)")]
    [Tooltip("CanvasGroup que envuelve LeftButton, RightButton, UpButton, DownButton, CharacterText, SkinText. Su alpha pasa a 0 al dar Listo.")]
    [SerializeField] private CanvasGroup _selectionGroup;
    [Tooltip("RectTransform del FinishButton (el botón 'Listo'). Se mueve +Y al dar Listo.")]
    [SerializeField] private RectTransform _finishButtonRt;
    [Tooltip("Cantidad en píxeles que sube el FinishButton al dar Listo.")]
    [SerializeField] private float _finishButtonUpOffset = 60f;

    [Header("ConfirmButton (host empieza la partida)")]
    [Tooltip("CanvasGroup del ConfirmButton. Pasa a alpha 1 cuando todos están listos.")]
    [SerializeField] private CanvasGroup _confirmButtonGroup;

    [Header("Sin conexión")]
    [Tooltip("Textos/GameObjects 'Sin conexión...' que aparecen en slots vacíos (uno por jugador, 4 total).")]
    [SerializeField] private GameObject[] _disconnectedTexts;

    private bool _localFinishApplied;
    private float _finishButtonBaselineY;
    private bool _finishButtonBaselineCaptured;

    public Transform GetModelSpawnPoint(ulong clientId)
    {
        if (_modelSpawnPoints == null) return null;
        if (clientId >= (ulong)_modelSpawnPoints.Length) return null;
        return _modelSpawnPoints[clientId];
    }

    [Header("UI Unica")]
    [SerializeField] private GameObject arrowsPanel;
    [Tooltip("Posición local del ArrowsContainer cuando se reparenta al slot del jugador local.")]
    [SerializeField] private Vector2 _arrowsLocalPosition = new Vector2(0f, -130f);

    [Header("Back Button (solo visible para el host)")]
    [SerializeField] private Button _backButton;
    [SerializeField] private string _lobbySceneName = "LobbyScene";
    [SerializeField] private Vector2 _backButtonSize = new Vector2(130f, 50f);
    [Tooltip("Offset desde la esquina superior derecha (negativo en X y Y para meterse hacia adentro).")]
    [SerializeField] private Vector2 _backButtonOffset = new Vector2(-20f, -20f);
    [SerializeField] private string _backButtonLabel = "Volver";
    [SerializeField] private Sprite _backButtonSprite;
    [SerializeField] private Color _backButtonColor = new Color(0.12f, 0.12f, 0.16f, 0.95f);
    [SerializeField] private Color _backButtonTextColor = Color.white;
    [SerializeField] private float _backButtonFontSize = 22f;

    private bool _disconnectCallbackRegistered;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        SetupBackButton();
        CaptureFinishButtonBaseline();
        ApplyConfirmButtonGroup(visible: false);
        ResetSelectionGroup();

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            if (startButton != null)
            {
                if (NetworkManager.Singleton.IsServer)
                {
                    startButton.gameObject.SetActive(true);
                    startButton.interactable = false;
                }
                else
                {
                    startButton.gameObject.SetActive(false);
                }
                PlayerAudio.Instance?.ChangeSoundById("Select");
            }
            else
            {
                Debug.LogWarning("[SelectController] startButton no está asignado en el Inspector.");
            }

            foreach (var text in readyTexts)
            {
                if (text != null) text.SetActive(false);
            }

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                SyncPlayer(client.ClientId);
            }

            IDController[] allPlayers = Object.FindObjectsByType<IDController>(FindObjectsSortMode.None);
            foreach (IDController jugador in allPlayers)
            {
                if (jugador.IsOwner)
                {
                    RegisterLocalPlayer(jugador);
                }
                jugador.Update3DModel();
            }

            RefreshSlotVisibility();
            RegisterClientDisconnectListener();
        }
        else
        {
            RefreshSlotVisibility();
        }
    }

    private void CaptureFinishButtonBaseline()
    {
        if (_finishButtonRt == null || _finishButtonBaselineCaptured) return;
        _finishButtonBaselineY = _finishButtonRt.anchoredPosition.y;
        _finishButtonBaselineCaptured = true;
    }

    private void ResetSelectionGroup()
    {
        if (_selectionGroup == null) return;
        _selectionGroup.alpha = 1f;
        _selectionGroup.interactable = true;
        _selectionGroup.blocksRaycasts = true;
        _localFinishApplied = false;

        if (_finishButtonRt != null && _finishButtonBaselineCaptured)
        {
            Vector2 pos = _finishButtonRt.anchoredPosition;
            pos.y = _finishButtonBaselineY;
            _finishButtonRt.anchoredPosition = pos;
        }
    }

    private void SetupBackButton()
    {
        if (_backButton == null) _backButton = BuildBackButton();
        if (_backButton == null) return;

        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
        _backButton.gameObject.SetActive(isHost);
        _backButton.onClick.RemoveAllListeners();
        _backButton.onClick.AddListener(OnBackClicked);
    }

    private Button BuildBackButton()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[SelectController] No se encontró Canvas para construir el botón Back.");
            return null;
        }

        GameObject go = new GameObject("BackButton", typeof(Image), typeof(Button));
        go.transform.SetParent(canvas.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = _backButtonSize;
        rt.anchoredPosition = _backButtonOffset;

        Image img = go.GetComponent<Image>();
        img.color = _backButtonColor;
        if (_backButtonSprite != null)
        {
            img.sprite = _backButtonSprite;
            img.type = Image.Type.Sliced;
        }

        GameObject labelGo = new GameObject("Label", typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);

        RectTransform lrt = labelGo.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = Vector2.zero;
        lrt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = _backButtonLabel;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = _backButtonTextColor;
        tmp.fontSize = _backButtonFontSize;
        tmp.enableAutoSizing = false;
        tmp.raycastTarget = false;

        go.transform.SetAsLastSibling();

        return go.GetComponent<Button>();
    }

    private void OnBackClicked()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null)
        {
            SceneManager.LoadScene(_lobbySceneName);
            return;
        }

        if (!nm.IsServer)
        {
            SceneManager.LoadScene(_lobbySceneName);
            return;
        }

        System.Collections.Generic.List<ulong> ids = new System.Collections.Generic.List<ulong>(nm.ConnectedClientsIds);
        foreach (ulong id in ids)
        {
            if (id == nm.LocalClientId) continue;
            nm.DisconnectClient(id, "El host volvió al lobby");
        }

        nm.Shutdown();
        SceneManager.LoadScene(_lobbySceneName);
    }

    private void RegisterClientDisconnectListener()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;
        if (nm.IsServer) return;
        if (_disconnectCallbackRegistered) return;

        nm.OnClientDisconnectCallback += OnLocalClientDisconnected;
        _disconnectCallbackRegistered = true;
    }

    private void OnLocalClientDisconnected(ulong clientId)
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;
        if (clientId != nm.LocalClientId) return;

        nm.OnClientDisconnectCallback -= OnLocalClientDisconnected;
        _disconnectCallbackRegistered = false;

        SceneManager.LoadScene(_lobbySceneName);
    }

    private void OnDestroy()
    {
        if (_disconnectCallbackRegistered && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnLocalClientDisconnected;
            _disconnectCallbackRegistered = false;
        }
    }

    public void ShowReadyUI(ulong clientId, bool isReady)
    {
        if (readyTexts == null) return;
        if (clientId >= (ulong)readyTexts.Length) return;
        if (readyTexts[clientId] == null) return;

        readyTexts[clientId].SetActive(isReady);

        if (isReady)
        {
            TMP_Text tmp = readyTexts[clientId].GetComponent<TMP_Text>();
            if (tmp == null) tmp = readyTexts[clientId].GetComponentInChildren<TMP_Text>(true);
            if (tmp != null) tmp.text = GetPlayerName(clientId);
        }
    }

    private string GetPlayerName(ulong clientId)
    {
        IDController[] allPlayers = Object.FindObjectsByType<IDController>(FindObjectsSortMode.None);
        foreach (IDController player in allPlayers)
        {
            if (player == null) continue;
            if (player.OwnerClientId != clientId) continue;
            string playerName = player.playerName.Value.ToString();
            if (!string.IsNullOrWhiteSpace(playerName)) return playerName;
            break;
        }
        return $"Jugador {clientId + 1}";
    }

    public void CheckAllPlayersReady()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        int connectedClients = NetworkManager.Singleton.ConnectedClientsList.Count;
        int readyPlayers = 0;

        IDController[] allPlayers = Object.FindObjectsByType<IDController>(FindObjectsSortMode.None);
        foreach (IDController player in allPlayers)
        {
            if (player.already.Value) readyPlayers++;
        }

        bool allReady = (readyPlayers == connectedClients && connectedClients > 0);

        if (startButton != null) startButton.interactable = allReady;

        ApplyConfirmButtonGroup(allReady);
    }

    private void ApplyConfirmButtonGroup(bool visible)
    {
        if (_confirmButtonGroup == null) return;
        _confirmButtonGroup.alpha = visible ? 1f : 0f;
        _confirmButtonGroup.interactable = visible;
        _confirmButtonGroup.blocksRaycasts = visible;
    }

    public void RegisterLocalPlayer(IDController script)
    {
        localPlayerScript = script;

        if (arrowsPanel != null && wizardDisplayGO[script.OwnerClientId] != null)
        {
            arrowsPanel.transform.SetParent(wizardDisplayGO[script.OwnerClientId].transform, false);
            RectTransform rt = arrowsPanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = _arrowsLocalPosition;
                rt.localScale = Vector3.one;
            }

            arrowsPanel.SetActive(true);
        }
    }
    public void SaveAllSelections()
    {
        IDController.savedSelections.Clear();

        IDController[] allPlayers = Object.FindObjectsByType<IDController>(FindObjectsSortMode.None);

        foreach (IDController player in allPlayers)
        {
            IDController.savedSelections[player.OwnerClientId] =
                new IDController.PlayerSelection(player.skinIndex.Value, player.colorIndex.Value);

            Debug.Log($"Guardado Player {player.OwnerClientId} | Skin {player.skinIndex.Value} | Color {player.colorIndex.Value}");
        }
    }
    
    public void UpArrow() => localPlayerScript?.ChangeSelection(1, 0);
    public void DownArrow() => localPlayerScript?.ChangeSelection(-1, 0);
    public void RightArrow() => localPlayerScript?.ChangeSelection(0, 1);
    public void LeftArrow() => localPlayerScript?.ChangeSelection(0, -1);

    public void OKClick()
    {
        Debug.Log("<color=yellow>1. Botón OK presionado físicamente.</color>");

        if (localPlayerScript == null)
        {
            Debug.LogError("<color=red>2. ERROR: localPlayerScript está vacío.</color> El jugador local nunca se registró en el SelectController.");
            return;
        }

        Debug.Log($"<color=yellow>3. El valor actual de 'already' es: {localPlayerScript.already.Value}</color>");

        if (localPlayerScript.already.Value) return;

        localPlayerScript.already.Value = true;
        Debug.Log("<color=yellow>4. Se le ordenó a la variable de red cambiar a TRUE. Esperando que el servidor avise de regreso...</color>");

        ApplyLocalFinishState();
    }

    private void ApplyLocalFinishState()
    {
        if (_localFinishApplied) return;
        _localFinishApplied = true;

        if (_selectionGroup != null)
        {
            _selectionGroup.alpha = 0f;
            _selectionGroup.interactable = false;
            _selectionGroup.blocksRaycasts = false;
        }

        if (_finishButtonRt != null)
        {
            if (!_finishButtonBaselineCaptured) CaptureFinishButtonBaseline();
            Vector2 pos = _finishButtonRt.anchoredPosition;
            pos.y = _finishButtonBaselineY + _finishButtonUpOffset;
            _finishButtonRt.anchoredPosition = pos;
        }
    }

    public void SyncPlayer(ulong ID)
    {
        if (wizardDisplayGO == null) return;
        if (ID >= (ulong)wizardDisplayGO.Length) return;

        RefreshSlotVisibility();

        if (wizardDisplayGO[ID] == null)
        {
            Debug.LogWarning($"[SelectController] wizardDisplayGO[{ID}] no está asignado.");
            return;
        }

        Image tintImage = wizardDisplayGO[ID].GetComponent<Image>();
        if (tintImage == null) return;

        switch (ID)
        {
            case 0:
                tintImage.color = Color.red;
                break;

            case 1:
                tintImage.color = Color.green;
                break;

            case 2:
                tintImage.color = Color.yellow;
                break;

            case 3:
                tintImage.color = Color.blue;
                break;

            default:
                tintImage.color = Color.white;
                break;
        }
    }

    public void RefreshSlotVisibility(ulong? excludeClientId = null)
    {
        if (_disconnectedTexts == null || _disconnectedTexts.Length == 0) return;

        bool[] isConnected = new bool[_disconnectedTexts.Length];

        IDController[] allPlayers = Object.FindObjectsByType<IDController>(FindObjectsSortMode.None);
        foreach (IDController player in allPlayers)
        {
            if (player == null) continue;
            if (excludeClientId.HasValue && player.OwnerClientId == excludeClientId.Value) continue;
            if (player.OwnerClientId < (ulong)isConnected.Length) isConnected[player.OwnerClientId] = true;
        }

        for (int i = 0; i < _disconnectedTexts.Length; i++)
        {
            if (_disconnectedTexts[i] == null) continue;
            _disconnectedTexts[i].SetActive(!isConnected[i]);
        }
    }
}
