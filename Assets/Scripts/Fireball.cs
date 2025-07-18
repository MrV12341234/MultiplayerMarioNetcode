using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;

public class Fireball : NetworkBehaviour
{
    [SerializeField] private float  speed    = 8f;
    [SerializeField] private float  lifeTime = 3f;
    [SerializeField] private LayerMask bounceMask;

    private Rigidbody2D      rb;
    private NetworkTransform netTrans;
    
    private NetworkObject networkObject;
    private bool hasBeenVisible;
    private Renderer rend;
    private bool isInitialized;
    private bool selfKill; 

    // Synced direction (true = right, false = left)
    private readonly NetworkVariable<bool> netFacingRight =
        new NetworkVariable<bool>(default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    // Which client fired the shot (server-side info only)
    private ulong ownerId;

    // ---------- INITIALISATION ----------
    private void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        netTrans = GetComponent<NetworkTransform>();
        networkObject = GetComponent<NetworkObject>();
        rend = GetComponent<Renderer>();
        
    }
    
    private void Start()
    {
        // Initialize self-kill timers
        Invoke(nameof(Despawn), lifeTime); // Use existing Despawn method
        selfKill = true;
        Invoke(nameof(SelfKillOver), 0.1f); // Need to create this method
    
        // Handle physics for remote objects
        if (!IsOwner)
        {
            rb.simulated = false;
            GetComponent<Collider2D>().enabled = false;
        }
    
        isInitialized = true;
    }
    
    private void Update()
    {
        // Only process for remote fireballs not yet visible
        if (!IsOwner && !hasBeenVisible)
        {
            // Check against all cameras (works in multiplayer)
            if (rend.isVisible)
            {
                EnablePhysics();
            }
        }
    }
    
    private void SelfKillOver()
    {
        selfKill = false;
    }
    
    private bool IsVisibleFromCamera()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null) return false;
        return renderer.isVisible;
    }

    private void EnablePhysics()
    {
        hasBeenVisible = true;
        rb.simulated = true;
        GetComponent<Collider2D>().enabled = true;
    
        // Critical: Sync with network position
        if (networkObject != null)
        {
            transform.position = networkObject.transform.position;
        }
    }

    /// Called by the server *before* the object is spawned.
    public void Configure(bool facingRight, ulong shooterId)
    {
        ownerId = shooterId;
        netFacingRight.Value = facingRight;               // goes out in spawn message

        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * speed, 0f);

        // Flip visually via scale so we don’t rely on quaternion sync
        transform.localScale = new Vector3(dir, 1f, 1f);
    }

    public override void OnNetworkSpawn()
    {
        bool ownerOrServer = IsServer || IsOwner;

        if (ownerOrServer)
        {
            rb.bodyType  = RigidbodyType2D.Dynamic;
            rb.simulated = true;

            // Owner-client (but not server) applies velocity again for safety
            if (!IsServer && IsOwner)
            {
                ApplyVelocityFromNetVar();
            }

            if (IsServer)
                Invoke(nameof(Despawn), lifeTime);
        }
        else
        {
            rb.bodyType                 = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.simulated                = true;
            if (netTrans != null) netTrans.enabled = true;
        }
    }

    // Helper so we only calculate this once
    private void ApplyVelocityFromNetVar()
    {
        float dir = netFacingRight.Value ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * speed, 0f);
        transform.localScale = new Vector3(dir, 1f, 1f);
    }

    // ---------- COLLISION & DESPAWN (unchanged) ----------
    private void OnCollisionEnter2D(Collision2D col)
    {
        // initialization check
        if (!isInitialized) return;
    
        // physics state check
        if (!rb.simulated) return;
        
        var other = col.gameObject;

        // 1) Enemy hits – local destroy + server despawn request
        if (other.layer == LayerMask.NameToLayer("Enemy"))
        {
            Destroy(other);
            if (IsServer) Despawn();
            else          DespawnServerRpc();
            return;
        }

        // 2) Player hits – owner detects & tells server
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            if (player == null) return;
            
            Debug.Log($"Fireball collision. Owner: {ownerId}, Target: {player.OwnerClientId}, IsServer: {IsServer}");
            
            // Prevent self-kill
            if (player.OwnerClientId == ownerId) 
            {
                Debug.Log("Prevented self-kill");
                return;
            }

            if (IsServer)
            {
                Debug.Log("Server applying hit directly");
                player.Hit();
                Debug.Log($"Fireball hit host player. Owner: {ownerId}, Target: {player.OwnerClientId}");
                Despawn(); // despawn fireball shot by host player
            }
            else
            {
                Debug.Log($"Client requesting hit on player {player.OwnerClientId}");
                HitPlayerServerRpc(player.OwnerClientId);
                DespawnServerRpc(); // despawn fireball shot by client player
                // player.Hit();
                Debug.Log($"Fireball hit player. Owner: {ownerId}, Target: {player.OwnerClientId}");
            }
            return;
        }

        // 3) Bounce surfaces – handled only on server
        if (((1 << other.layer) & bounceMask) != 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 6f);
            return;
        }

        // 4) Everything else
        if (IsOwner)  DespawnServerRpc();
        else if (IsServer) Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HitPlayerServerRpc(ulong targetClientId)
    {
        Debug.Log($"Server received hit request for player {targetClientId}");
    
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out var client))
        {
            var player = client.PlayerObject.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log($"Server applying hit to player {targetClientId}");
                player.Hit();
            }
            else
            {
                Debug.LogError($"Player object not found for client {targetClientId}");
            }
        }
        else
        {
            Debug.LogError($"Client not found: {targetClientId}");
        }
    }

    private void Despawn()
    {
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
}
