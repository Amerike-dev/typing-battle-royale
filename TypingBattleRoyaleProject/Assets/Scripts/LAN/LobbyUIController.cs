using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

public class LobbyUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI localIPText;
    [SerializeField] private TextMeshProUGUI portText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Player Slots")]
    [SerializeField] private TextMeshProUGUI[] _playerSlots;

    [Header("Configuration")]
    [SerializeField] private ushort defaultPort = 7777;

    private void Start()
    {
        DisplayNetworkInformation();
        UpdatePlayerSlots(new NetworkList<ulong>(), 4);
    }

    private void DisplayNetworkInformation()
    {
        string localIP = IPAddressHelper.GetRecommendedLANIP();
        
        if (localIPText != null) localIPText.text = $"IP Local: {localIP}";
        
        if (portText != null) portText.text = $"Puerto: {defaultPort}";
        
        if (instructionsText != null) instructionsText.text = GetInstructions();
        
        IPAddressHelper.PrintAllIPAddresses();
    }

    private string GetInstructions()
    {
        return @"INSTRUCCIONES DE CONEXIÓN LAN:

Para HOST (Servidor):
1. Presiona el botón 'Host'
2. Comparte tu IP con otros jugadores
3. Espera a que se conecten

Para CLIENTE:
1. Ingresa la IP del Host en el campo
2. Presiona el botón 'Join'
3. Espera la conexión

TIPS:
- Asegúrate de que estén en la misma red (misma WiFi/router)
- Desactiva el firewall si hay problemas de conexión
- Usa cables Ethernet para mejor estabilidad";
    }

    public void ShowStatusMessage(string message, bool isError = false)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = isError ? Color.red : Color.green;
        }

        Debug.Log(isError ? $"<color=red>{message}</color>" : $"<color=green>{message}</color>");
    }

    public void UpdatePlayerSlots(NetworkList<ulong> connectedClients, int maxPlayers)
    {
        if (_playerSlots == null)
            return;

        for (int i = 0; i < _playerSlots.Length; i++)
        {
            if (i >= maxPlayers)
            {
                _playerSlots[i].gameObject.SetActive(false);
                continue;
            }

            _playerSlots[i].gameObject.SetActive(true);

            if (i < connectedClients.Count)
            {
                _playerSlots[i].text = $"Player {connectedClients[i]}";
                _playerSlots[i].color = GetPlayerColor(i);
            }
            else
            {
                _playerSlots[i].text = "Waiting";
                _playerSlots[i].color = Color.gray;
            }

        }
    }

    private Color GetPlayerColor(int index)
    {
        Color[] colors = { Color.red };
        return colors[index % colors.Length];
    }
}
