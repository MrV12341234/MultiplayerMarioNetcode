using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Spawns OrangeBar prefabs at a fixed interval, starting the first time
/// any player’s camera sees this spawn-point.
/// </summary>
public class OrangeBarSpawnPoint : NetworkBehaviour
{
    [Header("Prefab reference")]
    [SerializeField] NetworkObject orangeBarPrefab;

    [Header("Spawning")]
    [Tooltip("Seconds between spawns while the spawn-point is visible")]
    [SerializeField] float spawnInterval = 6f;

    [Header("Bar settings")]
    [SerializeField] OrangeBar.Direction direction = OrangeBar.Direction.Up;
    [SerializeField] float speed     = 1f;   // units per second
    [SerializeField] float lifeTime  = 6f;   // seconds before each bar despawns

    bool  active;        // true once visibility is triggered
    float timer;         // counts up to spawnInterval

    /* ---------- Visibility callbacks ---------- */

    // Fired on *every* client whose camera renders the attached renderer
    void OnBecameVisible()
    {
        if (IsServer)
            Activate();
        else
            ActivateServerRpc();             // clients ask host to start spawning
    }

    // Optional: pause spawning when no camera can see the point
    void OnBecameInvisible()
    {
        if (IsServer) active = false;
    }

    [ServerRpc(RequireOwnership = false)]
    void ActivateServerRpc() => Activate();

    void Activate()
    {
        if (active) return;
        active = true;
        timer  = spawnInterval;              // immediate first spawn
    }

    /* ---------- Server-side spawn loop ---------- */

    void Update()
    {
        if (!IsServer || !active) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;

        timer = 0f;
        SpawnBar();
    }

    void SpawnBar()
    {
        var obj = Instantiate(orangeBarPrefab, transform.position, Quaternion.identity);

        // Configure the bar *before* the network spawn
        obj.GetComponent<OrangeBar>().Init(direction, speed, lifeTime);

        obj.Spawn();                         // replicated to everyone
    }
}
