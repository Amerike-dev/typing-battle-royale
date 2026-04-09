using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractorView : MonoBehaviour
{
    public MonolithView NearestMonolith { get; private set; }
    private bool isJorge = false;

    void Update()
    {
        if (isJorge)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame) Debug.Log("Presione la e");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entraste al Monolito");
        MonolithView monolith = other.GetComponentInParent<MonolithView>();
        if (monolith != null)
        {
            isJorge = true;
            NearestMonolith = monolith;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Saliste del Monolito");
        MonolithView monolith = other.GetComponentInParent<MonolithView>();
        if (monolith != null && NearestMonolith == monolith)
        {
            isJorge = false;
            NearestMonolith = null; 
        }
    }

}