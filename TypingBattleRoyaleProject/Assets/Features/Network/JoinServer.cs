using Unity.Netcode;
using UnityEngine;

public class JoinServer : MonoBehaviour
{
    public void Join()
    {
        NetworkManager.Singleton.StartClient();
    }
}
