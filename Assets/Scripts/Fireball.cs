using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class Fireball : NetworkBehaviour
{
    [SerializeField] private float  speed    = 8f;
    [SerializeField] private float  lifeTime = 3f;
    [SerializeField] private LayerMask bounceMask;

    private Rigidbody2D      rb;
    private NetworkTransform netTrans;
    

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
        if (other.CompareTag("Player") && !IsOwner)
        {
            var player = other.GetComponent<Player>();

            if (IsServer)
            {
                player.Hit();
                Despawn();
            }
            else
            {
                HitPlayerServerRpc(player.OwnerClientId);
                DespawnServerRpc();
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
        foreach (var netObj in NetworkManager.Singleton.ConnectedClientsList)
            if (netObj.ClientId == targetClientId)
                netObj.PlayerObject.GetComponent<Player>()?.Hit();
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
