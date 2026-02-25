using UnityEngine;

public class PlayState : GameState
{
    public PlayState(GameplayManager manager) : base(manager) { }

    public void DebugMessage()
    {
        Debug.Log("Representa el bucle principal de 10 minutos de combate y monolitos.");
    }
    public override void Enter() { }

    public override void Update() { }

    public override void Exit() { }
}
