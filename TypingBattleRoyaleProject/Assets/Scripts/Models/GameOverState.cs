using UnityEngine;

public class GameOverState : GameState
{
    public GameOverState(GameplayManager manager) : base(manager) { }

    public void DebugMessage()
    {
        Debug.Log("Representa el final de la partida.");
    }

    public override void Enter() { }

    public override void Update() { }

    public override void Exit() { }
}
