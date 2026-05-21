using UnityEngine;

public class PlayState : GameState
{
    private InGameTimer _gameTimer;
    private Coroutine _timerCoroutine;

    public PlayState(GameplayManager manager) : base(manager)
    {
        _gameTimer = new InGameTimer();
        _gameTimer.phasesDurations = new Vector2Int[] { new Vector2Int(10, 0) };
    }

    public void DebugMessage()
    {
        Debug.Log("Representa el bucle principal de 10 minutos de combate y monolitos.");
    }
    public override void Enter()
    {
        if (manager.PlayerController != null)
            manager.PlayerController.enabled = true;
        
        if (_gameTimer != null)
        {
            _gameTimer.OnTimeUp += manager.HandleTimeUp;
            _timerCoroutine = manager.StartCoroutine(_gameTimer.CountTime());
        }
        AudioManager.Instance?.ChangeMusic("music_combat", 0.5f);
        
        Debug.Log("[PlayState] Enter");
    }

    public override void Update()
    {
        
    }

    public override void Exit()
    {
        if (manager.PlayerController != null)
            manager.PlayerController.enabled = false;
        
        if (_timerCoroutine != null) manager.StopCoroutine(_timerCoroutine);
        
        if (_gameTimer != null) _gameTimer.OnTimeUp -= manager.HandleTimeUp;
        
        AudioManager.Instance?.ChangeMusic("music_exploration", 0.5f);
        Debug.Log("[PlayState] Exit");
    }
}
