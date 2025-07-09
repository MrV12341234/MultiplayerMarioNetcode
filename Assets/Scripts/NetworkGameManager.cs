using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

/// <summary>
/// Keeps track of lives and orchestrates server-side respawns.
/// Attach this to a (networked) GameObject that exists in every scene.
/// </summary>
public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    [Header("Level settings")]
    [SerializeField] private Transform spawnPoint;       // drag your level’s start pipe/ground here
    [SerializeField] private float respawnDelay = 3f;

    private NetworkVariable<int> lives = new NetworkVariable<int>(3);   // synced automatically

    private void Awake()
    {
        Instance = this;               // singleton for convenience
        DontDestroyOnLoad(gameObject); // survive scene loads if I decide to use them later
    }

    /* ----------------  Public API  ---------------- */

    // The caller doesn’t pass its ID – Netcode gives it to us in rpcParams.
    [ServerRpc(RequireOwnership = false)]
    public void NotifyDeathServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        ulong senderId = rpcParams.Receive.SenderClientId;
        StartCoroutine(RespawnRoutine(senderId));
    }

    /* ----------------  Server-only helpers  ---------------- */

    private IEnumerator RespawnRoutine(ulong clientId)
    {
        lives.Value--;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var conn))
            yield break;

        var player = conn.PlayerObject.GetComponent<Player>();
        if (player == null) yield break;

        // 1) Hide / disable player on **all** clients
        player.DisablePlayerClientRpc();

        // 2) Optional delay
        yield return new WaitForSeconds(respawnDelay);

        // 3) Tell **only the owner** to snap to the spawn point
        var target = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } }
        };
        player.TeleportOwnerClientRpc(spawnPoint.position, target);

        // 4) Re-enable behaviour on everyone
        player.ResetStateClientRpc();
    }
}