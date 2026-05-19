using UnityEngine;
using TMPro;
using NUnit.Framework;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class GameplayManager : NetworkBehaviour
{
    public static GameplayManager Instance;
    public List<GameObject> Monolith = new List<GameObject>();

    [Header("Player references")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private CastInputController _castInputController;
    [SerializeField] private PlayerAnimatorView _playerAnimatorView;
    [SerializeField] private TargetSystem _targetSystem;

    [Header("UI references")]
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private CanvasGroup _endGameCanvas;
    [SerializeField] private EndGameUI _endGameUI;
    [SerializeField] private SpellBookUI _spellBookUI;

    [Header("Spell UI (escena)")]
    [SerializeField] private CanvasGroup _spellUICanvasGroup;
    [SerializeField] private TMPro.TMP_InputField _spellInputField;
    [SerializeField] private TextMeshProUGUI _spellDisplayText;
    [SerializeField] private SpellUIController _spellUIController;

    [Header("Propiedades")]
    public PlayerController PlayerController
    {
        get
        {
            return _playerController;
        }
        set
        {
            _playerController = value;
        }
    }
    public CastInputController CastInputController => _castInputController;
    public PlayerAnimatorView PlayerAnimatorView => _playerAnimatorView;
    public TargetSystem TargetSystem => _targetSystem;
    public TextMeshProUGUI CountdownText => _countdownText;
    public TextMeshProUGUI WinnerText => _winnerText;
    public CanvasGroup EndGameCanvas => _endGameCanvas;
    public EndGameUI EndGameUI => _endGameUI;
    public SpellBookUI SpellBookUI => _spellBookUI;

    [Header("Estados")]
    public StateMachine stateMachine;
    public ExplorationState explorationState;
    public BattleState battleState;
    public WaitingState waitingState;
    public PlayState playState;
    public GameOverState gameOverState;

    [SerializeField] private List<Transform> _spawnPoints;

    [SerializeField] private SkinInfo[] arraySkins;
    private List<Vector3> _shuffledPositions = new List<Vector3>();
    private bool _allSpawned = false;
    
    public Dictionary<ulong, NetworkObject> _matchPlayers = new Dictionary<ulong, NetworkObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeStates();
        if (explorationState != null)
        {
            stateMachine.ChangeState(explorationState);
        }
        
        if(IsServer) SpawnPlayers();
    }

    private void Update()
    {
        stateMachine?.Update();
    }

    private void InitializeStates()
    {
        waitingState = new WaitingState(this);
        playState = new PlayState(this);
        gameOverState = new GameOverState(this, "");
        stateMachine = new StateMachine(waitingState, 0f);

        if (_playerController != null)
        {
            explorationState = new ExplorationState(_playerController.cameraController, this);
            battleState = new BattleState(
                _castInputController,
                _playerController,
                _playerAnimatorView,
                _playerController.cameraController,
                _targetSystem,
                _spellBookUI);
        }
    }

    public void RegisterLocalPlayer(PlayerController player)
    {
        if (player == null) return;

        _playerController = player;
        _castInputController = player.castInputController;
        _playerAnimatorView = player.playerAnimatorView;

        if (_castInputController != null)
        {
            if (_spellUICanvasGroup != null) _castInputController.uiCanvasGroup = _spellUICanvasGroup;
            if (_spellInputField != null) _castInputController.castSpell = _spellInputField;
            if (_spellDisplayText != null) _castInputController.spell = _spellDisplayText;
            if (_spellUIController != null)
            {
                _castInputController.uiController = _spellUIController;
                _spellUIController.inputController = _castInputController;
                if (_spellDisplayText != null) _spellUIController.displayText = _spellDisplayText;
            }
        }

        explorationState = new ExplorationState(_playerController.cameraController, this);
        battleState = new BattleState(
            _castInputController,
            _playerController,
            _playerAnimatorView,
            _playerController.cameraController,
            _targetSystem,
            _spellBookUI);

        Debug.Log($"[GameplayManager] RegisterLocalPlayer: camera={_playerController.cameraController != null}, castInput={_castInputController != null}, spellBookUI={_spellBookUI != null}, targetSystem={_targetSystem != null}");

        if (stateMachine == null) stateMachine = new StateMachine(explorationState, 0f);
        else stateMachine.ChangeState(explorationState);
    }

    private void SpawnPlayers()
    {
        StartCoroutine(PopulateSpawnPoint());
    }

    private IEnumerator PopulateSpawnPoint()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (_spawnPoints == null || _spawnPoints.Count == 0)
            {
                Debug.LogError("¡No hay Spawn Points asignados en el GameplayManager!");
                yield break;
            }

            if (_spawnPoints.Count < NetworkManager.Singleton.ConnectedClientsIds.Count) 
                Debug.LogWarning($"[SPAWN] Solo hay {_spawnPoints.Count} puntos para {NetworkManager.Singleton.ConnectedClientsIds.Count} jugadores.");

            _shuffledPositions.Clear();
            
            foreach (var t in _spawnPoints)
            {
                if (t != null) _shuffledPositions.Add(t.position);
            }

            RandomizeSpawnPoints();
            AssignPlayersToSpawnPoints();
            NotifySpawnDoneClientRpc();
            yield return null;
        }
        else 
        {
            while (!_allSpawned)
            {
                yield return null;
            }
        }
    }
    
    [ClientRpc]
    private void NotifySpawnDoneClientRpc()
    {
        if (!IsServer) _allSpawned = true;
    }

    private void RandomizeSpawnPoints()
    {
        for (int i = 0; i < _shuffledPositions.Count; i++)
        {
            int randomIndex = Random.Range(i, _shuffledPositions.Count);
            Vector3 temp = _shuffledPositions[i];
            _shuffledPositions[i] = _shuffledPositions[randomIndex];
            _shuffledPositions[randomIndex] = temp;
        }
        Debug.Log("Spawns mezclados correctamente.");
    }

    private void AssignPlayersToSpawnPoints()
    {
        int spawnIndex = 0;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (spawnIndex >= _spawnPoints.Count)
            {
                Debug.LogWarning("No hay suficientes puntos de spawn para todos los jugadores.");
                return;
            }
            Debug.Log($"[SPAWN] from clientId={NetworkManager.Singleton.LocalClientId}, IsHost={IsHost}, IsServer={IsServer}");
            SpawnSelectedPlayer(clientId, _shuffledPositions[spawnIndex]);
            spawnIndex++;
        }
    }

    private void SpawnSelectedPlayer(ulong clientId, Vector3 spawnPosition)
    {
        if (!IDController.savedSelections.TryGetValue(clientId, out IDController.PlayerSelection selection))
        {
            selection = new IDController.PlayerSelection(0, 0);
        }

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
            {
                client.PlayerObject.Despawn(true);
            }
        }

        GameObject prefab = arraySkins[selection.skinIndex].gameplayPrefabs[selection.colorIndex];

        GameObject playerInstance = Instantiate(prefab, spawnPosition, Quaternion.identity);

        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(clientId);
            
            if (IsServer)
            {
                var ps = playerInstance.GetComponent<PlayerStatsNet>();
                if (ps != null) 
                {
                    ps.networkPlayerID.Value = "Player_" + clientId;
                    ps.OnAllLifeLost += () => CheckLastAlive();
                }
                _matchPlayers[clientId] = networkObject;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_spawnPoints == null || _spawnPoints.Count == 0) return;

        Gizmos.color = Color.cyan;
        foreach (var sp in _spawnPoints)
        {
            if (sp != null)
            {
                Gizmos.DrawWireSphere(sp.position, 0.5f);
                Gizmos.DrawLine(sp.position, sp.position + sp.forward * 1.0f);
            }
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (!IsServer) return;
        
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.ConnectionApprovalCallback = null;
        NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;

        foreach (var c in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (c.PlayerObject != null)
            {
                var ps = c.PlayerObject.GetComponent<PlayerStatsNet>(); 
                if (ps != null) ps.OnAllLifeLost += () => CheckLastAlive();
            }
        }
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            NetworkManager.Singleton.ConnectionApprovalCallback = null;
        }
    }
    
    private void CheckLastAlive()
    {
        if (!IsServer) return;

        var alive = NetworkManager.Singleton.ConnectedClientsList
            .Select(c => c.PlayerObject?.GetComponent<PlayerStatsNet>())
            .Where(ps => ps != null && ps.isAlive.Value).ToList(); 

        if (alive.Count == 1) TriggerGameOver(alive[0].ID); 
    }
    
    public void HandleTimeUp()
    {
        if (!IsServer) return;

        var winner = NetworkManager.Singleton.ConnectedClientsList
            .Select(c => c.PlayerObject?.GetComponent<PlayerStatsNet>())
            .Where(ps => ps != null)
            .OrderByDescending(ps => ps.killCount.Value)
            .ThenByDescending(ps => ps.currentHP.Value)
            .ThenByDescending(ps => ps.wPM.Value) 
            .FirstOrDefault();

        if (winner != null)
        {
            TriggerGameOver(winner.ID);
        }
    }
    
    public void TriggerGameOver(string winnerID)
    {
        if (!IsServer) return; 
        
        EndGameClientRpc(winnerID);
    }

    [ClientRpc]
    private void EndGameClientRpc(string winnerID)
    {
        if (gameOverState != null)
        {
            gameOverState.SetWinnerID(winnerID);
            stateMachine.ChangeState(gameOverState);
        }
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SubmitWPMServerRpc(float newWPM, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var ps = client.PlayerObject?.GetComponent<PlayerStatsNet>();
            if (ps != null) ps.wPM.Value = ps.wPM.Value == 0f ? newWPM : (ps.wPM.Value + newWPM) / 2f;
        }
    }
    
    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"<color=orange>[SERVER] Detectó la desconexión del cliente {clientId}</color>");

        if (_matchPlayers.TryGetValue(clientId, out var netObj))
        {
            if (netObj != null)
            {
                var ps = netObj.GetComponent<PlayerStatsNet>();
                if (ps != null)
                {
                    Debug.Log($"<color=orange>[SERVER] Marcando a {clientId} como muerto</color>");
                    ps.isAlive.Value = false; 
                    ps.OnAllLifeLost?.Invoke();
                }
                Debug.Log($"<color=orange>[SERVER] Despawneando a {clientId}</color>");
                if (netObj.IsSpawned) netObj.Despawn(true);
            }
        }
        else
        {
            Debug.Log($"<color=red>[SERVER] El cliente {clientId} se desconectó pero no estaba en el diccionario _matchPlayers</color>");
        }
        
        CheckLastAlive();
    }
    
    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
     {
         if (stateMachine != null && stateMachine.currentState is PlayState)
         {
             response.Approved = false;
             response.Reason = "Partida en curso";
             return;
         }
         response.Approved = true;
         response.CreatePlayerObject = false;
     }
}