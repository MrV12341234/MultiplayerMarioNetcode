using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

// To use this bar / script you must put the empty prefab called orangebarspawn (the white circle) where you want the bar to spawn. 

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(Rigidbody2D))]
public class OrangeBar : NetworkBehaviour
{
    public enum Direction { Up, Down }

    /* These are assigned by the spawn-point via Init(); they aren't exposed
       in the prefab’s Inspector, so designers can’t tweak them there. */
    Direction moveDir;
    float     speed;
    float     lifeTime;

    Rigidbody2D rb;
    float       timer;
    bool        isDespawning;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    /// <summary>Called immediately after the prefab is instantiated on the server.</summary>
    public void Init(Direction dir, float spd, float life)
    {
        moveDir  = dir;
        speed    = spd;
        lifeTime = life;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) timer = 0f;            // only the server tracks lifetime
    }

    void FixedUpdate()
    {
        if (!IsServer || isDespawning) return;               // server-authoritative movement

        Vector2 delta = (moveDir == Direction.Up ? Vector2.up : Vector2.down)
                        * speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + delta);

        if ((timer += Time.fixedDeltaTime) >= lifeTime)
        {
            isDespawning = true;
            /*  ──►  destroy = true  ◄──  */
            NetworkObject.Despawn(destroy: true);
        }
    }
}