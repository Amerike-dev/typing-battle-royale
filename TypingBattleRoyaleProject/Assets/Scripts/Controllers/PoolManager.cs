using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    //Para que funcione los objetos que se spawnean deben llamar a PoolManager.Instance.ReturnToPool(tag, gameObject) cuando ya no se necesiten

    public static PoolManager Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public bool expandable = true;
        public int maxSize = 1000;
    }

    public List<Pool> pools;
    private Dictionary<string, IObjectPool<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        poolDictionary = new Dictionary<string, IObjectPool<GameObject>>();

        foreach (Pool pool in pools)
        {
            var objectPool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(pool.prefab),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false, 
                defaultCapacity: pool.size,
                maxSize: pool.expandable ? pool.maxSize : pool.size
            );

            List<GameObject> prewarmedObjects = new List<GameObject>(pool.size);
            for (int i = 0; i < pool.size; i++)
            {
                prewarmedObjects.Add(objectPool.Get());
            }
            foreach (var obj in prewarmedObjects)
            {
                objectPool.Release(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.TryGetValue(tag, out var pool))
        {
            Debug.LogWarning($"No existe el tag {tag} en el PoolManager");
            return null;
        }

        GameObject objectToSpawn = pool.Get();
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.TryGetValue(tag, out var pool))
        {
            Debug.LogWarning($"No existe el tag {tag} en el PoolManager. Destruyendo objeto.");
            Destroy(objectToReturn);
            return;
        }

        pool.Release(objectToReturn);
    }
}