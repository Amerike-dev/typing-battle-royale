using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

#if UNITY_EDITOR
public class MultiplayerSetupHelper : EditorWindow
{
    [MenuItem("Multiplayer/Setup Assistant")]
    private static void ShowWindow()
    {
        GetWindow<MultiplayerSetupHelper>("Multiplayer Setup");
    }

    private GameObject playerPrefab;
    private GameObject networkManagerObj;
    private PlayerSettings playerSettings;
    private bool showHelp = false;

    private void OnGUI()
    {
        GUILayout.Label("Setup Assistant para Netcode for GameObjects", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        showHelp = EditorGUILayout.Foldout(showHelp, "¿Qué hace este asistente?");
        if (showHelp)
        {
            EditorGUILayout.HelpBox(
                "Este asistente te ayuda a configurar:\n" +
                "1. NetworkManager en la escena\n" +
                "2. Prefab del jugador con componentes de red\n" +
                "3. Características de movimiento y física\n" +
                "4. Sincronización de red",
                MessageType.Info
            );
        }

        EditorGUILayout.Space();
        
        GUILayout.Label("1. Configuración del Jugador", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "El Player Prefab debe tener:\n" +
            "- NetworkObject (sincronización de red)\n" +
            "- NetworkTransform (posición/rotación)\n" +
            "- CharacterController (física)\n" +
            "- NetworkPlayerController (control de movimiento)",
            MessageType.None
        );

        playerPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Player Prefab",
            playerPrefab,
            typeof(GameObject),
            false
        );

        if (playerPrefab != null)
        {
            if (GUILayout.Button("Verificar Configuración del Player Prefab"))
            {
                VerifyPlayerPrefabSetup();
            }
        }

        EditorGUILayout.Space();
        
        GUILayout.Label("2. Configuración de NetworkManager", EditorStyles.boldLabel);
        networkManagerObj = (GameObject)EditorGUILayout.ObjectField(
            "NetworkManager en Escena",
            networkManagerObj,
            typeof(GameObject),
            true
        );

        if (GUILayout.Button("Crear/Encontrar NetworkManager"))
        {
            CreateOrFindNetworkManager();
        }

        if (networkManagerObj != null && playerPrefab != null)
        {
            if (GUILayout.Button("Asignar Player Prefab a NetworkManager"))
            {
                AssignPlayerPrefabToNetworkManager();
            }
        }

        EditorGUILayout.Space();
        
        GUILayout.Label("3. Configuración de PlayerSettings", EditorStyles.boldLabel);
        playerSettings = (PlayerSettings)EditorGUILayout.ObjectField(
            "PlayerSettings",
            playerSettings,
            typeof(PlayerSettings),
            false
        );

        if (GUILayout.Button("Crear PlayerSettings por Defecto"))
        {
            CreateDefaultPlayerSettings();
        }

        if (playerSettings != null && playerPrefab != null)
        {
            if (GUILayout.Button("Aplicar PlayerSettings al Prefab"))
            {
                ApplyPlayerSettingsToPrefab();
            }
        }

        EditorGUILayout.Space();
        
        GUILayout.Label("Verificación Completa", EditorStyles.boldLabel);
        if (GUILayout.Button("Verificar Todo el Sistema"))
        {
            VerifyCompleteSystem();
        }
    }

    private void VerifyPlayerPrefabSetup()
    {
        bool hasNetworkObject = playerPrefab.GetComponent<NetworkObject>() != null;
        bool hasNetworkTransform = playerPrefab.GetComponent<NetworkTransform>() != null;
        bool hasCharacterController = playerPrefab.GetComponent<CharacterController>() != null;
        bool hasPlayerController = playerPrefab.GetComponent<NetworkPlayerController>() != null;

        string report = "Estado del Player Prefab:\n\n";
        report += $"NetworkObject: {(hasNetworkObject ? "✓" : "✗")}\n";
        report += $"NetworkTransform: {(hasNetworkTransform ? "✓" : "✗")}\n";
        report += $"CharacterController: {(hasCharacterController ? "✓" : "✗")}\n";
        report += $"NetworkPlayerController: {(hasPlayerController ? "✓" : "✗")}\n\n";

        if (hasNetworkObject && hasNetworkTransform && hasCharacterController && hasPlayerController)
        {
            report += "El Player Prefab está configurado correctamente.";
            EditorUtility.DisplayDialog("Verificación Completada", report, "OK");
        }
        else
        {
            report += "Faltan componentes. Por favor, revisa la configuración.";
            EditorUtility.DisplayDialog("Advertencia", report, "OK");
        }
    }

