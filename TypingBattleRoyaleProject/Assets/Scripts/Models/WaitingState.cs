using UnityEngine;

public class WaitingState : GameState
{
    public WaitingState(GameplayManager manager) : base(manager) { }

    public void TestDebug()
    {
        Debug.Log("Representa el inicio tipo \"Juegos del Hambre\" (esperando jugadores en el centro).");
    }
    public override void Enter() { }

    public override void Update() { }

    public override void Exit() { }

}
