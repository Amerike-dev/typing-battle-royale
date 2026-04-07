using System.Collections;
using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    
    public Transform target;

    
    public float duration = 1.5f;
    public float height = 3f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            //falta meter al player a un estado en el que no se pueda mover en el aire pero pueda voltear a ver 
            StartCoroutine(MovePlayer(player.transform));
        }
    }

    IEnumerator MovePlayer(Transform player)
    {
        float time = 0f;

        Vector3 startPos = player.position;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            Vector3 currentTargetPos = target.position;
            Vector3 linearPos = Vector3.Lerp(startPos, currentTargetPos, t);
            float parabola = 4 * height * t * (1 - t);

            Vector3 finalPos = new Vector3(
                linearPos.x,
                linearPos.y + parabola,
                linearPos.z
            );

            player.position = finalPos;
            yield return null;
        }

        
        player.position = target.position;
    }
}
