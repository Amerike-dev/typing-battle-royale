using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientNPCSpawner : MonoBehaviour
{
    [Header("Debug")]
    public bool enableAmbientNPCs = true;

    [Header("Pools")]
    public BirdNPC birdPrefab;
    public FireflyNPC fireflyPrefab;

    [Header("Birds")]
    public int maxBirds = 18;
    public Transform birdSpawnLeft;
    public Transform birdSpawnRight;

    [Header("Fireflies")]
    public int maxFireflies = 12;
    public BoxCollider fireflyArea;

    public List<BirdNPC> birdPool = new List<BirdNPC> ();
    public List<FireflyNPC> fireflyPool = new List<FireflyNPC>();

    void Start()
    {
        if (!enableAmbientNPCs)
            return;

        CreateBirdPool();
        CreateFireflyPool();
        SpawnFireflies();

        StartCoroutine(BirdRoutine());
    }

    void CreateBirdPool()
    {
        for (int i = 0; i < maxBirds; i++)
        {
            BirdNPC bird = Instantiate(birdPrefab, transform);
            bird.gameObject.SetActive(false);
            birdPool.Add(bird);
        }
    }

    void CreateFireflyPool()
    {
        for (int i = 0; i < maxFireflies; i++)
        {
            FireflyNPC fly = Instantiate(fireflyPrefab, transform);
            fly.gameObject.SetActive(false);
            fireflyPool.Add(fly);
        }
    }

    IEnumerator BirdRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(20f, 40f));

            int flockSize = Random.Range(3, 7);

            for (int i = 0; i < flockSize; i++)
            {
                BirdNPC bird = GetBird();

                if (bird == null)
                    continue;

                bool leftToRight = Random.value > 0.5f;

                Vector3 start = leftToRight ?
                    birdSpawnLeft.position :
                    birdSpawnRight.position;

                Vector3 dir = leftToRight ?
                    Vector3.right :
                    Vector3.left;

                start += new Vector3(
                    Random.Range(-5f, 5f),
                    Random.Range(-3f, 3f),
                    Random.Range(-5f, 5f));

                bird.Activate(start, dir);
            }
        }
    }

    void SpawnFireflies()
    {
        int count = Random.Range(8, 13);

        for (int i = 0; i < count; i++)
        {
            FireflyNPC fly = GetFirefly();

            if (fly == null)
                return;

            fly.Activate(fireflyArea);
        }
    }

    BirdNPC GetBird()
    {
        foreach (var bird in birdPool)
        {
            if (!bird.gameObject.activeSelf)
                return bird;
        }

        return null;
    }

    FireflyNPC GetFirefly()
    {
        foreach (var fly in fireflyPool)
        {
            if (!fly.gameObject.activeSelf)
                return fly;
        }

        return null;
    }
}
