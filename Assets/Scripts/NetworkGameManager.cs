using System.Collections;
using UnityEngine;
using Unity.Netcode;

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
        DontDestroyOnLoad(gameObject); // survive scene loads if you still decide to use them later
    }

    /* ----------------  Public API  ---------------- */

    /// <summary>Called by a Player when they die (owner-side).</summary>
    [ServerRpc(RequireOwnership = false)]
    public void NotifyDeathServerRpc(ulong clientId)
    {
        if (!IsServer) return;
        StartCoroutine(RespawnRoutine(clientId));
    }

    /* ----------------  Server-only helpers  ---------------- */

    private IEnumerator RespawnRoutine(ulong clientId)
    {
        lives.Value--;

        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerObject == null) yield break;

        // 1) Hide and disable the player for everyone
        playerObject.GetComponent<Player>().DisablePlayerClientRpc();

        // 2) Wait for animation / delay
        yield return new WaitForSeconds(respawnDelay);

        // 3) Reset position & state on the server (owner authoritative transform will replicate)
        playerObject.transform.SetPositionAndRotation(spawnPoint.position, Quaternion.identity);
        playerObject.GetComponent<Player>().ResetStateClientRpc();
    }
}