using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using Fusion.Sockets;
using System;
using System.Collections;

public class FusionLauncher : MonoBehaviour
{
    public GameObject playerPrefab;

    private NetworkRunner runner;

    private async void Start()
    {
        runner = GetComponent<NetworkRunner>();
            
        runner.ProvideInput = true;
        
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host, // For LAN play we'll start with Host mode
            SessionName = "LAN_Mario",
            Scene       = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-5f, 5f), 1, 0);
        runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
    }
}

