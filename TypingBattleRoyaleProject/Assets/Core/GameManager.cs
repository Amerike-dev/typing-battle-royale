using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();
                if (instance == null)
                {
                    Debug.LogError("El GameManager no esta en la escena y no se pudo encontrar.");
                }
            }
            return instance;
        }
    }
    private StateMachine stateMachine;
    private ExplorationState explorationState;
    
    public CameraController camaraController;
    public GameplayManager gameplayManager;
    public InGameTimer gameTimer;

    

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        
        DontDestroyOnLoad(gameObject);

        explorationState = new ExplorationState(camaraController, gameplayManager);
        stateMachine = new StateMachine(explorationState, 1.0f);
        stateMachine.ChangeState(explorationState);
    }

    private void Start()
    {
        StartCoroutine(gameTimer.CountTime());
    }
    private void Update()
    {
        if (stateMachine != null)
        {
            stateMachine.Update();
        }
    }
}
