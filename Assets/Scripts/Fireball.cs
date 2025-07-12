using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;


public class Fireball : NetworkBehaviour
{
    [SerializeField] private float speed     = 8f;
    [SerializeField] private float lifeTime  = 3f;
    [SerializeField] private LayerMask bounceMask;   // ground / pipes etc. (fireball will bounce off whatever item is checked in this)

    private Rigidbody2D rb;
    
    

    // Who fired the ball? (server-written -> everyone reads)
    private readonly NetworkVariable<ulong> ownerId =
        new(readPerm: NetworkVariableReadPermission.Everyone);

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    // Called immediately after Instantiate and BEFORE Spawn()
    public void Init(bool facingRight, ulong shooterId)
    {
        ownerId.Value = shooterId;
        rb.linearVelocity = new Vector2((facingRight ? 1 : -1) * speed, 0f);
        transform.rotation = facingRight ? Quaternion.identity
            : Quaternion.Euler(0, 180, 0);
    }

    // Server schedules despawn
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Server actually simulates physics.
            rb.isKinematic = false;
            rb.simulated   = true;
            Invoke(nameof(Despawn), lifeTime);
        }
        else
        {
            // Clients follow the NetworkTransform; no local physics.
            rb.isKinematic = true;
            rb.simulated   = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsServer) return;                         // server-only authority

        GameObject other = col.gameObject;

        /* 1 ─ Hit another PLAYER ─────────────────────────────────── */
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            if (player && player.OwnerClientId != ownerId.Value)   // ignore self-hits
            {
                player.Hit();        
                Despawn();
                return;
            }
        }

        /* 2 ─ Hit an ENEMY ───────────────────────────────────────── */
        if (other.layer == LayerMask.NameToLayer("Enemy"))
        {
            Destroy(other);  // or enemy.Kill();
            Despawn();
            return;
        }

        /* 3 ─ Bounce off ground / walls / pipes ‐ optional ──────── */
        if (((1 << other.layer) & bounceMask) != 0)
        {
            // Simple vertical flip for “skipping” fireball behaviour
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 6f);
            return;
        }

        /* 4 ─ Everything else – just despawn */
         
    }

    private void Despawn() => NetworkObject.Despawn();
}
