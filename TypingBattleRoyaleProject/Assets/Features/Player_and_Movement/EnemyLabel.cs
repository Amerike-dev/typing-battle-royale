using TMPro;
using UnityEngine;

public class EnemyLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;

    [SerializeField] private Transform _cameraTransform;

    private void Awake()
    {
        if (_cameraTransform == null && Camera.main != null)
            _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (_cameraTransform == null) return;
        transform.rotation = _cameraTransform.rotation;
    }

    public void SetLabel(string playerId)
    {
        _label.text = playerId;
    }
}
