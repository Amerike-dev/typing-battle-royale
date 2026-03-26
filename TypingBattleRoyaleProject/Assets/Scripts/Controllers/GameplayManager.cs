using UnityEngine;
using TMPro;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    public StateMachine stateMachine;
    public ExplorationState explorationState;
    public BattleState battleState;
    public WaitingState waitingState;
    public PlayerController playerController;
    
    public PlayState playState;
    [SerializeField] private TextMeshProUGUI _countdownText;

    public TextMeshProUGUI CountdownText
    {
        get
        {
            return _countdownText;
        }
    }

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
    }

    private void InitializeStates()
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
        
        explorationState = new ExplorationState(playerController.camaraController, this);
        battleState = new BattleState(playerController.castInputController, playerController, playerController.playerAnimatorView);
        waitingState = new WaitingState(this);
        playState = new PlayState(this); 
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
}