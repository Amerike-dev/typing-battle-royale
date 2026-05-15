using UnityEngine;

//Gestionar la selección de objetivos (Lock-on) y el apuntado manual.
//Este sistema será implementado detalladamente en TBR-003. No eliminar.
public class TargetSystem : MonoBehaviour
{
    [Header("Target Setting")]
    [SerializeField] private float _defaultMaxRadius = 30f;

    public Transform target;

    public Transform CurrentTarget { get; private set; }

    void Start()
    {

    }

    void Update()
    {

    }

    // Busca y selecciona al enemigo más cercano dentro de un radio específico.
    public void FindClosestTarget()
    {
        SetTarget(FindClosestTarget(transform.position, _defaultMaxRadius));
    }

    // Activa o desactiva el bloqueo de cámara sobre el objetivo actual
    public void ToggleLockOn()
    {
    }

    public Transform FindClosestTarget(Vector3 from, float maxRadius)
    {
        return null;
    }

    public void Cycle()
    {
        
    }

    public void Clear()
    {
        SetTarget(null);
    }
}
