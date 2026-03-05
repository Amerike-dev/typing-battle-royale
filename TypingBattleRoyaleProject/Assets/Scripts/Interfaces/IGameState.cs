public interface IGameState
{
    void Enter();
    void Excute(float tick);
    void Update();
    void Exit();
}