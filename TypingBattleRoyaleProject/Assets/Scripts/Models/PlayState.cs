using UnityEngine;

public class PlayState : GameState
{
    public PlayerUI playerUI;

    public PlayState(GameplayManager manager) : base(manager) { }

    public void DebugMessage()
    {
        Debug.Log("Representa el bucle principal de 10 minutos de combate y monolitos.");
    }
    public override void Enter()
    {
        manager.playerController.enabled = true;
        playerUI.SpellTypingOFF();
        
        Debug.Log("ENTRÓ A PLAYSTATE");
        manager.StartMatch();
        playerUI.SetMatchTimer(manager.GetMatchTimer());

        Debug.Log("[PlayState] Enter");
    }

    public override void Update()
    {
        
    }

    public override void Exit()
    {
        manager.playerController.enabled = false;
        Debug.Log("[PlayState] Exit");
    }
}
