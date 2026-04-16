using System;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    [Header("LAN Configuration")]
    [SerializeField] private ushort defaultPort = 7777;
    [SerializeField] private string gameScene = "GameplayScene";

    private void Start()
    {
        if (hostButton != null) hostButton.onClick.AddListener(OnJoinButtonClick);

        if (joinButton != null) joinButton.onClick.AddListener(OnJoinButtonClick);

        string localIp = GetLocalIPAdress();

        if(ipInputField != null && string.IsNullOrEmpty(ipInputField.text)) ipInputField.text = localIp;
    }

    private void OnHostButtonClick()
    {
        Unity.Netcode.Transports.UTP.UnityTransport transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();

        if (transport != null) transport.SetConnectionData("0.0.0.0", (ushort)defaultPort);

        bool success = NetworkManager.Singleton.StartHost();

        if (success)
            NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
        else
            Debug.LogError("Error al iniciar el HOST");
    }

    private void OnJoinButtonClick()
    {
        throw new NotImplementedException();
    }

    private string GetLocalIPAdress()
    {
        throw new NotImplementedException();
    }
}
