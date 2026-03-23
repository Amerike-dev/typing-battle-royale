using UnityEngine;

public class DebbugController : MonoBehaviour
{
    public Vector3[] spawnPoints;

    void Start()
    {
        MonolithData monolith = new MonolithData("01", 2, "Ola, soy Homero Chino");

        PrintPoints(spawnPoints);

        SpawnCalculator spawnCalculator = new SpawnCalculator(spawnPoints);

        PrintPoints(spawnCalculator.shufflePoints);

    }


    void PrintPoints(Vector3[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            print(points[i]);
        }
    }

}
