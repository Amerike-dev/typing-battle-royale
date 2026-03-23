using UnityEngine;

public class NetworkManagerMock : MonoBehaviour
{
    public static NetworkManagerMock Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        PlayerIDGenerator.ResetIDs();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
