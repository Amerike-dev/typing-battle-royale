using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public float timer;
    public UnityEvent OntimerEnd;
    private bool _finished = true;
    private void Update()
    {
        if(!_finished)
        {
            Tiempo();
        }
    }

    void Tiempo()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Debug.Log("Evento Disparado");
            OntimerEnd.Invoke();
        }
    }
}
