using System.Collections.Generic;
using UnityEngine;

public class MonolithSpawn : MonoBehaviour
{
    public GameObject monolithPrefab;
    public List<Transform> spawnMonolithPoints = new List<Transform>();
    public int initialMonoliths = 4;

    public GameplayManager gameplayManager;

    public void SpawnMonolith()
    {
        for (int i = 0; i < initialMonoliths; i++)
        {
            int randomSpawns = Random.Range(0, spawnMonolithPoints.Count);
            Transform selectedPoint = spawnMonolithPoints[randomSpawns];

            GameObject monolith = Instantiate(monolithPrefab, selectedPoint);
            gameplayManager.Monolith.Add(monolith);
        }
    }
}
