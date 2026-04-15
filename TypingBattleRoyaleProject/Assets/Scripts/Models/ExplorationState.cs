using TMPro.Examples;
using UnityEngine;

public class ExplorationState : IGameState
{
    public CameraController cameraController;
    private GameplayManager _gameplayManager;
    public GameObject nearestMonolith = null;
   
    public ExplorationState(CameraController _cameraController, GameplayManager _manager)
    {
        _cameraController = cameraController;
        _gameplayManager = _manager;
    }
    public void Enter()
    { 
        cameraController.OnCamaraMove = true; 
        _gameplayManager.PlayerController.onExplorationState = true;
        
    }

    public void Execute(float tick)
    {
        
    }

    public void Update()
    {

        float distance=float.MaxValue;
        foreach(GameObject Monolith in _gameplayManager.Monolith)
        {
            if( Monolith != null)
            {
                float MonolithDistance = Vector3.Magnitude(_gameplayManager.playerController.transform.position - Monolith.transform.position);
                if (MonolithDistance < distance)
                {
                    distance = MonolithDistance;
                    nearestMonolith = Monolith;
                }
            } 
        }
    }

    public void Exit()
    {
        cameraController.OnCamaraMove = false; 
        _gameplayManager.PlayerController.onExplorationState = false;
        
    }
}