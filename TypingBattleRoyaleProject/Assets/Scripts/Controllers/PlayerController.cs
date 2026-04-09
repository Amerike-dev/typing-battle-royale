using TMPro.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private float continuousSpeed;
    public float jumpForce = 5f;

    public float maxHealth;
    public float currentHealth;

    private Vector2 _moveInput;
    private Rigidbody _rb;
    private bool _isGrounded;
    public CamaraController1 camaraController;
    public CastInputController castInputController;
    public PlayerAnimatorView playerAnimatorView;
    public InputActionReference explorationState;

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
        continuousSpeed = moveSpeed;

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

    public void NullSpeed()
    {
        moveSpeed = 0;
    }

    public void MoveSpeed()
    {
        moveSpeed = continuousSpeed;
    }

}
