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
        if(currentState != null)
        {
            currentState.Exit();
        }
        newState.Enter();
        currentState = newState;
    }

    public void Execute(float newTick)
    {
        
    }
}
