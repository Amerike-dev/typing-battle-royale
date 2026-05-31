using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientNPCSpawner : MonoBehaviour
{
    [Header("Debug")]
    public bool enableAmbientNPCs = true;

    [Header("Birds")]
    public BirdNPC[] birdPrefabs;
    public int maxBirds = 40;
    public Transform birdSpawnLeft;
    public Transform birdSpawnRight;

    [Header("Doves")]
    public GameObject[] dovePrefabs;
    public int maxDoves = 9;
    public float doveSpeed = 4f;

    [Header("Fireflies")]
    public FireflyNPC[] fireflyPrefabs;
    public int maxFireflies = 35;
    public BoxCollider fireflyArea;

    [Header("Particles")]
    public ParticleSystem[] airParticlePrefabs;
    public int airParticlesPerPrefab = 3;

    private List<BirdNPC> birdPool = new List<BirdNPC>();
    private List<GameObject> dovePool = new List<GameObject>();
    private List<FireflyNPC> fireflyPool = new List<FireflyNPC>();

    void Start()
    {
        if (!enableAmbientNPCs) return;

        CreateBirdPool();
        CreateDovePool();
        CreateFireflyPool();
        ActivateAllFireflies();
        ActivateAirParticles();
        StartCoroutine(BirdRoutine());
        StartCoroutine(DoveRoutine());
    }

    void CreateBirdPool()
    {
        if (birdPrefabs == null || birdPrefabs.Length == 0) return;

        for (int i = 0; i < maxBirds; i++)
        {
            BirdNPC prefab = birdPrefabs[Random.Range(0, birdPrefabs.Length)];
            BirdNPC bird = Instantiate(prefab, transform);
            bird.gameObject.SetActive(false);
            birdPool.Add(bird);
        }
    }

    void CreateDovePool()
    {
        if (dovePrefabs == null || dovePrefabs.Length == 0) return;

        for (int i = 0; i < maxDoves; i++)
        {
            GameObject prefab = dovePrefabs[Random.Range(0, dovePrefabs.Length)];
            GameObject dove = Instantiate(prefab, transform);
            dove.SetActive(false);
            dovePool.Add(dove);
        }
    }

    void CreateFireflyPool()
    {
        if (fireflyPrefabs == null || fireflyPrefabs.Length == 0) return;

        for (int i = 0; i < maxFireflies; i++)
        {
            FireflyNPC prefab = fireflyPrefabs[Random.Range(0, fireflyPrefabs.Length)];
            FireflyNPC fly = Instantiate(prefab, transform);
            fly.gameObject.SetActive(false);
            fireflyPool.Add(fly);
        }
    }

    void ActivateAllFireflies()
    {
        foreach (var fly in fireflyPool)
            fly.Activate(fireflyArea);
    }

    void ActivateAirParticles()
    {
        if (airParticlePrefabs == null || airParticlePrefabs.Length == 0) return;

        Bounds b = fireflyArea.bounds;

        foreach (var prefab in airParticlePrefabs)
        {
            if (prefab == null) continue;

            for (int i = 0; i < airParticlesPerPrefab; i++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(b.min.x, b.max.x),
                    Random.Range(b.min.y, b.max.y),
                    Random.Range(b.min.z, b.max.z));

                ParticleSystem ps = Instantiate(prefab, randomPos, Quaternion.identity, transform);
                ps.gameObject.SetActive(true);
                ps.Play();
            }
        }
    }

    IEnumerator BirdRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(8f, 20f));

            int flockSize = Random.Range(5, 12);
            for (int i = 0; i < flockSize; i++)
            {
                BirdNPC bird = GetInactiveBird();
                if (bird == null) continue;

                bool leftToRight = Random.value > 0.5f;
                Vector3 start = leftToRight ? birdSpawnLeft.position : birdSpawnRight.position;
                Vector3 dir = leftToRight ? Vector3.right : Vector3.left;

                start += new Vector3(
                    Random.Range(-8f, 8f),
                    Random.Range(-4f, 4f),
                    Random.Range(-8f, 8f));

                bird.Activate(start, dir);

                yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
            }
        }
    }

    IEnumerator DoveRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 6f));

            bool leftToRight = Random.value > 0.5f;
            Vector3 dir = leftToRight ? Vector3.right : Vector3.left;

            Bounds b = fireflyArea.bounds;
            Vector3 groupOrigin = new Vector3(
                leftToRight ? b.min.x : b.max.x,
                Random.Range(b.min.y, b.max.y),
                Random.Range(b.min.z, b.max.z));

            List<GameObject> group = new List<GameObject>();

            for (int i = 0; i < 3; i++)
            {
                GameObject dove = GetInactiveDove();
                if (dove == null) continue;

                Vector3 offset = new Vector3(
                    Random.Range(-1.5f, 1.5f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1.5f, 1.5f));

                dove.transform.position = groupOrigin + offset;
                dove.SetActive(true);
                group.Add(dove);

                yield return new WaitForSeconds(0.05f);
            }

            StartCoroutine(MoveDoveGroup(group, dir));
        }
    }

    IEnumerator MoveDoveGroup(List<GameObject> group, Vector3 dir)
    {
        float despawnDistance = 80f;
        Vector3 startPos = group.Count > 0 ? group[0].transform.position : Vector3.zero;

        while (true)
        {
            bool allGone = true;

            foreach (var dove in group)
            {
                if (dove == null || !dove.activeSelf) continue;

                allGone = false;
                dove.transform.position += dir * doveSpeed * Time.deltaTime;

                float sineY = Mathf.Sin(Time.time * 2f) * 0.3f;
                Vector3 pos = dove.transform.position;
                pos.y += sineY * Time.deltaTime;
                dove.transform.position = pos;

                if (Vector3.Distance(startPos, dove.transform.position) > despawnDistance)
                    dove.SetActive(false);
            }

            if (allGone) yield break;

            yield return null;
        }
    }

    BirdNPC GetInactiveBird()
    {
        foreach (var bird in birdPool)
            if (!bird.gameObject.activeSelf) return bird;
        return null;
    }

    GameObject GetInactiveDove()
    {
        foreach (var dove in dovePool)
            if (!dove.activeSelf) return dove;
        return null;
    }
}