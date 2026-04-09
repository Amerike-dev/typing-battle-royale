using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraController1 : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.1f;
    
    [Header("References")]
    [SerializeField] private Transform playerBody;
    public InputActionReference lookAction;

    private float _xRotation = 0f;
    private Vector2 _lookInput;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => lookAction.action.Enable();
    void OnDisable() => lookAction.action.Disable();

    void Update()
    {
        _lookInput = lookAction.action.ReadValue<Vector2>();
        float mouseX = _lookInput.x * sensitivity;
        float mouseY = _lookInput.y * sensitivity;
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
