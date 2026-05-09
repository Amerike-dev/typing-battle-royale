using UnityEngine;

public class BattleState : IGameState
{
    private CastInputController _castInput;
    private PlayerController _playerController;
    private PlayerAnimatorView _animatorView;

    public BattleState(CastInputController castInput, PlayerController playerController, PlayerAnimatorView animatorView)
    {
        this._castInput = castInput;
        this._playerController = playerController;
        this._animatorView = animatorView;
    }

    void IGameState.Enter()
    {
        _castInput.enabled = true;
        //_playerController.enabled = false;
        _playerController.NullMoveSpeed();
        _animatorView.TriggerCasting();
        _playerController.onExplorationState = true;
    }

    void IGameState.Execute(float tick) { }

    void IGameState.Update()
    {
        
    }

    void IGameState.Exit()
    {
        _castInput.enabled = false;
        _playerController.MoveSpeed();
        _animatorView.StopCasting();
    }
}
