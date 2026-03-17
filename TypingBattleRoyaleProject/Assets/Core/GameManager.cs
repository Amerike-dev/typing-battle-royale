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

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        
        DontDestroyOnLoad(gameObject);

        //Crear estado inicial/exploracion
    }

    private void Update()
    {
        stateMachine?.Update();
    }
}
