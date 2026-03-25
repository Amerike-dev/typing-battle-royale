using System.Collections;
using UnityEngine;

public class WaitingState : GameState
{
    public WaitingState(GameplayManager manager) : base(manager) { }

    public void TestDebug()
    {
        Debug.Log("Representa el inicio tipo \"Juegos del Hambre\" (esperando jugadores en el centro).");
    }

    public override void Enter()
    {
        if(manager.playerController != null) manager.playerController.enabled = false;
        manager.StartCoroutine(CountdownRoutine());
    }

    public override void Update() { }

    public override void Exit()
    {
        manager.CountdownText.gameObject.SetActive(false);
    }
    
    
    private IEnumerator CountdownRoutine()
    {
        manager.CountdownText.gameObject.SetActive(true);
        
        for (int i = 3; i > 0; i--)
        {
            manager.CountdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        manager.CountdownText.text = "¡Lucha!";
        yield return new WaitForSeconds(1f);
        
        manager.stateMachine.ChangeState(manager.playState);
    }
}
