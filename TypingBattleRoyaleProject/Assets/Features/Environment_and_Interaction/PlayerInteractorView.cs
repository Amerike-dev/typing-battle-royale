using UnityEngine;
using System.Collections;
public class PlayerInteractorView : MonoBehaviour
{
    public MonolithView NearestMonolith { get; private set; }
    public GameObject[] Monoliths;
    public float proximityRange = 3f;
    public float checkerMonolith = 0.1f;

    /*private void OnTriggerEnter(Collider other)
    {
        MonolithView monolith = other.GetComponent<MonolithView>();
        if (monolith != null && monolith != NearestMonolith)
        {
            NearestMonolith = monolith;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        MonolithView monolith = other.GetComponent<MonolithView>();
        if (monolith != null)
        {
            NearestMonolith = null;
        }
    }*/
    public void NearMonolith()
    {
        float Nearest = 0;
        GameObject NearMonolith;
        foreach (var Monolith in Monoliths)
        {
            float Distance=Vector3.Distance(Monolith.transform.position,transform.position);
            if(Distance < proximityRange)
            {
                if(Nearest < Distance)
                {
                    Nearest = Distance;
                    NearMonolith = Monolith;
                }
                while(true)
                NearestMonolith = Monolith.GetComponent<MonolithView>();
            }
            if(Distance > proximityRange)
            {
                NearestMonolith = null;
            }
        }
    }
}