using System;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private float continuousSpeed;
    public float jumpForce = 5f;
    private Vector2 _moveInput;
    private Rigidbody _rb;
    private bool _isGrounded;
    public CameraController cameraController;
    public CastInputController castInputController;
    public InputActionReference explorationState;

    [Header("Other")]
    public PlayerAnimatorView playerAnimatorView;

    public bool onExplorationState;
    public PlayerStats stats;
    public PlayerInventory inventory;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
        }
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        
    }

    void OnEnable()
    {
        if (explorationState == null)
        {
            return;
        }
        explorationState.action.started += ExplorationState;
        explorationState.action.Enable();
    }
    void OnDisable()
    {
        if (explorationState == null)
        {
            return;
        }
        explorationState.action.started -= ExplorationState;
        explorationState.action.Disable();
    }
    void Awake()
    {
        onExplorationState = true;
    }

    void FixedUpdate()
    {
        MoveCharacter();
    }

    void MoveCharacter()
    {
        Vector3 moveDirection = (transform.forward * _moveInput.y) + (transform.right * _moveInput.x);
        
        if (moveDirection.magnitude > 1) moveDirection.Normalize();
        
        Vector3 targetVelocity = moveDirection * moveSpeed;
        
        _rb.linearVelocity = new Vector3(targetVelocity.x, _rb.linearVelocity.y, targetVelocity.z);
        
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

    public void ExplorationState(InputAction.CallbackContext context)
    {
        if (onExplorationState)
            GameplayManager.Instance.stateMachine.ChangeState(GameplayManager.Instance.explorationState);
        else
            GameplayManager.Instance.stateMachine.ChangeState(GameplayManager.Instance.battleState);
    }

    public void NullMoveSpeed()
    {
        moveSpeed = 0;
    }

    public void MoveSpeed()
    {
        moveSpeed = continuousSpeed;
    }
}
