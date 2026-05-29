using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleState : IGameState
{
    private CastInputController _castInput;
    private PlayerController _playerController;
    private PlayerAnimatorView _animatorView;
    private CameraController _cameraController;
    private TargetSystem _targetSystem;
    private SpellBookUI _spellBookUI;
    private GameplayManager _gameplayManager;

    public BattleState(
        CastInputController castInput,
        PlayerController playerController,
        PlayerAnimatorView animatorView,
        CameraController cameraController = null,
        TargetSystem targetSystem = null,
        SpellBookUI spellBookUI = null,
        GameplayManager gameplayManager = null)
    {
        _castInput = castInput;
        _playerController = playerController;
        _animatorView = animatorView;
        _cameraController = cameraController;
        _targetSystem = targetSystem;
        _spellBookUI = spellBookUI;
        _gameplayManager = gameplayManager;
    }

    void IGameState.Enter()
    {
        _playerController.onExplorationState = false;
        _playerController.NullMoveSpeed();

        Debug.Log($"[BattleState] _targetSystem asignado: {_targetSystem != null}");

        if (_animatorView != null) _animatorView.TriggerCasting();

        var camera = _cameraController != null ? _cameraController : _playerController.cameraController;

        if (_targetSystem != null && _playerController != null)
        {
            _targetSystem.SetSource(_playerController.transform);
            _targetSystem.FindClosestTarget();

            Debug.Log($"[BattleState] Target actual después de buscar: {(_targetSystem.CurrentTarget != null ? _targetSystem.CurrentTarget.name : "NULL")}");
        }

        if (camera != null)
        {
            camera.OnCamaraMove = false;

            Transform t = _targetSystem != null ? _targetSystem.CurrentTarget : null;
            camera.SetBattleTarget(t);
        }

        IReadOnlyList<Spell> inventorySpells = null;
        if (_playerController.inventory != null)
        {
            inventorySpells = _playerController.inventory.GetUnlockedSpells();
        }

        if (_spellBookUI != null)
        {
            if (_castInput != null) _castInput.enabled = false;
            _spellBookUI.OnSpellConfirmed -= HandleSpellConfirmed;
            _spellBookUI.OnSelectionCancelled -= HandleSelectionCancelled;
            _spellBookUI.OnSpellConfirmed += HandleSpellConfirmed;
            _spellBookUI.OnSelectionCancelled += HandleSelectionCancelled;
            _spellBookUI.Show(inventorySpells);
        }
        else
        {
            EnableCast();
        }

        Debug.Log($"[BattleState] Enter. camera={camera != null}, ui={_spellBookUI != null}, inventoryCount={(inventorySpells != null ? inventorySpells.Count : 0)}");

        _playerController.RaiseEnterBattle();
    }

    void IGameState.Execute(float tick) { }

    void IGameState.Update() 
    {

        if (_targetSystem == null)
            return;

        if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("[BattleState] Tecla 1 presionada. Ciclando target.");

            _targetSystem.Cycle();

            var camera = _cameraController != null ? _cameraController : _playerController.cameraController;

            if (camera != null)
            {
                camera.SetBattleTarget(_targetSystem.CurrentTarget);
            }
        }
    }

    void IGameState.Exit()
    {
        if (_castInput != null) _castInput.CancelCastIfActive();
        _playerController.MoveSpeed();
        if (_animatorView != null) _animatorView.StopCasting();

        var camera = _cameraController != null ? _cameraController : _playerController.cameraController;
        if (camera != null)
        {
            camera.ClearBattleTarget();
        }

        if (_targetSystem != null)
        {
            _targetSystem.Clear();
        }

        if (_spellBookUI != null)
        {
            _spellBookUI.OnSpellConfirmed -= HandleSpellConfirmed;
            _spellBookUI.OnSelectionCancelled -= HandleSelectionCancelled;
            _spellBookUI.Hide();
        }

        Debug.Log("[BattleState] Exit");
        _playerController.RaiseExitBattle();
    }

    private void EnableCast()
    {
        if (_castInput == null) return;

        // Sin spell por defecto: si no hay currentSpell, no habrá texto que tipear (no se castea
        // hasta obtener una de un monolito).
        _castInput.enabled = true;
    }

    private void HandleSpellConfirmed(Spell spell)
    {
        if (_castInput == null) return;

        if (spell == null)
        {
            Debug.LogWarning("[BattleState] Slot vacío confirmado y sin spell. No se puede castear.");
            return;
        }

        _castInput.currentSpell = spell;
        _castInput.spellText = spell.runeString;

        if (string.IsNullOrEmpty(_castInput.spellText))
        {
            Debug.LogError($"[BattleState] Spell has empty runeString. Set the 'Rune String' field on the Spell ScriptableObject (spellName='{(_castInput.currentSpell != null ? _castInput.currentSpell.spellName : "?")}'). Cancelling cast.");
            return;
        }

        Debug.Log($"[BattleState] Spell confirmed: '{_castInput.spellText}'");

        if (_spellBookUI != null) _spellBookUI.Hide();
        _castInput.enabled = true;
    }

    private void HandleSelectionCancelled()
    {
        var gm = GameplayManager.Instance;
        if (gm == null || gm.stateMachine == null) return;
        
        if (gm.stateMachine.currentState == gm.gameOverState) return;

        _playerController.onExplorationState = true;
        gm.stateMachine.ChangeState(gm.explorationState);
    }
}
