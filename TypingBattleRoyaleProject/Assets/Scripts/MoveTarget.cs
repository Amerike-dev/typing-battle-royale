using UnityEngine;

public class MoveTarget : MonoBehaviour
{
    [Header("Rotación")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Movimiento vertical")]
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatSpeed = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        RotateMarker();
        FloatMarker();
    }

    private void RotateMarker()
    {
        transform.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void FloatMarker()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
