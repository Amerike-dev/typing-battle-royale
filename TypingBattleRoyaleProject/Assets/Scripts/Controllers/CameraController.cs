using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.1f;
    
    [Header("References")]
    [SerializeField] private Transform playerBody;
    public InputActionReference lookAction;
    public bool OnCamaraMove = true;
    private float returnSpeed = 10f;
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
        ActiveForState();
    }

    void ActiveForState()
    {
        if (OnCamaraMove) { CameraMovement(); }
        if (!OnCamaraMove) { LookAhead(); }
    }

    void CameraMovement()
    {
        _lookInput = lookAction.action.ReadValue<Vector2>();
        float mouseX = _lookInput.x * sensitivity;
        float mouseY = _lookInput.y * sensitivity;
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void LookAhead()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, returnSpeed * Time.deltaTime);
        _xRotation = transform.localEulerAngles.x;
        if (_xRotation > 180f) _xRotation -= 360f;
    }
}
