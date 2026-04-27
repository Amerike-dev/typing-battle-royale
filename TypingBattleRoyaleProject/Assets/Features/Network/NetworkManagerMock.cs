using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerMock : MonoBehaviour
{
    public static NetworkManagerMock Instance;

    public int playerAmount = 2;

    [SerializeField] private GameObject _playerPrefab;
    

    private Color _localPlayerColor = Color.blue;
    private Color _dummyPlayerColor = Color.orange;

    public List<PlayerStats> Players;
    public List<PlayerController> Controllers;

    public MonolithSpawn monolithSpawn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        PlayerIDGenerator.ResetIDs();
    }

    public void GameInitialize()
    {
        Players = new List<PlayerStats>();
        Controllers = new List<PlayerController>();

        monolithSpawn.SpawnMonolith();

        for (int i = 0; i < playerAmount; i++)
        {
            bool local = (i == 0);

            GameObject tempPlayer = Instantiate(_playerPrefab, transform.position, Quaternion.identity);

            if (tempPlayer.TryGetComponent(out MeshRenderer mesh))
            {
                mesh.material = new Material(mesh.material);

                if (local)
                {
                    mesh.material.color = _localPlayerColor;
                }
                else
                {
                    mesh.material.color = _dummyPlayerColor;
                }
            }

            string id = PlayerIDGenerator.GenerateID();
            PlayerStats generatedStats = new PlayerStats(id);
            PlayerInventory generatedInventory = new PlayerInventory();

            PlayerController playerController = tempPlayer.GetComponent<PlayerController>();
            playerController.stats = generatedStats;
            playerController.inventory = generatedInventory;
            playerController.enabled = false;

            Camera playerCamera = tempPlayer.GetComponentInChildren<Camera>();
            AudioListener audioListener = tempPlayer.GetComponentInChildren<AudioListener>();

            if (!local)
            {
                EnemyLabel label = tempPlayer.GetComponent<EnemyLabel>();
                label.SetLabel(id);
                playerCamera.enabled = false;
                audioListener.enabled = false;
            }
            else
            {
                playerController.enabled = true;
                playerCamera.enabled = true;
                audioListener.enabled = true;

            }

            Players.Add(generatedStats);
            Controllers.Add(playerController);
        }

       
    }
}
