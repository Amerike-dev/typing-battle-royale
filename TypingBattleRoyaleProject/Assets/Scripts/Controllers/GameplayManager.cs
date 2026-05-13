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
    [SerializeField]public List<GameObject> Monolith = new List<GameObject>();

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

        /*var monolithControllers = FindObjectsByType<MonolithController>(FindObjectsSortMode.None);
        foreach(var mc in monolithControllers)
        {
            Monolith.Add(mc.gameObject);
        }*/
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
            if (_castInputController != null) _castInputController.OnSpellCast += HandleOnSpellCast;
        }

        //SetupSpawns();
    }

    public void RegisterLocalPlayer(PlayerController player)
    {
        if (player == null) return;

        _playerController = player;
        _castInputController = player.castInputController;
        _playerAnimatorView = player.playerAnimatorView;

        explorationState = new ExplorationState(_playerController.cameraController, this);
        battleState = new BattleState(
            _castInputController,
            _playerController,
            _playerAnimatorView,
            _playerController.cameraController,
            _targetSystem,
            _spellBookUI);

        if (_castInputController != null)
        {
            _castInputController.OnSpellCast -= HandleOnSpellCast;
            _castInputController.OnSpellCast += HandleOnSpellCast;
        }

        Debug.Log($"[GameplayManager] RegisterLocalPlayer: camera={_playerController.cameraController != null}, castInput={_castInputController != null}, spellBookUI={_spellBookUI != null}, targetSystem={_targetSystem != null}");

        if (stateMachine == null) stateMachine = new StateMachine(explorationState, 0f);
        else stateMachine.ChangeState(explorationState);
    }

    private void HandleOnSpellCast(Spell spell)
    {
        Debug.Log($"GameplayManager escucho el evento OnSpellCast: {(spell != null ? spell.spellName : "null")}");
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
                if (ps != null) ps.OnAllLifeLost += () => CheckLastAlive();
            }
        }
    }

    private void OnDestroy()
    {
        if (_castInputController != null) _castInputController.OnSpellCast -= HandleOnSpellCast;
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

        foreach (var c in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (c.PlayerObject != null)
            {
                var ps = c.PlayerObject.GetComponent<PlayerStatsNet>(); 
                if (ps != null) ps.OnAllLifeLost += () => CheckLastAlive();
            }
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
}