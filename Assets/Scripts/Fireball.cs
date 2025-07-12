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
        ownerId = shooterId;
        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * speed, 0f);
        transform.rotation = facingRight
            ? Quaternion.identity
            : Quaternion.Euler(0, 180, 0);
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
            Destroy(other);
            Despawn(); 
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 6f);
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
}
