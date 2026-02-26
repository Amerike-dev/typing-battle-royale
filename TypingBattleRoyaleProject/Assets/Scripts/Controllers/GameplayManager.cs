using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

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
    }

    private void InitializeStates()
    {
        
    }
}