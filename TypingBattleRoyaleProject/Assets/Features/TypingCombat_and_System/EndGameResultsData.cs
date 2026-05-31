using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EndGameResultsData
{
    public static string WinnerID { get; private set; }
    public static List<PodiumPlayerResult> RankedPlayers { get; private set; } = new();

    public static void SetResults(string winnerID, List<PlayerStatsNet> players)
    {
        WinnerID = winnerID;
        RankedPlayers.Clear();

        foreach (PlayerStatsNet player in players)
        {
            if (player == null) continue;

            string displayName = player.ID;
            ulong ownerClientId = 0;
            int skinIndex = 0;
            int colorIndex = 0;

            IDController idController = player.GetComponent<IDController>();

            if (idController == null)
                idController = player.GetComponentInParent<IDController>();

            if (idController == null)
                idController = player.GetComponentInChildren<IDController>();

            if (idController != null)
            {
                ownerClientId = idController.OwnerClientId;
                skinIndex = idController.skinIndex.Value;
                colorIndex = idController.colorIndex.Value;

                string customName = idController.playerName.Value.ToString();

                if (!string.IsNullOrWhiteSpace(customName))
                    displayName = customName;
            }
            else
            {
                Debug.LogWarning($"[EndGameResultsData] No se encontró IDController para {player.name}. Se usará skin 0 color 0.");
            }

            RankedPlayers.Add(new PodiumPlayerResult
            {
                playerId = player.ID,
                playerName = displayName,

                kills = player.killCount.Value,
                damageDealt = player.damageDealt.Value,
                wpm = player.wPM.Value,
                isWinner = player.ID == winnerID,

                ownerClientId = ownerClientId,
                skinIndex = skinIndex,
                colorIndex = colorIndex
            });

            Debug.Log($"[EndGameResultsData] Guardado {displayName} | Skin {skinIndex} | Color {colorIndex}");
        }

        RankedPlayers = RankedPlayers
            .OrderByDescending(p => p.isWinner)
            .ThenByDescending(p => p.kills)
            .ThenByDescending(p => p.damageDealt)
            .ThenByDescending(p => p.wpm)
            .ToList();
    }
}
