using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class MonolithSpawn : NetworkBehaviour
{
    public GameObject monolithPrefab;
    public List<Transform> spawnMonolithPoints = new List<Transform>();
    public int initialMonoliths = 9;

    [Header("Distribución por isla")]
    [Tooltip("Distancia máxima entre puntos de spawn para considerarlos parte de la misma isla. Ajustar según el tamaño de las islas en la escena.")]
    public float islandClusterRadius = 25f;

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

        List<Transform> chosenPoints = new List<Transform>();
        for (int i = 0; i < amountToSpawn; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            chosenPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);
        }

        List<List<Transform>> islands = ClusterByProximity(chosenPoints, islandClusterRadius);
        Debug.Log($"[MonolithSpawn] Detectadas {islands.Count} islas a partir de {chosenPoints.Count} puntos (radio {islandClusterRadius}).");

        Dictionary<Elements, int> globalUsage = new Dictionary<Elements, int>();
        foreach (var e in MonolithController.PlayableElements) globalUsage[e] = 0;

        Shuffle(islands);

        foreach (var island in islands)
        {
            HashSet<Elements> usedInIsland = new HashSet<Elements>();
            foreach (var point in island)
            {
                Elements chosen = PickWeightedElement(globalUsage, usedInIsland);
                usedInIsland.Add(chosen);
                globalUsage[chosen]++;

                SpawnSingleMonolith(point, chosen);
            }
        }
    }

    private void SpawnSingleMonolith(Transform selectedPoint, Elements targetElement)
    {
        GameObject monolith = Instantiate(monolithPrefab, selectedPoint.position, selectedPoint.rotation);

        var networkObject = monolith.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("[MonolithSpawn] ¡Tu prefab no tiene el componente NetworkObject!");
            Destroy(monolith);
            return;
        }

        var controller = monolith.GetComponent<MonolithController>();
        if (controller != null) controller.forcedTargetElement = targetElement;

        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(monolith, this.gameObject.scene);
        networkObject.Spawn(true);

        Debug.Log($"[MonolithSpawn] Spawneado {monolith.name} con elemento {targetElement}");
    }

    private List<List<Transform>> ClusterByProximity(List<Transform> points, float radius)
    {
        int n = points.Count;
        int[] parent = new int[n];
        for (int i = 0; i < n; i++) parent[i] = i;

        int Find(int x)
        {
            while (parent[x] != x)
            {
                parent[x] = parent[parent[x]];
                x = parent[x];
            }
            return x;
        }

        void Union(int a, int b)
        {
            int ra = Find(a);
            int rb = Find(b);
            if (ra != rb) parent[ra] = rb;
        }

        float r2 = radius * radius;
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if ((points[i].position - points[j].position).sqrMagnitude <= r2)
                    Union(i, j);
            }
        }

        Dictionary<int, List<Transform>> clusters = new Dictionary<int, List<Transform>>();
        for (int i = 0; i < n; i++)
        {
            int root = Find(i);
            if (!clusters.TryGetValue(root, out var list))
            {
                list = new List<Transform>();
                clusters[root] = list;
            }
            list.Add(points[i]);
        }

        return clusters.Values.ToList();
    }

    private Elements PickWeightedElement(Dictionary<Elements, int> globalUsage, HashSet<Elements> excluded)
    {
        List<Elements> candidates = MonolithController.PlayableElements
            .Where(e => !excluded.Contains(e))
            .ToList();

        if (candidates.Count == 0) candidates = MonolithController.PlayableElements.ToList();

        float total = 0f;
        float[] weights = new float[candidates.Count];
        for (int i = 0; i < candidates.Count; i++)
        {
            weights[i] = 1f / (1f + globalUsage[candidates[i]]);
            total += weights[i];
        }

        float roll = Random.value * total;
        float acc = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            acc += weights[i];
            if (roll <= acc) return candidates[i];
        }
        return candidates[candidates.Count - 1];
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
