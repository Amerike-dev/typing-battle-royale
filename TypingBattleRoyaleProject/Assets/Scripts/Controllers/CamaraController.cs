using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraController : MonoBehaviour
{
    public Transform playerBody;
    private PlayerInputData playerInputData;
    private float returnSpeed = 10f;
    private float mouseSensitivity = 20f;
    private Vector2 lookInput;
    float xRotation = 0f;
    //Si es true la camara sigue al cursor
    public bool OnCamaraMove = true;
    void Update()
    {
        playerInputData.CameraAxis = lookInput;
        ActiveForState();
    }
    void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }
    void ActiveForState()
    {
        if (OnCamaraMove) { CameraMovement(); }
        if (!OnCamaraMove) { LookAhead(); }
    }
    public void CameraMovement()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
    void LookAhead()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, returnSpeed * Time.deltaTime);
        xRotation = transform.localEulerAngles.x;
        if (xRotation > 180f) xRotation -= 360f;
    }
}
