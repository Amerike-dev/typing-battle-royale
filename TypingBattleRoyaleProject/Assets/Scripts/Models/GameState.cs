public abstract class GameState : IGameState
{
    protected GameplayManager manager;

    public GameState(GameplayManager manager)
    {
        this.manager = manager;
    }

    public virtual void Enter()
    {
    }

    public virtual void Update()
    {
    }

    public virtual void Exit()
    {
    }
}