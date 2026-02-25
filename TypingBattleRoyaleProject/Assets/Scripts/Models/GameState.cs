using UnityEngine;

public abstract class GameState
{
    public GameplayManager manager;

    public GameState(GameplayManager manager)
    {
        this.manager = manager;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
