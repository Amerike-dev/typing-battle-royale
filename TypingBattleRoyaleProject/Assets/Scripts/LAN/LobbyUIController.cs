using UnityEngine;
using TMPro;

public class LobbyUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI localIPText;
    [SerializeField] private TextMeshProUGUI portText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Configuration")]
    [SerializeField] private ushort defaultPort = 7777;

    private void Start()
    {
        DisplayNetworkInformation();
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
}
