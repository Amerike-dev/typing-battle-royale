using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private Vector2 _moveInput;
    private Rigidbody _rb;
    private bool _isGrounded;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
        }
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

    }

    void Update()
    {
        MoveCharacter();
    }

    void MoveCharacter()
    {
        Vector3 movement = new Vector3(_moveInput.x, 0, _moveInput.y);
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        _isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (_isGrounded && value.isPressed)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
