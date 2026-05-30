using DG.Tweening;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        if (manager.PauseController != null && manager.PauseController._menuContent != null)
        manager.PauseController._menuContent.SetActive(false);

        List<PlayerStatsNet> players = new List<PlayerStatsNet>();

        var allStats = Object.FindObjectsByType<PlayerStatsNet>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        players.AddRange(allStats);

        if (NetworkManager.Singleton != null &&
           NetworkManager.Singleton.LocalClient != null &&
           NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            var pc = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

            if (pc != null)
            {
                pc.ExitSpectatorModeForGameOver();
                pc.enabled = false;
            }
        }

        EndGameResultsData.SetResults(_winnerID, players);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene("PodiumScene");

        /*
        manager.PauseController._menuContent.SetActive(false);
        List<PlayerStatsNet> players = new List<PlayerStatsNet>();

        if (NetworkManager.Singleton != null)
        {
            var allStats = Object.FindObjectsByType<PlayerStatsNet>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            players.AddRange(allStats);
            
            if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                var pc = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.ExitSpectatorModeForGameOver();
                    pc.enabled = false;
                }
            }
        }

        if (manager.EndGameUI != null)
        {
            manager.EndGameUI.gameObject.SetActive(true);
        }

        if (manager.EndGameCanvas != null)
        {
            manager.EndGameCanvas.interactable = true;
            manager.EndGameCanvas.blocksRaycasts = true;
        }

        manager.EndGameUI.Show();

        if (manager.WinnerText != null)
        {
            string localPlayerID = string.Empty;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                var localStats = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerStatsNet>();
                
                if (localStats != null) localPlayerID = localStats.ID;
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

        CursorManager.ShowCursor();

        Time.timeScale = 0f;*/
    }

    public override void Update() { }

    public override void Exit()
    {
        if(manager.EndGameCanvas != null) manager.EndGameCanvas.alpha = 0f;
        
        Time.timeScale = 1f;
    }
}
