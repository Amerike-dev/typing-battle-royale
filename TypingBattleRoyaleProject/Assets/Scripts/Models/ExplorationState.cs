using TMPro.Examples;
using UnityEngine;

public class ExplorationState : IGameState
{
    public CameraController cameraController;
    private GameplayManager _gameplayManager;
    public GameObject nearestMonolith = null;

    public ExplorationState(CameraController _cameraController, GameplayManager _manager)
    {
        cameraController = _cameraController;
        _gameplayManager = _manager;
    }

    public void Enter()
    {
        var camera = cameraController != null ? cameraController : (_gameplayManager.PlayerController != null ? _gameplayManager.PlayerController.cameraController : null);
        if (camera != null)
        {
            camera.OnCamaraMove = true;
            camera.ClearBattleTarget();
        }

        if (_gameplayManager.PlayerController != null)
            _gameplayManager.PlayerController.onExplorationState = true;

        var castInput = _gameplayManager.CastInputController;
        if (castInput != null) castInput.enabled = false;

        var spellBook = _gameplayManager.SpellBookUI;
        if (spellBook != null) spellBook.Hide();

        Debug.Log($"[ExplorationState] Enter. camera={camera != null}");
    }
    public void Execute(float tick)
    {
        
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        var camera = cameraController != null ? cameraController : (_gameplayManager.PlayerController != null ? _gameplayManager.PlayerController.cameraController : null);
        if (camera != null) camera.OnCamaraMove = false;

        if (_gameplayManager.PlayerController != null)
            _gameplayManager.PlayerController.onExplorationState = false;
    }
}