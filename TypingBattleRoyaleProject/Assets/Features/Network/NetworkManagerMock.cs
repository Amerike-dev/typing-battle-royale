using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerMock : MonoBehaviour
{
    public static NetworkManagerMock Instance;

    public int playerAmount = 2;
    [SerializeField] private GameObject _playerPrefab;
    public List<PlayerStats> Players;
    public List<PlayerController> Controllers;

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

    void Start()
    {
        

    }

    public void GameInitialize()
    {
        Players = new List<PlayerStats>();
        Controllers = new List<PlayerController>();

        for (int i = 0; i < playerAmount; i++)
        {
            GameObject tempPlayer = Instantiate(_playerPrefab, transform.position, Quaternion.identity);
            string id = PlayerIDGenerator.GenerateID();
            PlayerStats generatedStats = new PlayerStats(id, 100);
            PlayerInventory generatedInventory = new PlayerInventory();
            PlayerController playerController = tempPlayer.GetComponent<PlayerController>();
            playerController.stats = generatedStats;
            playerController.inventory = generatedInventory;

            if (i != 0)
            {
                playerController.enabled = false;
            }

            Players.Add(generatedStats);
            Controllers.Add(playerController);
        }
    }
}
