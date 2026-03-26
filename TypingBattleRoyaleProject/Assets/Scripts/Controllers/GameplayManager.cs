using UnityEngine;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    
    [Header("Player references")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private CastInputController _castInputController;
    [SerializeField] private PlayerAnimatorView _playerAnimatorView;
    [SerializeField] private PlayerUI _playerUI;
    
    [Header("UI references")]
    [SerializeField] private TextMeshProUGUI _countdownText;
    [SerializeField] private TextMeshProUGUI _winnerText;
    [SerializeField] private Canvas _endGameCanvas;
    
    [Header("Propiedades")]
    public PlayerController PlayerController => _playerController;
    public CastInputController CastInputController => _castInputController;
    public PlayerUI PlayerUI => _playerUI;
    public PlayerAnimatorView PlayerAnimatorView => _playerAnimatorView;
    public TextMeshProUGUI CountdownText => _countdownText;
    public TextMeshProUGUI WinnerText => _winnerText;
    public Canvas EndGameCanvas => _endGameCanvas;
    
    [Header("Estados")]
    public StateMachine stateMachine;
    public ExplorationState explorationState;
    public BattleState battleState;
    public WaitingState waitingState;

    public PlayerController playerController;
    
    public PlayState playState;
    public GameOverState gameOverState;

    [Header("Timer")]
    [SerializeField] private float matchDuration = 600f;
    private MatchTimer matchTimer;
    public MatchTimer GetMatchTimer() => matchTimer;

    [SerializeField] private Transform[] _spawnPoints;
    
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
        StartMatch();
        InitializeStates();
        stateMachine = new StateMachine(explorationState, 0);


        waitingState.Enter();
    }

    private void Update()
    {
        stateMachine?.Update();
        matchTimer?.Tick(Time.deltaTime);

        InitializeStates();
        stateMachine.ChangeState(waitingState);

    }

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

    private void SetupSpawns()
    {
        Vector3[] position = new Vector3[_spawnPoints.Length];

        for (int i = 0; i < _spawnPoints.Length; i++)
        {
            position[i] = _spawnPoints[i].position;
        }

        SpawnCalculator spawnCalculator = new SpawnCalculator(position);

        NetworkManagerMock.Instance.GameInitialize();

        foreach (PlayerController controller in NetworkManagerMock.Instance.Controllers)
        {
            Vector3 spawnPoint = spawnCalculator.GetSpawnPoint();
            controller.transform.position = spawnPoint;
        }
    }

    public void StartMatch()
    {
        matchTimer = new MatchTimer();
        matchTimer.OnTimerEnd += HandleTimerEnd;
        matchTimer.Start(matchDuration);

        var ui = GameObject.FindObjectOfType<PlayerUI>();
        if (ui != null)
        {
            ui.SetMatchTimer(matchTimer);
        }
    }

    private void HandleTimerEnd()
    {
        Debug.Log("Tiempo terminado");
    }
    
    private void OnDestroy()
    {
        if (_castInputController != null)
        {
            _castInputController.OnSpellCast -= HandleOnSpellCast;
        }
    }
}