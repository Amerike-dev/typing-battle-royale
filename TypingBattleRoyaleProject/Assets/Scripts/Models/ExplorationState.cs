using TMPro.Examples;
using UnityEngine;

public class ExplorationState : IGameState
{
    public CamaraController camaraController;
    private GameplayManager _gameplayManager;
   
    public ExplorationState(CamaraController _camaraController, GameplayManager _manager)
    {
        _camaraController = camaraController;
        _gameplayManager = _manager;
    }
    public void Enter()
    { 
        camaraController.OnCamaraMove = true; 
        _gameplayManager.PlayerController.onExplorationState = true;
        
    }

    public void Execute(float tick)
    {
        
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        camaraController.OnCamaraMove = false; 
        _gameplayManager.PlayerController.onExplorationState = false;
        
    }
}