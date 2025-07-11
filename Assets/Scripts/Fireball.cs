using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject), typeof(Rigidbody2D))]
public class Fireball : NetworkBehaviour
{
    [SerializeField] private float speed     = 8f;
    [SerializeField] private float lifeTime  = 3f;
    [SerializeField] private LayerMask bounceMask;   // ground / pipes etc. (fireball will bounce off whatever item is checked in this)

    private Rigidbody2D rb;

    // Who fired the ball? (server-written -> everyone reads)
    private NetworkVariable<ulong> ownerClientId =
        new NetworkVariable<ulong>(readPerm: NetworkVariableReadPermission.Everyone);

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    // Called immediately after Instantiate and BEFORE Spawn()
    public void Init(bool facingRight, ulong shooterId)
    {
        ownerClientId.Value = shooterId;
        rb.linearVelocity = new Vector2((facingRight ? 1 : -1) * speed, 0f);
        transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
    }

    // Server schedules despawn
    public override void OnNetworkSpawn()
    {
        if (IsServer) Invoke(nameof(Despawn), lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsServer) return;                         // server-only authority

        GameObject other = col.gameObject;

        /* 1 ─ Hit another PLAYER ─────────────────────────────────── */
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            if (player && player.OwnerClientId != ownerClientId.Value)   // ignore self-hits
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
