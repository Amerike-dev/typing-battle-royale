using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonolithSpawn : NetworkBehaviour
{
    public GameObject monolithPrefab;
    public List<Transform> spawnMonolithPoints = new List<Transform>();
    public int initialMonoliths = 9;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("[MonolithSpawn] Host listo en la escena. Spawneando monolitos.");
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

            // Instanciamos el objeto de forma local
            GameObject monolith = Instantiate(monolithPrefab, selectedPoint.position, selectedPoint.rotation);

            var networkObject = monolith.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("[MonolithSpawn] ¡Tu prefab no tiene el componente NetworkObject!");
                Destroy(monolith);
                return;
            }

            networkObject.Spawn(true);
            
            // 2. ¡NUEVO! Inicializamos los hechizos de este monolito específico
            var controller = monolith.GetComponent<MonolithController>();
            if (controller != null)
            {
                controller.ServerInitialize();
            }

            Debug.Log($"[MonolithSpawn] Spawneado {monolith.name}");
        }
    }
}
