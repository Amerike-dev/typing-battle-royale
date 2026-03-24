using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    public StateMachine stateMachine;
    public ExplorationState explorationState;
    public BattleState battleState;
    public WaitingState waitingState;
    public PlayerController playerController;

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
        
        InitializeStates();
        stateMachine = new StateMachine(explorationState, 0);


    }

    private void InitializeStates()
    {
        explorationState = new ExplorationState(playerController.camaraController, this);
        battleState = new BattleState(playerController.castInputController, playerController, playerController.playerAnimatorView);
        waitingState = new WaitingState(this);

    }
}