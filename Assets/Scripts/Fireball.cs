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
        if (IsServer)
        {
            // host (server) simulates real physics
            rb.bodyType   = RigidbodyType2D.Dynamic;
            rb.simulated  = true;
            Invoke(nameof(Despawn), lifeTime);
        }
        else
        {
            // clients replay via NetworkTransform + still detect local enemies
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

        // 2) SERVER‐ONLY from here on out
        if (!IsServer) 
            return;

        // 3) PLAYER HITS: server authoritatively shrinks/kills any non‐self client
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            if (player && player.OwnerClientId != ownerId)
            {
                player.Hit();    // fires your existing Shrink/Death + RPCs
                Despawn();       // then remove the projectile
            }
            return;
        }

        // 4) BOUNCE SURFACES: only on server
         if (((1 << other.layer) & bounceMask) != 0)
        {
            Debug.Log("before Bounced! vel=" + rb.linearVelocity);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 6f);
            Debug.Log("after Bounced! vel=" + rb.linearVelocity);
            return;
        } 

        // 5) ANYTHING ELSE: server despawns the projectile
        Despawn();
    }

    private void Despawn()
    {
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
    
    // below function required so client can request despawn of fireball after it hits enemy
    [ServerRpc(RequireOwnership = false)]
    private void DespawnServerRpc(ServerRpcParams rpcParams = default)
    {
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
}
