using UnityEngine;

public class IPHolder : MonoBehaviour
{

    public static IPHolder Instance;

    public ulong PlayerID { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerId(ulong ID)
    {
        PlayerID = ID;
        print(PlayerID);
    }

}