    private void CreateOrFindNetworkManager()
    {
        var existingManager = FindFirstObjectByType<NetworkManager>();

        if (existingManager != null)
        {
            networkManagerObj = existingManager.gameObject;
            EditorUtility.DisplayDialog(
                "NetworkManager Encontrado",
                "Se encontró un NetworkManager en la escena.",
                "OK"
            );
            return;
        }
        
        GameObject managerObj = new GameObject("NetworkManager");
        NetworkManager manager = managerObj.AddComponent<NetworkManager>();
        networkManagerObj = managerObj;

        Debug.Log("NetworkManager creado exitosamente.");
        Selection.activeObject = networkManagerObj;
    }

    private void AssignPlayerPrefabToNetworkManager()
    {
        if (networkManagerObj == null || playerPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "NetworkManager y Player Prefab son necesarios.", "OK");
            return;
        }

        NetworkManager manager = networkManagerObj.GetComponent<NetworkManager>();
        if (manager == null)
        {
            EditorUtility.DisplayDialog("Error", "El objeto seleccionado no tiene NetworkManager.", "OK");
            return;
        }
        
        manager.NetworkConfig.PlayerPrefab = playerPrefab;
        Debug.Log("Player Prefab asignado a NetworkManager.");
    }

    private void CreateDefaultPlayerSettings()
    {
        string path = "Assets/Scripts/DefaultPlayerSettings.asset";
        PlayerSettings settings = ScriptableObject.CreateInstance<PlayerSettings>();
        AssetDatabase.CreateAsset(settings, path);
        AssetDatabase.SaveAssets();

        playerSettings = settings;
        Selection.activeObject = settings;

        Debug.Log("PlayerSettings creado en: " + path);
    }

    private void ApplyPlayerSettingsToPrefab()
    {
        if (playerSettings == null || playerPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "PlayerSettings y Player Prefab son necesarios.", "OK");
            return;
        }

        CharacterController controller = playerPrefab.GetComponent<CharacterController>();
        if (controller != null)
        {
            playerSettings.ConfigureCharacterController(controller);
        }

        NetworkTransform networkTransform = playerPrefab.GetComponent<NetworkTransform>();
        if (networkTransform != null)
        {
            playerSettings.ConfigureNetworkTransform(networkTransform);
        }

        Debug.Log("PlayerSettings aplicado al Player Prefab.");
    }

    private void VerifyCompleteSystem()
    {
        string report = "Estado del Sistema Multijugador:\n\n";
        
        report += "=== Player Prefab ===\n";
        if (playerPrefab == null)
        {
            report += "No asignado.\n\n";
        }
        else
        {
            bool hasNetworkObject = playerPrefab.GetComponent<NetworkObject>() != null;
            bool hasNetworkTransform = playerPrefab.GetComponent<NetworkTransform>() != null;
            bool hasCharacterController = playerPrefab.GetComponent<CharacterController>() != null;
            bool hasPlayerController = playerPrefab.GetComponent<NetworkPlayerController>() != null;

            report += $"NetworkObject: {(hasNetworkObject ? "✓" : "✗")}\n";
            report += $"NetworkTransform: {(hasNetworkTransform ? "✓" : "✗")}\n";
            report += $"CharacterController: {(hasCharacterController ? "✓" : "✗")}\n";
            report += $"NetworkPlayerController: {(hasPlayerController ? "✓" : "✗")}\n";
            report += "\n";
        }
        
        report += "=== NetworkManager ===\n";
        if (networkManagerObj == null)
        {
            report += "No asignado.\n\n";
        }
        else
        {
            NetworkManager manager = networkManagerObj.GetComponent<NetworkManager>();
            if (manager == null)
            {
                report += "Objeto no tiene NetworkManager.\n\n";
            }
            else
            {
                bool hasPlayerPrefab = manager.NetworkConfig.PlayerPrefab != null;
                report += $"Player Prefab asignado: {(hasPlayerPrefab ? "✓" : "✗")}\n\n";
            }
        }
        
        report += "=== PlayerSettings ===\n";
        if (playerSettings == null)
        {
            report += "No asignado.\n\n";
        }
        else
        {
            report += $"✓ Configurado\n\n";
        }

        EditorUtility.DisplayDialog("Verificación del Sistema", report, "OK");
    }
}
#endif
