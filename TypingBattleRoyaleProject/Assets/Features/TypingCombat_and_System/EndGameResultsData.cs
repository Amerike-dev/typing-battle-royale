using System.Collections.Generic;
using System.Linq;

public static class EndGameResultsData
{
    public static string WinnerID {get; private set;}
    public static List<PodiumPlayerResult> RankedPlayers {get; private set;} = new();

    public static void SetResults(string winnerID, List<PlayerStatsNet> players)
    {
        WinnerID = winnerID;
        RankedPlayers.Clear();

        foreach(var player in players)
        {
            if (player == null) continue;

            string displayName = player.ID;

            if (player.TryGetComponent<IDController>(out var iDController))
            {
                string customName = iDController.playerName.Value.ToString();

                if (!string.IsNullOrEmpty(customName)) displayName = customName;
            }

            RankedPlayers.Add(new PodiumPlayerResult
            {
                playerId = player.ID,
                playerName = displayName,
                kills = player.killCount.Value,
                damageDealt = player.damageDealt.Value,
                wpm = player.wPM.Value,
                isWinner = player.ID == winnerID
            }
            );
        }

        RankedPlayers = RankedPlayers
            .OrderByDescending(p => p.isWinner)
            .ThenByDescending(p => p.kills)
            .ThenByDescending(p => p.damageDealt)
            .ThenByDescending(p => p.wpm)
            .ToList();

    }
}
