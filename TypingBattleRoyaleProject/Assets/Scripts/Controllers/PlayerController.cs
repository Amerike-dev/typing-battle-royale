using System;
using TMPro.Examples;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
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
    public PlayerStatsNet stats;
    public PlayerInventory inventory;

    public event Action OnEnterBattle;
    public event Action OnExitBattle;

    public void RaiseEnterBattle() => OnEnterBattle?.Invoke();
    public void RaiseExitBattle() => OnExitBattle?.Invoke();

    private float _verticalVelocity;
    private float _x, _z;
    private Vector3 _inputDirection;
    private float _jumpValue = 0.5f;

    [SerializeField] private PlayerInput _playerInput;
    void Start()
    {
        continuousSpeed = moveSpeed;
        _characterController = GetComponent<CharacterController>();

        if (_characterController == null)
        {
            _characterController = gameObject.AddComponent<CharacterController>();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            DisableLocalOnlyComponents();
            return;
        }

        if (explorationState == null)
        {
            return;
        }
        explorationState.action.started += ExplorationState;
        explorationState.action.Enable();
        jumpAction.Enable();

        cameraController = GetComponentInChildren<CameraController>();
        if (cameraController == null) cameraController = FindAnyObjectByType<CameraController>();

        if (cameraController != null)
        {
            if (IsOwner)
            {
                cameraController.isMine = true; 
                cameraController.lookAction.action.Enable(); 
                cameraController.SetTarget(transform);
                cameraController.gameObject.SetActive(true);
            }
            else
            {
                cameraController.isMine = false;
                cameraController.gameObject.SetActive(false);
            }
        }

        if (castInputController == null) castInputController = GetComponentInChildren<CastInputController>(true);
        if (playerAnimatorView == null) playerAnimatorView = GetComponentInChildren<PlayerAnimatorView>(true);

        EnsureSingleAudioListener();

        if (_playerInput == null) _playerInput = GetComponent<PlayerInput>();
        if (IsOwner)
        {
            if (_playerInput != null) _playerInput.enabled = true;
            GameplayManager.Instance.RegisterLocalPlayer(this);
        }
        else
        {
            if (_playerInput != null) _playerInput.enabled = false;
        }
    }

    private void EnsureSingleAudioListener()
    {
        var myListener = GetComponentInChildren<AudioListener>(true);
        var allListeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var listener in allListeners)
        {
            if (listener == null) continue;
            listener.enabled = (listener == myListener);
        }
    }

    private void DisableLocalOnlyComponents()
    {
        var playerInput = GetComponentInChildren<PlayerInput>(true);
        if (playerInput != null) playerInput.enabled = false;

        foreach (var cam in GetComponentsInChildren<Camera>(true))
        {
            cam.enabled = false;
        }

        foreach (var listener in GetComponentsInChildren<AudioListener>(true))
        {
            listener.enabled = false;
        }

        foreach (var canvas in GetComponentsInChildren<Canvas>(true))
        {
            canvas.gameObject.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsOwner) return;

        if (explorationState != null && explorationState.action != null)
        {
            explorationState.action.started -= ExplorationState;
            explorationState.action.Disable();
        }
        if (jumpAction != null) jumpAction.Disable();
    }

    void Awake()
    {
        onExplorationState = true;
        if (inventory == null) inventory = new PlayerInventory();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (onExplorationState) MoveCharacter();

        /* para pruebas de desconexion
        if (IsOwner && Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log("Forzando desconexión local...");
            NetworkManager.Singleton.Shutdown();
        }*/
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
        var gm = GameplayManager.Instance;
        if (gm == null || gm.stateMachine == null) return;

        if (gm.stateMachine.currentState is GameOverState) return;

        onExplorationState = !onExplorationState;

        if (onExplorationState)
            gm.stateMachine.ChangeState(gm.explorationState);
        else
            gm.stateMachine.ChangeState(gm.battleState);
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
