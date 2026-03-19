using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public float timer;
    public UnityEvent OntimerEnd;
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Debug.Log("Evento Disparado");
            OntimerEnd.Invoke();
        }

    }
}
