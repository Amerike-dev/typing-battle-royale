using TMPro.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Stats Player")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    public float maxHealth;
    public float currentHealth;

    private Vector2 _moveInput;
    private Rigidbody _rb;
    private bool _isGrounded;
    public CamaraController camaraController;
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

    public void ExplorationState(InputAction.CallbackContext context)
    {
        if (onExplorationState)
            GameplayManager.Instance.stateMachine.ChangeState(GameplayManager.Instance.explorationState);
        else
            GameplayManager.Instance.stateMachine.ChangeState(GameplayManager.Instance.battleState);
    }
}
