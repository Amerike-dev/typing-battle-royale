public class BattleState : IGameState
{
    private CastInputController _castInput;
    private PlayerController _playerController;

    public BattleState(CastInputController castInput, PlayerController playerController)
    {
        this._castInput = castInput;
        this._playerController = playerController;
    }

    void IGameState.Enter()
    {
        _castInput.enabled = true;
        _playerController.enabled = false;
    }

    void IGameState.Execute(float tick) { }

    void IGameState.Update() { }

    void IGameState.Exit()
    {
        _castInput.enabled = false;
        _playerController.enabled = true;
    }
}
