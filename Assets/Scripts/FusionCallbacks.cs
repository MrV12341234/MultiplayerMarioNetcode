using System;
using Fusion;
using UnityEngine;
using Fusion.Sockets;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class FusionCallbacks : MonoBehaviour, INetworkRunnerCallbacks
{
    public GameObject playerPrefab;

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 spawnPos = new Vector3(Random.Range(-5f, 5f), 2f, 0f);
            runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
        }
    }

    // Required empty methods:
    public void OnPlayerLeft(NetworkRunner r, PlayerRef p) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner r, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner r, PlayerRef p, NetworkInput input) { }
    public void OnShutdown(NetworkRunner r, ShutdownReason reason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner r) { }
    public void OnDisconnectedFromServer(NetworkRunner r) { }
    public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest req, byte[] tok) => req.Accept();
    public void OnConnectFailed(NetworkRunner r, NetAddress addr, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr msg) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> list) { }
    public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner r, HostMigrationToken token) { }
    public void OnReliableDataReceived(NetworkRunner r, PlayerRef p, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner r) { }
    public void OnSceneLoadStart(NetworkRunner r) { }
}

