using UnityEngine;
using UnityEngine.Rendering;

public class StateMachine
{
    public IGameState currentState;
    public float Tick {  get; private set; }

    public StateMachine(IGameState currentState, float tick)
    {
        this.currentState = currentState;
        this.Tick = tick;
    }

    public void Update()
    {
        currentState.Update();
    }

    public void ChangeState(IGameState newState)
    {

    }

    public void Execute()
    {
        //este se tiene que ejecutar cada tick
    }
}
