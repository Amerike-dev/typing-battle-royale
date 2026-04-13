using TMPro.Examples;
using UnityEngine;

public class ExplorationState : IGameState
{
    public CamaraController camaraController;
    private GameplayManager _gameplayManager;
    public GameObject nearestMonolith = null;
   
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
        camaraController.OnCamaraMove = false; 
        _gameplayManager.PlayerController.onExplorationState = false;
        
    }
}