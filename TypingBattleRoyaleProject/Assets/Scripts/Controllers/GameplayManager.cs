using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    private GameState _currentState;

    private WaitingState _waitingState;
    private PlayState _playState;
    private GameOverState _gameOverState;

    private void InitializeStates()
    {
        _waitingState = new WaitingState(this);
        _playState = new PlayState(this);
        _gameOverState = new GameOverState(this);
    }

    void Update()
    {
        if (_currentState != null)
        {
            _currentState.Update();
        }
    }

    public void SwitchState(GameState newState)
    {
        if (_currentState != null)
        {
            _currentState.Exit();
        }

        _currentState = newState;

        _currentState.Enter();
    }
}
