using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameOverState : GameState
{
    private string _winnerID;
    public GameOverState(GameplayManager manager, string winnerID) : base(manager) { }

    public void SetWinnerID(string winnerID)
    {
        _winnerID = winnerID;
    }

    public void DebugMessage()
    {
        Debug.Log("Representa el final de la partida.");
    }

    public override void Enter()
    {
        List<PlayerStats> players = new List<PlayerStats>();

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject == null) continue;

                var pc = client.PlayerObject.GetComponent<PlayerController>();
                if (pc != null) pc.enabled = false;

                var ps = client.PlayerObject.GetComponent<PlayerStats>();
                if (ps != null)
                {
                    players.Add(ps);
                }
                else if (pc != null && pc.stats != null)
                {
                    players.Add(pc.stats);
                }
            }
        }

        if (manager.EndGameCanvas != null) manager.EndGameCanvas.alpha = 1f;

        if (manager.WinnerText != null)
        {
            string localPlayerID = string.Empty;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                var localPc = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
                if (localPc != null && localPc.stats != null)
                {
                    localPlayerID = localPc.stats.ID;
                }
            }

            if (!string.IsNullOrEmpty(localPlayerID) && _winnerID == localPlayerID)
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

        manager.EndGameUI?.Populate(_winnerID, players);
        Time.timeScale = 0f;
    }

    public override void Update() { }

    public override void Exit()
    {
        if(manager.EndGameCanvas != null) manager.EndGameCanvas.alpha = 0f;
        
        Time.timeScale = 1f;
    }
}
