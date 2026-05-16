using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonolithSpawn : NetworkBehaviour
{
    public GameObject monolithPrefab;
    public List<Transform> spawnMonolithPoints = new List<Transform>();
    public int initialMonoliths = 4;

    public GameplayManager gameplayManager;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        SpawnMonolith();
    }
    public void SpawnMonolith()
    {
        List<Transform> availablePoints = new List<Transform>(spawnMonolithPoints);

        int amountToSpawn = Mathf.Min(initialMonoliths, availablePoints.Count);

        for (int i = 0; i < amountToSpawn; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform selectedPoint = availablePoints[randomIndex];

            availablePoints.RemoveAt(randomIndex);

            Debug.Log($"[Server] Intentando generar monolito en {selectedPoint.position}");
            GameObject monolith = Instantiate(monolithPrefab, selectedPoint.position, selectedPoint.rotation);
            var networkObject = monolith.GetComponent<NetworkObject>();
            if (networkObject == null) Debug.LogError("¡El prefab del monolito no tiene un componente NetworkObject!");
            
            networkObject.Spawn(true);
        }
    }
}
