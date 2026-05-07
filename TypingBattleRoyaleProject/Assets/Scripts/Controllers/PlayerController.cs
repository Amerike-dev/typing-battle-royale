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
    private CharacterController _characterController;
    private bool _isGrounded;
    public CameraController cameraController;
    public CastInputController castInputController;
    public InputActionReference explorationState;
    public InputAction jumpAction;

    [Header("Other")]
    public PlayerAnimatorView playerAnimatorView;

    public bool onExplorationState;
    public PlayerStats stats;
    public PlayerInventory inventory;

    private float _verticalVelocity;
    private float _x, _z;
    private Vector3 _inputDirection;
    private float _jumpValue = 0.5f;
    void Start()
    {
        _characterController = GetComponent<CharacterController>();

        if (_characterController == null)
        {
            _characterController = gameObject.AddComponent<CharacterController>();
        }
    }

    void OnEnable()
    {
        if (explorationState == null)
        {
            return;
        }
        explorationState.action.started += ExplorationState;
        explorationState.action.Enable();
        jumpAction.Enable();
    }
    void OnDisable()
    {
        if (explorationState == null)
        {
            return;
        }
        explorationState.action.started -= ExplorationState;
        explorationState.action.Disable();
        jumpAction.Disable();
    }
    void Awake()
    {
        onExplorationState = true;
    }

    void Update()
    {
        MoveCharacter();
    }

    void MoveCharacter()
    {
        _isGrounded = _characterController.isGrounded;

        Jump();

        _x = _moveInput.x;
        _z = _moveInput.y;

        Vector3 horizontalMovement = transform.right * _x + transform.forward * _z;

        Vector3 movement = horizontalMovement * moveSpeed;

        movement.y = _verticalVelocity;
        _characterController.Move(movement * Time.deltaTime);
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void Jump()
    {
        if (_isGrounded)
        {
            _verticalVelocity = -2f;
        }
        if (_isGrounded && (jumpAction.ReadValue<float>() > _jumpValue))
        {
            _verticalVelocity = Mathf.Sqrt(jumpForce * -2f * -9.81f);
        }

        _verticalVelocity += -9.81f * Time.deltaTime;
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
