using System.Collections;
using UnityEngine;

public class WaitingState : GameState
{
    private Coroutine _countDownCoroutine;
    public WaitingState(GameplayManager manager) : base(manager) { }

    public void TestDebug()
    {
        Debug.Log("Representa el inicio tipo \"Juegos del Hambre\" (esperando jugadores en el centro).");
    }

    public override void Enter()
    {
        if(manager.PlayerController != null) manager.PlayerController.enabled = false;
        _countDownCoroutine = manager.StartCoroutine(CountdownRoutine());
        Debug.Log("Comenzando CountDown");
    }

    public override void Update() { }

    public override void Exit()
    {
        if (_countDownCoroutine != null)
        {
            manager.StopCoroutine(_countDownCoroutine);
            _countDownCoroutine = null;
        }
        Debug.Log("Saliendo de WaitingState");
        manager.CountdownText.gameObject.SetActive(false);
    }
    
    
    private IEnumerator CountdownRoutine()
    {
        manager.CountdownText.gameObject.SetActive(true);
        
        for (int i = 3; i > 0; i--)
        {
            Debug.Log(i);
            manager.CountdownText.text = i.ToString();
            yield return FadeNumber();
        }
        manager.CountdownText.text = "¡Lucha!";
        yield return FadeNumber();


        manager.stateMachine.ChangeState(manager.playState);
    }
    private IEnumerator FadeNumber()
    {
        manager._countDownCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(0.4f);
        float duration = 0.6f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            manager._countDownCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / duration);
            yield return null;
        }
        manager._countDownCanvasGroup.alpha = 0f;
    }
}
