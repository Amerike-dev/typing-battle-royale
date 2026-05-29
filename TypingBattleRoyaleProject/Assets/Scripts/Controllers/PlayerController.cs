using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private float continuousSpeed;
    public float jumpForce = 5f;
    [Tooltip("Multiplicador sobre la altura máxima del salto. 0.5 = 50% de la altura original.")]
    [Range(0f, 1f)] public float jumpHeightMultiplier = 0.5f;
    private Vector2 _moveInput;
    private CharacterController _characterController;
    private bool _isGrounded;
    public CameraController cameraController;
    public CastInputController castInputController;
    public InputActionReference explorationState;
    public InputAction jumpAction;

    [Header("Spectator")]
    [SerializeField] private SpectatorUI spectatorUI;
    [SerializeField] private SpectatorController spectatorController;

    [Header("Other")]
    public PlayerAnimatorView playerAnimatorView;
    
    [Header("Debug")]
    [SerializeField] private List<string> debugUnlockedSpells = new List<string>();

    public bool onExplorationState;
    public PlayerStatsNet stats;
    public PlayerInventory inventory;

    [Header("Emotes")]
    [Tooltip("Sensibilidad del selector de la rueda al mover el mouse.")]
    [SerializeField] private float emoteWheelSensitivity = 1.2f;
    
    [Tooltip("Altura (en mundo) del emote por encima del nametag del jugador.")]
    [SerializeField] private float emoteHeightOffset = 4.5f;
    
    [Tooltip("Altura objetivo del emote en unidades de mundo.")]
    [SerializeField] private float emoteWorldHeight = 0.7f;
    [Tooltip("Duración total del emote sobre el jugador.")]
    [SerializeField] private float emoteDuration = 2f;
    private EmoteSet _emoteSet;
    private EmoteWheel _emoteWheel;
    private bool _wheelOpen;
    private Vector2 _wheelSelector;
    private GameObject _activeEmote;

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

        // Todos los clientes necesitan el set para mostrar los emotes de cualquier jugador.
        _emoteSet = Resources.Load<EmoteSet>("EmoteSet");

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

            if (spectatorUI == null)
            {
                spectatorUI = FindFirstObjectByType<SpectatorUI>();
            }

            if (spectatorController == null)
            {
                spectatorController = GetComponent<SpectatorController>();
            }

            GameplayManager.Instance.RegisterLocalPlayer(this);

            // Rueda de emotes (solo el jugador local).
            if (_emoteSet != null && _emoteSet.emotes != null && _emoteSet.emotes.Length > 0)
            {
                var holder = new GameObject("EmoteWheel");
                _emoteWheel = holder.AddComponent<EmoteWheel>();
                _emoteWheel.Build(_emoteSet);
            }
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

        if (_emoteWheel != null) Destroy(_emoteWheel.gameObject);

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
        if (inventory == null) inventory = new PlayerInventory(this);
    }
    
    public void UpdateDebugList(List<Spell> currentSpells)
    {
        debugUnlockedSpells.Clear();
        foreach (var spell in currentSpells)
        {
            debugUnlockedSpells.Add(spell.spellName);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleEmotes();

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

        if (playerAnimatorView != null)
        {
            playerAnimatorView.SetGrounded(_isGrounded);
            playerAnimatorView.SetMovement(_x, _z);
        }
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
            // Altura máxima = jumpForce * jumpHeightMultiplier (0.5 -> 50% de la altura original).
            _verticalVelocity = Mathf.Sqrt(jumpForce * 2f * 9.81f * jumpHeightMultiplier);
            AudioManager.Instance?.PlaySFX("sfx_jump");
            if (playerAnimatorView != null) playerAnimatorView.TriggerJump();
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
        
        if (jumpAction != null && jumpAction.enabled) 
        {
            jumpAction.Disable();
        }
    }

    public void MoveSpeed()
    {
        moveSpeed = continuousSpeed;
        
        if (jumpAction != null && !jumpAction.enabled) 
        {
            jumpAction.Enable();
        }
    }

    public void EnterSpectatorMode()
    {

        Debug.Log("[PlayerController] Entering Spectator Mode");

        onExplorationState = false;
        _moveInput = Vector2.zero;
        moveSpeed = 0;

        if(_playerInput != null) _playerInput.enabled = false;

        if (explorationState != null && explorationState.action != null) explorationState.action.Disable();

        if (jumpAction != null) jumpAction.Disable();

        if (_characterController != null) _characterController.enabled = false;

        foreach (var collider in GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false;
        }

        if (IsOwner && spectatorUI != null)
        {
            spectatorUI.Show();
        }

        if (IsOwner && spectatorController != null)
        {
            spectatorController.BeginSpectating(cameraController, spectatorUI);
        }
    }

    public void ExitSpectatorModeForGameOver()
    {
        if (!IsOwner) return;

        if (spectatorController != null)
        {
            spectatorController.StopSpectating();
        }

        if (spectatorUI != null)
        {
            spectatorUI.Hide();
        }

        Debug.Log("[PlayerController] Espectador cerrado por GameOver.");
    }
    
    [ServerRpc]
    public void ClaimMonolithSpellServerRpc(ulong monolithId, int spellIndex, string spellName, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(monolithId, out var obj))
        {
            var monolith = obj.GetComponent<MonolithController>();
            
            if (!monolith.syncedSpellClaimed[spellIndex])
            {
                monolith.MarkSpellAsClaimed(spellIndex);
                UnlockSpellClientRpc(spellName, rpcParams.Receive.SenderClientId);
            }
        }
    }

    [ClientRpc]
    private void UnlockSpellClientRpc(string spellName, ulong ownerId)
    {
        if (NetworkManager.Singleton.LocalClientId != ownerId) return;

        var monoliths = FindObjectsByType<MonolithController>(FindObjectsSortMode.None);
        Spell foundSpell = null;

        foreach (var m in monoliths)
        {
            foundSpell = m.allSpells.FirstOrDefault(s => s != null && s.spellName == spellName);
            if (foundSpell != null) break;
        }

        if (foundSpell != null)
        {
            inventory.AddSpell(foundSpell);
            Debug.Log($"<color=green>¡Éxito!</color> Hechizo {foundSpell.spellName} agregado al inventario desde el monolito.");
        }
        else
        {
            Debug.LogError($"[ERROR] No pudimos encontrar el hechizo {spellName} en NINGÚN monolito de la escena. ¡Revisa que el nombre coincida exactamente!");
        }
    }

    // ---------------- Emotes (rueda local + emote networked sobre el player) ----------------

    private void HandleEmotes()
    {
        if (_emoteWheel == null) return;
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (!_wheelOpen)
        {
            // La rueda solo se abre en exploración, manteniendo el botón central del mouse.
            if (onExplorationState && mouse.middleButton.wasPressedThisFrame) OpenEmoteWheel();
            return;
        }

        // Mientras se mantiene: el mouse mueve el selector (no la cámara).
        Vector2 delta = mouse.delta.ReadValue() * emoteWheelSensitivity;
        _wheelSelector = Vector2.ClampMagnitude(_wheelSelector + delta, 260f);
        int index = _emoteWheel.UpdateSelector(_wheelSelector);

        if (mouse.middleButton.wasReleasedThisFrame) CloseEmoteWheel(index); // suelta -> selecciona
        else if (!onExplorationState) CloseEmoteWheel(-1);                    // cambió de estado -> cancela
    }

    private void OpenEmoteWheel()
    {
        _wheelOpen = true;
        _wheelSelector = Vector2.zero;
        _emoteWheel.Open();
        if (cameraController != null) cameraController.OnCamaraMove = false;
    }

    private void CloseEmoteWheel(int selectedIndex)
    {
        _wheelOpen = false;
        _emoteWheel.Close();
        if (cameraController != null) cameraController.OnCamaraMove = true;

        if (selectedIndex >= 0 && _emoteSet != null && _emoteSet.emotes != null && selectedIndex < _emoteSet.emotes.Length)
            PlayEmoteServerRpc(selectedIndex);
    }

    [ServerRpc]
    private void PlayEmoteServerRpc(int index) => PlayEmoteClientRpc(index);

    [ClientRpc]
    private void PlayEmoteClientRpc(int index) => ShowEmoteAbove(index);

    /// <summary>Muestra el emote sobre el nametag de ESTE jugador en todos los clientes.</summary>
    private void ShowEmoteAbove(int index)
    {
        if (_emoteSet == null || _emoteSet.emotes == null) return;
        if (index < 0 || index >= _emoteSet.emotes.Length) return;

        var emote = _emoteSet.emotes[index];
        if (emote == null || emote.sprite == null) return;

        if (_activeEmote != null) Destroy(_activeEmote);

        var go = new GameObject("EmoteVisual");
        go.transform.SetParent(transform, true);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = emote.sprite;
        sr.sortingOrder = 500;
        _activeEmote = go;

        StartCoroutine(EmoteVisualRoutine(go, sr, emote.anim));
    }

    private Transform GetEmoteAnchor()
    {
        var label = GetComponentInChildren<EnemyLabel>(true);
        return label != null ? label.transform : transform;
    }

    private IEnumerator EmoteVisualRoutine(GameObject go, SpriteRenderer sr, EmoteAnim anim)
    {
        Transform anchor = GetEmoteAnchor();

        // Escala base para que el sprite mida 'emoteWorldHeight' en mundo.
        float baseScale = emoteWorldHeight;
        if (sr.sprite != null && sr.sprite.bounds.size.y > 0.0001f)
            baseScale = emoteWorldHeight / sr.sprite.bounds.size.y;

        float t = 0f;
        while (go != null && t < emoteDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / emoteDuration);

            Vector3 basePos = (anchor != null ? anchor.position : transform.position + Vector3.up * 2f)
                              + Vector3.up * emoteHeightOffset;
            Vector3 animOffset = Vector3.zero;
            float spin = 0f;
            float scaleMul = 1f;
            float alpha = 1f;

            switch (anim)
            {
                case EmoteAnim.PopBounce:
                    scaleMul = Pop(k);
                    break;
                case EmoteAnim.FloatUp:
                    animOffset = Vector3.up * Mathf.Lerp(0f, 0.6f, k);
                    scaleMul = Pop(Mathf.Min(1f, k * 2f)) * (1f + 0.06f * Mathf.Sin(t * 8f));
                    break;
                case EmoteAnim.Shake:
                    animOffset = new Vector3(Mathf.Sin(t * 38f) * 0.06f, 0f, 0f);
                    scaleMul = Pop(k);
                    break;
                case EmoteAnim.Spin:
                    spin = Mathf.Lerp(360f, 0f, 1f - (1f - k) * (1f - k));
                    scaleMul = Pop(k);
                    break;
                case EmoteAnim.DropDown:
                    animOffset = Vector3.up * Mathf.Lerp(0.6f, 0f, 1f - (1f - k) * (1f - k));
                    scaleMul = Mathf.Clamp01(k * 4f);
                    break;
                case EmoteAnim.Fade:
                default:
                    alpha = Mathf.Min(1f, k * 4f);
                    break;
            }

            if (k > 0.8f) alpha = Mathf.Min(alpha, 1f - (k - 0.8f) / 0.2f); // fade-out común

            Camera cam = Camera.main;
            go.transform.position = basePos + animOffset;
            Quaternion face = cam != null ? cam.transform.rotation : Quaternion.identity;
            go.transform.rotation = face * Quaternion.Euler(0f, 0f, spin);
            go.transform.localScale = Vector3.one * (baseScale * scaleMul);
            sr.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));

            yield return null;
        }

        if (go != null) Destroy(go);
        if (_activeEmote == go) _activeEmote = null;
    }

    private static float Pop(float k)
    {
        if (k <= 0f) return 0f;
        if (k < 0.6f) return Mathf.Lerp(0f, 1.2f, k / 0.6f);
        return Mathf.Lerp(1.2f, 1f, (k - 0.6f) / 0.4f);
    }
}
