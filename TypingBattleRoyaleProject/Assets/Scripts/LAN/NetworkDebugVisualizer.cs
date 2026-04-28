using UnityEngine;
using Unity.Netcode;

public class NetworkDebugVisualizer : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private float uiWidth = 300f;
    [SerializeField] private float uiHeight = 200f;

    [Header("Métricas de Red")]
    private bool showNetworkMetrics = true;
    private bool showPlayerPositions = true;
    //private float updateInterval = 0.5f;
    //private float lastUpdateTime;

    private NetworkManager networkManager;
    private GUIStyle style;

    private void Start()
    {
        networkManager = NetworkManager.Singleton;
        SetupGUIStyle();
    }

    private void SetupGUIStyle()
    {
        style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = textColor;
        style.padding = new RectOffset(10, 10, 10, 10);
        style.normal.background = MakeTexture(2, 2, new Color(0, 0, 0, 0.5f));
    }

    private Texture2D MakeTexture(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showDebugInfo = !showDebugInfo;
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            showNetworkMetrics = !showNetworkMetrics;
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            showPlayerPositions = !showPlayerPositions;
        }
    }

    private void OnGUI()
    {
        if (!showDebugInfo) return;
        if (networkManager == null) return;
        
        DrawBasicNetworkInfo();
        
        if (showNetworkMetrics)
        {
            DrawNetworkMetrics();
        }
        
        if (showPlayerPositions)
        {
            DrawPlayerPositions();
        }
        
        DrawControls();
    }

    private void DrawBasicNetworkInfo()
    {
        string info = "=== Información de Red ===\n";
        info += $"Modo: {GetConnectionMode()}\n";
        info += $"IsHost: {networkManager.IsHost}\n";
        info += $"IsServer: {networkManager.IsServer}\n";
        info += $"IsClient: {networkManager.IsClient}\n";

        if (networkManager.IsClient)
        {
            info += $"Client ID: {networkManager.LocalClientId}\n";
        }

        info += $"Jugadores conectados: {networkManager.ConnectedClients.Count}\n";

        GUI.Label(new Rect(10, 10, uiWidth, uiHeight), info, style);
    }

    private void DrawNetworkMetrics()
    {
        string metrics = "=== Métricas de Red ===\n";

        if (networkManager.IsServer)
        {
            metrics += $"Host: Activo\n";
            metrics += $"FPS: {(1f / Time.deltaTime):F1}\n";
            metrics += $"Delta Time: {Time.deltaTime:F4}s\n";
        }
        else if (networkManager.IsClient)
        {
            metrics += $"Cliente conectado\n";
            metrics += $"Ping: Calculando...\n";
            metrics += $"Paquetes perdidos: 0%\n";
        }

        GUI.Label(new Rect(10, 150, uiWidth, 100), metrics, style);
    }

    private void DrawPlayerPositions()
    {
        string positions = "=== Posiciones de Jugadores ===\n";
        
        NetworkPlayerController[] players = FindObjectsByType<NetworkPlayerController>(0);

        foreach (var player in players)
        {
            NetworkObject netObj = player.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                positions += $"ID: {netObj.OwnerClientId}\n";
                positions += $"  Pos: {player.transform.position}\n";
                positions += $"  IsOwner: {netObj.IsOwner}\n";

                CharacterController controller = player.GetComponent<CharacterController>();
                bool isGrounded = controller != null && controller.isGrounded;
                positions += $"  IsGrounded: {isGrounded}\n\n";
            }
        }

        if (players.Length == 0)
        {
            positions += "No hay jugadores en la escena.\n";
        }

        GUI.Label(new Rect(10, 270, uiWidth, 200), positions, style);
    }

    private void DrawControls()
    {
        string controls = "=== Controles de Debug ===\n";
        controls += "F1: Mostrar/Ocultar Debug\n";
        controls += "F2: Mostrar/Ocultar Métricas\n";
        controls += "F3: Mostrar/Ocultar Posiciones\n";

        GUI.Label(new Rect(10, Screen.height - 80, uiWidth, 80), controls, style);
    }

    private string GetConnectionMode()
    {
        if (networkManager.IsHost) return "Host (Servidor + Cliente)";
        if (networkManager.IsServer) return "Servidor";
        if (networkManager.IsClient) return "Cliente";
        return "Desconectado";
    }

    private void OnDrawGizmos()
    {
        if (!showPlayerPositions) return;
        
        NetworkPlayerController[] players = FindObjectsByType<NetworkPlayerController>(0);

        foreach (var player in players)
        {
            NetworkObject netObj = player.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                Gizmos.color = netObj.IsOwner ? Color.green : Color.red;
                
                Gizmos.DrawWireSphere(player.transform.position, 0.5f);
                
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null && controller.velocity.magnitude > 0.1f)
                {
                    Gizmos.DrawLine(
                        player.transform.position,
                        player.transform.position + controller.velocity.normalized * 2f
                    );
                }
                
                Gizmos.color = Color.yellow;
                if (controller != null && controller.isGrounded)
                {
                    Gizmos.DrawLine(
                        player.transform.position,
                        player.transform.position + Vector3.down * 0.1f
                    );
                }
            }
        }
    }
    
    public void ShowTemporaryMessage(string message, float duration = 2f)
    {
        StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    private System.Collections.IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            GUI.Label(
                new Rect(
                    (Screen.width / 2) - 150,
                    Screen.height - 100,
                    300,
                    50
                ),
                message,
                style
            );
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
