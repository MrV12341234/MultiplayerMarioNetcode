using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class Fireball : NetworkBehaviour
{
    [SerializeField] private float     speed     = 8f;
    [SerializeField] private float     lifeTime  = 3f;
    [SerializeField] private LayerMask bounceMask;

    private Rigidbody2D   rb;
    private NetworkTransform netTrans;

    // server-only: which client fired this
    private ulong ownerId;

    private void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        netTrans = GetComponent<NetworkTransform>();
    }

    /// <summary>
    /// Called immediately after Spawn().
    /// </summary>
    public void Init(bool facingRight, ulong shooterId)
    {
        Debug.Log($"[Server] Fireball.Init(): dir={(facingRight? 1:-1)} speed={speed}, spawnPos={transform.position}");
        ownerId = shooterId;
        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * speed, 0f);
        transform.rotation = facingRight
            ? Quaternion.identity
            : Quaternion.Euler(0, 180, 0);
        Debug.Log($"Init(): facingRight={facingRight}, setting vel={(facingRight ? speed : -speed)}");

    }

    public override void OnNetworkSpawn()
    {
        bool ownerOrServer = IsServer || IsOwner;
        if (ownerOrServer)
        {
            // server and the shooting client both run real physics
            rb.bodyType   = RigidbodyType2D.Dynamic;
            rb.simulated  = true;
            if (IsServer)
                Invoke(nameof(Despawn), lifeTime);
        }
        else
        {
            // non‐owners only replay via NetworkTransform
            rb.bodyType                = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.simulated               = true;
            if (netTrans != null) netTrans.enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    { 
        var other = col.gameObject;

        // 1) LOCAL‐ONLY ENEMY HITS: run on every instance
        if (other.layer == LayerMask.NameToLayer("Enemy"))
        {
            Destroy(other); // locally destroy enemy
            if (IsServer)
            {
                // if we're the host, destroy fireball upon contact
                Despawn();
            }
            else
            {
                // if we're a client, fire the RPC that requests host to destroy fireball
                DespawnServerRpc();
            }
            return;
        }

        // 2) PLAYER HITS: owner detects and asks server to apply damage
        if (other.CompareTag("Player") && !IsOwner)
        {
            var player = other.GetComponent<Player>();
            
            if (IsServer)
            {
                    // (Optional fallback if server also collides)
                    player.Hit();
                    Despawn(); 
            }
            else
            {
                    // kill other clients (Ask server to shrink/kill that player)
                    HitPlayerServerRpc(player.OwnerClientId);
                    DespawnServerRpc();
            }
            
            return;
        }
        

        // 3) BOUNCE SURFACES: only on server
         if (((1 << other.layer) & bounceMask) != 0)
        {
            Debug.Log("before Bounced! vel=" + rb.linearVelocity);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 6f);
            Debug.Log("after Bounced! vel=" + rb.linearVelocity);
            return;
        } 

        // 4) Anything else — owner asks server to despawn
        if (IsOwner)
            DespawnServerRpc();
        else if (IsServer)
            Despawn();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void HitPlayerServerRpc(ulong targetClientId)
    {
        // Find that player's object and apply damage
        foreach (var netObj in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (netObj.ClientId == targetClientId)
            {
                var player = netObj.PlayerObject.GetComponent<Player>();
                player?.Hit();
                break;
            }
        }
    }

    private void Despawn()
    {
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
    
    // below function required so client can request despawn of fireball after it hits enemy
    [ServerRpc(RequireOwnership = false)]
    private void DespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
}
