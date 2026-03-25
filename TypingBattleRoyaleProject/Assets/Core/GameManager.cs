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
                Debug.LogError("El GameManager no esta en la escena");
            }
            return instance;
        }
    }
    private StateMachine stateMachine;
    private ExplorationState explorationState;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        
        DontDestroyOnLoad(gameObject);

        stateMachine = new StateMachine();
        
        explorationState = new ExplorationState();
        stateMachine.ChangeState(explorationState);
    }

    private void Update()
    {
        if (stateMachine != null)
        {
            stateMachine.Update();
        }
    }
}
