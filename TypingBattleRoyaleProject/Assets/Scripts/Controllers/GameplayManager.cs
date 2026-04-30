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
    public PlayerController PlayerController => _playerController;
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

    [SerializeField] private Transform[] _spawnPoints;

    public List<Vector3> spawnsPoints = new List<Vector3>();

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
        /*InitializeStates();
        stateMachine.ChangeState(waitingState);*/
        
        SpawnPlayers();
    }

    /*private void Update()
    {
        stateMachine?.Update();
    }*/

    private void InitializeStates()
    {
        waitingState = new WaitingState(this);
        playState = new PlayState(this);
        gameOverState = new GameOverState(this, "");
        stateMachine = new StateMachine(waitingState, 0f);
        battleState = new BattleState(_castInputController, _playerController, _playerAnimatorView);
        if (_castInputController != null) _castInputController.OnSpellCast += HandleOnSpellCast;

        SetupSpawns();
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
        if (OwnerClientId == 0)
        {
            spawnsPoints.Add(new Vector3(0,0,0));
        }
        yield return new WaitForSeconds(5f);
        
    }

    private void SetupSpawns()
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
    }

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
}