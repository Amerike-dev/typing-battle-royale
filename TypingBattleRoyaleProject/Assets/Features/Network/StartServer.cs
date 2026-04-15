using Unity.Netcode;
using UnityEngine;

public class StartServer : MonoBehaviour
{
    public void InitServer()
    {
        NetworkManager.Singleton.StartHost();
    }
}
