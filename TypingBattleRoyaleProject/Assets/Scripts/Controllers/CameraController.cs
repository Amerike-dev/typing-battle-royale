using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float battleSlerpSpeed = 8f;

    [Header("References")]
    private Transform playerBody;
    public InputActionReference lookAction;
    public bool OnCamaraMove = true;
    private float returnSpeed = 10f;
    private float _xRotation = 0f;
    private Vector2 _lookInput;

    private Transform _battleTarget;
    private bool _hasBattleTarget;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => lookAction.action.Enable();
    void OnDisable() => lookAction.action.Disable();

    void Update()
    {
        if (playerBody == null) return;

        ActiveForState();
    }

    void ActiveForState()
    {
        if (OnCamaraMove)
        {
            CameraMovement();
            return;
        }

        if (_hasBattleTarget && _battleTarget != null)
        {
            LookAtBattleTarget();
        }
        else
        {
            LookAhead();
        }
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

    void LookAtBattleTarget()
    {
        Vector3 toTarget = _battleTarget.position - playerBody.position;
        Vector3 flatToTarget = new Vector3(toTarget.x, 0f, toTarget.z);

        if (flatToTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion targetBodyRot = Quaternion.LookRotation(flatToTarget, Vector3.up);
            playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetBodyRot, battleSlerpSpeed * Time.deltaTime);
        }

        Vector3 camToTarget = _battleTarget.position - transform.position;
        if (camToTarget.sqrMagnitude < 0.0001f) return;

        float horizontalDistance = new Vector2(camToTarget.x, camToTarget.z).magnitude;
        float pitch = -Mathf.Atan2(camToTarget.y, horizontalDistance) * Mathf.Rad2Deg;
        Quaternion targetLocalPitch = Quaternion.Euler(pitch, 0f, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetLocalPitch, battleSlerpSpeed * Time.deltaTime);
        _xRotation = pitch;
    }

    public void SetTarget(Transform newPlayerBody)
    {
        playerBody = newPlayerBody;
    }

    public void SetBattleTarget(Transform target)
    {
        _battleTarget = target;
        _hasBattleTarget = target != null;
    }

    public void ClearBattleTarget()
    {
        _battleTarget = null;
        _hasBattleTarget = false;
    }
}
