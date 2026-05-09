using UnityEngine;
using TMPro;
using NUnit.Framework;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;

public class GameplayManager : NetworkBehaviour
{
    public static GameplayManager Instance;
    [SerializeField]public List<GameObject> Monolith = new List<GameObject>();

    [Header("Player references")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private CastInputController _castInputController;
    [SerializeField] private PlayerAnimatorView _playerAnimatorView;

    [Header("UI references")]
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private CanvasGroup _endGameCanvas;
    [SerializeField] private EndGameUI _endGameUI;

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
    public TextMeshProUGUI CountdownText => _countdownText;
    public TextMeshProUGUI WinnerText => _winnerText;
    public CanvasGroup EndGameCanvas => _endGameCanvas;
    public EndGameUI EndGameUI => _endGameUI;

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
        stateMachine.ChangeState(explorationState);
        
        SpawnPlayers();
    }

    private void Update()
    {
        stateMachine?.Update();
    }

    private void InitializeStates()
    {
        explorationState = new ExplorationState(_playerController.cameraController, this);
        waitingState = new WaitingState(this);
        playState = new PlayState(this);
        gameOverState = new GameOverState(this, "");
        stateMachine = new StateMachine(waitingState, 0f);
        battleState = new BattleState(_castInputController, _playerController, _playerAnimatorView);
        if (_castInputController != null) _castInputController.OnSpellCast += HandleOnSpellCast;

        //SetupSpawns();
    }

    private void HandleOnSpellCast()
    {
        Debug.Log("GameplayManager escucho el evento OnSpellCast");
    }

    private void SpawnPlayers()
    {

        StartCoroutine(PopulateSpawnPoint());
        
    }

    private IEnumerator PopulateSpawnPoint()
    {
        //Identificar si eres host o no
        //Si eres host popula spawnPoints sino espera a que este lista

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
            Debug.Log("[Cliente] Esperando confirmación de spawn del servidor...");
            while (!_allSpawned)
            {
                yield return null;
            }
            Debug.Log("[Cliente] Servidor terminó el spawn. Continuando...");
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

        GameObject prefab = arraySkins[selection.skinIndex].gameplayPrefabs[selection.colorIndex];

        GameObject playerInstance = Instantiate(prefab, spawnPosition, Quaternion.identity);

        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        if (networkObject != null) networkObject.SpawnWithOwnership(clientId);
    }
    //Aca terminan los nuevo metodos

    /*private void SetupSpawns()
    {
        Vector3[] position = new Vector3[_spawnPoints.Length];

        for (int i = 0; i < _spawnPoints.Length; i++)
        {
            position[i] = _spawnPoints[i].position;
        }

        SpawnCalculator spawnCalculator = new SpawnCalculator(position);

        foreach (PlayerController controller in NetworkManagerMock.Instance.Controllers)
        {
            Vector3 spawnPoint = spawnCalculator.GetSpawnPoint();
            controller.transform.position = spawnPoint;
        }
    }*/

    public void TriggerGameOver(string winnerID)
    {
        if (gameOverState != null)
        {
            gameOverState.SetWinnerID(winnerID);
            stateMachine.ChangeState(gameOverState);
        }
    }


    private void OnDestroy()
    {
        if (_castInputController != null)
        {
            _castInputController.OnSpellCast -= HandleOnSpellCast;
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
}