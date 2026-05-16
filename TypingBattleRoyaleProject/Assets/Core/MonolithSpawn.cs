using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonolithSpawn : NetworkBehaviour
{
    public GameObject monolithPrefab;
    public List<Transform> spawnMonolithPoints = new List<Transform>();
    public int initialMonoliths = 4;

    public string gameplaySceneName = "GameplayScene";
    public GameplayManager gameplayManager;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }

    private void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != gameplaySceneName) return;

        NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;

        if (clientsTimedOut.Count > 0)
            Debug.LogWarning($"[MonolithSpawn] {clientsTimedOut.Count} clientes no cargaron a tiempo");

        Debug.Log($"[MonolithSpawn] Todos los clientes listos. Spawneando monolitos.");
        SpawnMonolith();
    }

    public void SpawnMonolith()
    {
        List<Transform> availablePoints = new List<Transform>(spawnMonolithPoints);
        int amountToSpawn = Mathf.Min(initialMonoliths, availablePoints.Count);
        Scene targetScene = SceneManager.GetSceneByName(gameplaySceneName);

        for (int i = 0; i < amountToSpawn; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform selectedPoint = availablePoints[randomIndex];
            availablePoints.RemoveAt(randomIndex);

            GameObject monolith = Instantiate(monolithPrefab, selectedPoint.position, selectedPoint.rotation);

            if (targetScene.IsValid())
                SceneManager.MoveGameObjectToScene(monolith, targetScene);

            var networkObject = monolith.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("[MonolithSpawn] Prefab sin NetworkObject");
                Destroy(monolith);
                return;
            }

            networkObject.Spawn(true);
            Debug.Log($"[MonolithSpawn] Spawneado {monolith.name} en escena:{monolith.scene.name}");
        }
    }
}
