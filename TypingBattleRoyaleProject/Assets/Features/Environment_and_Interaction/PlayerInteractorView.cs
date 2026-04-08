using UnityEngine;

public class PlayerInteractorView : MonoBehaviour
{
    public MonolithView NearestMonolith { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entraste al Monolito");
        MonolithView monolith = other.GetComponentInParent<MonolithView>();
        if (monolith != null)
        {
            NearestMonolith = monolith;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Saliste del Monolito");
        MonolithView monolith = other.GetComponentInParent<MonolithView>();
        if (monolith != null && NearestMonolith == monolith)
        {
            NearestMonolith = null;
        }
    }
}