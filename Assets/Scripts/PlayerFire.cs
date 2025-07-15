using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Player))]
public class PlayerFire : NetworkBehaviour
{
    [Header("Fire-Mario settings")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform  spawnPoint;
    [SerializeField] private float      fireRate = 0.33f; // ≈3 per sec

    private Player player;
    private float  lastShot;

    private void Awake() => player = GetComponent<Player>();

    private void Update()
    {
        if (!IsOwner || !player.fire) return;
        if (Input.GetButtonDown("Fire1") && Time.time - lastShot >= fireRate)
        {
            lastShot = Time.time;
            bool facingRight = transform.eulerAngles.y < 1f;
            ShootServerRpc(spawnPoint.position, facingRight);
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector2 pos, bool facingRight, ServerRpcParams rpc = default)
    {
        Debug.Log($"[Server] ShootServerRpc(): pos={pos}, facingRight={facingRight}");
        // ① Instantiate & Spawn first
        var obj   = Instantiate(fireballPrefab, pos, Quaternion.identity);
        var netOb = obj.GetComponent<NetworkObject>();
        
        netOb.SpawnWithOwnership(rpc.Receive.SenderClientId);

        // ② Now safe to call Init()
        var fb = obj.GetComponent<Fireball>();
        fb.Init(facingRight, rpc.Receive.SenderClientId);
    }
}