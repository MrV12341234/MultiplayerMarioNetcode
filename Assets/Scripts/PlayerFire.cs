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
        var fb  = obj.GetComponent<Fireball>();

        // 2) Configure it *before* spawning so its NetworkVariables
        //    are included in the initial spawn-message
        fb.Configure(facingRight, rpc.Receive.SenderClientId);

        // 3) Now spawn the object and give ownership to the shooter
        obj.GetComponent<NetworkObject>()
            .SpawnWithOwnership(rpc.Receive.SenderClientId);
    }
}