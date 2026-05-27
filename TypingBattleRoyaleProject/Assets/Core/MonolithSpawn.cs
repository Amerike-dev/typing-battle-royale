using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MonolithSpawn : NetworkBehaviour
{
    public GameObject monolithPrefab;
    public List<Transform> spawnMonolithPoints = new List<Transform>();
    public int initialMonoliths = 9;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("[MonolithSpawn] Host iniciando red. Esperando estabilización...");
        StartCoroutine(SpawnWithDelayRoutine());
    }

    private IEnumerator SpawnWithDelayRoutine()
    {
        yield return new WaitForSeconds(0.5f); 
        
        Debug.Log("[MonolithSpawn] Red estable. Spawneando monolitos.");
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
            GameObject monolith = Instantiate(monolithPrefab, selectedPoint.position, selectedPoint.rotation);

            var networkObject = monolith.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("[MonolithSpawn] ¡Tu prefab no tiene el componente NetworkObject!");
                Destroy(monolith);
                return;
            }

            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(monolith, this.gameObject.scene);
            networkObject.Spawn(true);
            var controller = monolith.GetComponent<MonolithController>();
            
            //if (controller != null) controller.ServerInitialize();

            Debug.Log($"[MonolithSpawn] Spawneado {monolith.name}");
        }
    }
}
