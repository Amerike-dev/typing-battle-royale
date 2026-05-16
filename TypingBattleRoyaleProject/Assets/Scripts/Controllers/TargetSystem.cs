using UnityEngine;
using Unity.Netcode;

//Gestionar la selección de objetivos (Lock-on) y el apuntado manual.
//Este sistema será implementado detalladamente en TBR-003. No eliminar.
public class TargetSystem : MonoBehaviour
{
    [Header("Target Setting")]
    [SerializeField] private float _defaultMaxRadius = 30f;

    [Header("Target Marker")]
    [SerializeField] private GameObject _targetMarkerPrefab;
    [SerializeField] private Vector3 _markerOffset = new Vector3(0f, 2.2f, 0f);

    [Header("Retarget")]
    [SerializeField] private float _retargetInterval = 0.25f;

    public Transform target;

    public Transform CurrentTarget { get; private set; }

    private GameObject _currentMarker;

    private float _nextRetargetTime;
    private Vector3 _lastSearchPosition;

    void Start()
    {

    }

    void Update()
    {
        if (CurrentTarget == null)
        return;

        if (Time.time < _nextRetargetTime)
        return;

        _nextRetargetTime = Time.time + _retargetInterval;

        if (!IsTargetStillValid(CurrentTarget))
        {
            Debug.Log("[TargetSystem] Target inválido. Buscando nuevo objetivo...");
            FindClosestTarget();
        }
    }

    public Transform FindClosestTarget(Vector3 from, float maxRadius)
    {
        if(NetworkManager.Singleton == null)
        {
            Debug.LogWarning("[TargetSystem] No existe NetworkManager.Singleton.");
            return null;
        }

        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client == null || client.PlayerObject == null) continue;

            NetworkObject playerObject = client.PlayerObject;

            if (playerObject.OwnerClientId == NetworkManager.Singleton.LocalClientId) continue;

            Transform candidate = playerObject.transform;

            PlayerStatsNet stats = candidate.GetComponent<PlayerStatsNet>();

            if (stats == null)
                continue;

            // Excluir jugadores muertos.
            if (!stats.isAlive.Value)
                continue;

            float distance = Vector3.Distance(from, candidate.position);

            if (distance > maxRadius)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = candidate;
            }
        }

        if (closestTarget != null)
        {
            Debug.Log($"[TargetSystem] Target encontrado: {closestTarget.name}");
        }
        else
        {
            Debug.Log("[TargetSystem] No hay targets vivos en rango.");
        }

        return closestTarget;
    }

    // Busca y selecciona al enemigo más cercano dentro de un radio específico.
    public void FindClosestTarget()
    {
        _lastSearchPosition = transform.position;
        SetTarget(FindClosestTarget(_lastSearchPosition, _defaultMaxRadius));
    }

    public void Cycle()
    {
        
    }

    public void Clear()
    {
        SetTarget(null);
    }

    // Activa o desactiva el bloqueo de cámara sobre el objetivo actual
    public void ToggleLockOn()
    {
        if (CurrentTarget != null)
        {
            Clear();
        }
        else
        {
            FindClosestTarget();
        }
    }

    private bool IsTargetStillValid(Transform targetToCheck)
    {
        if (targetToCheck == null)
            return false;

        PlayerStatsNet stats = targetToCheck.GetComponent<PlayerStatsNet>();

        if (stats == null)
            return false;

        if (!stats.isAlive.Value)
            return false;

        float distance = Vector3.Distance(transform.position, targetToCheck.position);

        if (distance > _defaultMaxRadius)
            return false;

        return true;
    }

    private void SetTarget(Transform newTarget)
    {
        CurrentTarget = newTarget;

        target = CurrentTarget;

        RefreshMarker();
    }

    private void RefreshMarker()
    {
        DestroyMarker();

        if (CurrentTarget == null)
            return;

        CreateMarker(CurrentTarget);
    }

    private void CreateMarker(Transform targetTransform)
    {
        if (_targetMarkerPrefab == null)
        {
            Debug.LogWarning("[TargetSystem] No hay Target Marker Prefab asignado.");
            return;
        }

        _currentMarker = Instantiate(_targetMarkerPrefab);

        _currentMarker.transform.SetParent(targetTransform, false);
        _currentMarker.transform.localPosition = _markerOffset;
        _currentMarker.transform.localRotation = Quaternion.identity;
    }

    private void DestroyMarker()
    {
        if (_currentMarker != null)
        {
            Destroy(_currentMarker);
            _currentMarker = null;
        }
    }
}
