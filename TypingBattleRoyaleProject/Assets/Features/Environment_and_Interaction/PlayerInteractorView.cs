using UnityEngine;

public class PlayerInteractorView : MonoBehaviour
{
    public MonolithView NearestMonolith { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        MonolithView monolith = other.GetComponent<MonolithView>();
        if (monolith != null)
        {
            NearestMonolith = monolith;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MonolithView monolith = other.GetComponent<MonolithView>();
        if (monolith != null && NearestMonolith == monolith)
        {
            NearestMonolith = null;
        }
    }
}