using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Player))]
public class PlayerFire : NetworkBehaviour
{
    [Header("Fire-Mario settings")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform  spawnPoint;
    [SerializeField] private float      fireRate = 0.33f;   // ≈ 3 per second

    private Player player;
    private float  lastShot;

    private void Awake() => player = GetComponent<Player>();

    private void Update()
    {
        if (!IsOwner || !player.fire) return;          // only local Fire-Mario can shoot

        if (Input.GetButtonDown("Fire1") && Time.time - lastShot >= fireRate)
        {
            lastShot = Time.time;
            bool facingRight = transform.eulerAngles.y < 1f;
            ShootServerRpc(spawnPoint.position, facingRight);
        }
    }

    // Server spawns networked projectile
    [ServerRpc]
    private void ShootServerRpc(Vector2 pos, bool facingRight, ServerRpcParams rpc = default)
    {
        var obj = Instantiate(fireballPrefab, pos, Quaternion.identity);
        var netObj = obj.GetComponent<NetworkObject>();
        netObj.Spawn();
        obj.GetComponent<Fireball>().Init(facingRight, rpc.Receive.SenderClientId);
            

    }
}