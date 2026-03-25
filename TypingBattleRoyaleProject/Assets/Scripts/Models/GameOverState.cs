using UnityEngine;

public class GameOverState : GameState
{
    private string _winnerID;
    public GameOverState(GameplayManager manager, string winnerID) : base(manager) { }

    public void DebugMessage()
    {
        Debug.Log("Representa el final de la partida.");
    }

    public override void Enter()
    {
        foreach (PlayerController controller in NetworkManagerMock.Instance.Controllers)
        {
            if (controller != null) controller.enabled = false;
        }
        
        if(manager.EndGameCanvas != null) manager.EndGameCanvas.enabled = true;

        if (manager.WinnerText != null)
        {
            string localPlayerID = NetworkManagerMock.Instance.Controllers[0].stats.ID;
            if (_winnerID == localPlayerID)
            {
                manager.WinnerText.text = "¡VICTORIA!";
                manager.WinnerText.color = Color.green;
            }
            else
            {
                manager.WinnerText.text = "DERROTA";
                manager.WinnerText.color = Color.red;
            }
        }
        Time.timeScale = 0f;
    }

    public override void Update() { }

    public override void Exit()
    {
        if(manager.EndGameCanvas != null) manager.EndGameCanvas.enabled = false;
        
        Time.timeScale = 1f;
    }
}
