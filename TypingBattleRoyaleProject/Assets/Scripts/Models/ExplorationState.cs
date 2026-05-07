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
        cameraController.OnCamaraMove = true; 
        _gameplayManager.PlayerController.onExplorationState = true;
        _gameplayManager.PlayerController.onExplorationState = false;
    }

    public void Execute(float tick)
    {
        
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        cameraController.OnCamaraMove = false; 
        _gameplayManager.PlayerController.onExplorationState = false;
        
    }
}