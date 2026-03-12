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
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
