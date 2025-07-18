using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Player))]
public class PlayerFire : NetworkBehaviour
{
    [Header("Fire-Mario settings")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform  spawnPoint;
    [SerializeField] private float      fireRate = 0.33f; // 3 per sec

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
        // 1) Instantiate the prefab
        var obj = Instantiate(fireballPrefab, pos, Quaternion.identity);
        var fb = obj.GetComponent<Fireball>();
    
        Vector3 direction = facingRight ? Vector3.right : Vector3.left;
        Vector3 bufferOffset = direction * 0.5f;
        obj.transform.position += bufferOffset;

        // 2) Configure it with the shooter's ID
        ulong shooterId = rpc.Receive.SenderClientId;
        fb.Configure(facingRight, shooterId);

        // 3) Now spawn the object and give ownership to the shooter
        var netObj = obj.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(shooterId);
    
        Debug.Log($"Fireball spawned. Owner: {shooterId}");
    }
}