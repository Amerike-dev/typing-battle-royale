using UnityEngine;
using Random = UnityEngine.Random;

public class RespawnController : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    private int selectedIndex;

    public void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.gameObject.SetActive(false);
            selectedIndex = Random.Range(0, spawnPoints.Length);
            Vector3 targetPosition = spawnPoints[selectedIndex].position;
            player.transform.position = targetPosition;
            player.gameObject.SetActive(true);
        }
    }
}
