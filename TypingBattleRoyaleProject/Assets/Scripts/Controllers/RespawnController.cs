using UnityEngine;

public class RespawnController : MonoBehaviour
{
    [SerializeField] private Vector3[] spawnPoints;
    private int selectedIndex;

    public void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            selectedIndex = Random.Range(0, spawnPoints.Length);
            player.transform.position = spawnPoints[selectedIndex];
        }
    }
}
