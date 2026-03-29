using TMPro;
using UnityEngine;

public class EnemyLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;

    private Transform _cameraTransform;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;

    }

    private void Update()
    {
        transform.LookAt(_cameraTransform);
    }

    public void SetLabel(string playerId)
    {
        _label.text = playerId;
    }
}
