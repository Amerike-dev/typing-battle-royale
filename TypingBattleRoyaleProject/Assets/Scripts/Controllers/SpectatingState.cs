using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class SpectatingState : GameState
{

    private List<PlayerController> _activePlayers = new List<PlayerController>();
    private int _currentTargetIndex;

    public SpectatingState(GameplayManager manager) : base(manager)
    {
        
    }
    public override void Enter()
    {
        Debug.Log("Entrando en SpectatingState");
        if (manager.PlayerController != null)
            manager.PlayerController.enabled = false;

        DisableAllPlayerCameras();

        _activePlayers = manager.GetActivePlayers();

        Debug.Log("Jugadores activos para espectar: " + _activePlayers.Count);

        if (_activePlayers.Count == 0)
            return;

        _currentTargetIndex = GetClosestPlayerIndex();

        SetSpectatingCamera();
    }

    public override void Update()
    {
        if (_activePlayers == null || _activePlayers.Count == 0)
            return;

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            NextTarget();

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            PreviousTarget();
    }

    public override void Exit()
    {
    }
    
    public override void Execute(float tick)
    {

    }

    private int GetClosestPlayerIndex()
    {
        int closestIndex = 0;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < _activePlayers.Count; i++)
        {
            float distance = Vector3.Distance(
                manager.PlayerController.transform.position,
                _activePlayers[i].transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void NextTarget()
    {
        _currentTargetIndex++;

        if (_currentTargetIndex >= _activePlayers.Count)  _currentTargetIndex = 0;

        SetSpectatingCamera();
    }

    private void PreviousTarget()
    {
        _currentTargetIndex--;

        if (_currentTargetIndex < 0) _currentTargetIndex = _activePlayers.Count - 1;

        SetSpectatingCamera();
    }

    private void SetSpectatingCamera()
    {
        DisableAllPlayerCameras();

        PlayerController target = _activePlayers[_currentTargetIndex];

        if (target == null)
            return;

        Debug.Log("Cambiando cámara a: " + target.name);
            
        target.SetCameraActive(true);
    }

    private void DisableAllPlayerCameras()
    {
        foreach (PlayerController controller in NetworkManagerMock.Instance.Controllers)
        {
            if (controller == null)
                continue;

            controller.SetCameraActive(false);
        }
    }
}
