using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;
    public StateMachine stateMachine;
    public ExplorationState explorationState;
    public BattleState battleState;

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
        
    }
}