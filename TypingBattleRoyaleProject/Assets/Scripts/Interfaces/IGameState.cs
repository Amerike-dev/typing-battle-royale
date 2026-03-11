public interface IGameState
{
    void Enter();
    void Execute(float tick);
    void Update();
    void Exit();
}