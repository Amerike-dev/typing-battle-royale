using TMPro;
using UnityEngine;

public class EnemyLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _label;

    [SerializeField] private Transform _cameraTransform;

    private void Update()
    {
        transform.rotation = _cameraTransform.rotation;
    }

    public void SetLabel(string playerId)
    {
        _label.text = playerId;
    }
}
