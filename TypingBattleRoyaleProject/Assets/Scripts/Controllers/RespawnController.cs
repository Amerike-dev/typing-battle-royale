using UnityEngine;

public class RespawnController : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    private int selectedIndex;
    private Transform spawnTarget;

    private void SelectSpawn()
    {
        selectedIndex = Random.Range(0, spawnPoints.Length);
        spawnTarget = spawnPoints[selectedIndex];
    }

    private void OnTriggerEnter(Collider other)
    {
        IDController player = GetComponent<IDController>();
        if (player != null)
        {
            Debug.Log("Si entre");
            SelectSpawn();
            player.transform.position = spawnTarget.position;
        }
    }
}
