using TMPro.Examples;
using UnityEngine;

public class ExplorationState : IGameState
{
    private CamaraController _CameraController;
   
    public ExplorationState(CameraController cameraController, GameplayManager manager)
    {

    }
    public void Enter()
    {
        if (!_CameraController.OnCamaraMove)
        {
            _CameraController.OnCamaraMove = true;
        }
    }

    public void Execute(float tick)
    {
        
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        if (_CameraController.OnCamaraMove)
        {
            _CameraController.OnCamaraMove = false;
        }
    }
}