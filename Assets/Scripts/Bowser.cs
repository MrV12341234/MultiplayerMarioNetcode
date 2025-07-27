using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(NetworkObject))]
public class Bowser : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float jumpHeight = 8f;
    public float horizontalDistance = 1.5f;
    public float gravityScale = 3f;
    public float jumpInterval = 2f;

    [Header("Combat Settings")]
    public int hitsToKill = 5;
    public float timeBetweenFlames = 3f;

    [Header("References")]
    public Transform flameSpawnPoint;
    public GameObject flamePrefab;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool movingForward = true;
    private NetworkVariable<int> hitCount = new NetworkVariable<int>(0);
    private float groundCheckRadius = 0.2f;

    private void Awake()
    {
        
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        
        // if (IsServer)
        {
            StartCoroutine(JumpRoutine());
            StartCoroutine(FlameRoutine());
        }
    }

    private void Update()
    {
        // if (!IsServer) return;
        
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private IEnumerator JumpRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => isGrounded);
            yield return new WaitForSeconds(0.1f); // Small delay before jump
            
            
            // Calculate jump direction
            float direction = movingForward ? 1f : -1f;
            movingForward = !movingForward; // Toggle direction for next jump
            
            // Calculate jump velocity
            float jumpForce = Mathf.Sqrt(jumpHeight * -2 * (Physics2D.gravity.y * rb.gravityScale));
            rb.linearVelocity = new Vector2(direction * horizontalDistance, jumpForce);
            
            yield return new WaitForSeconds(jumpInterval);
        }
    }
    

    private IEnumerator FlameRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenFlames);
            SpawnFlameClientRpc();
        }
    }

    [ClientRpc]
    private void SpawnFlameClientRpc()
    {
        if (flamePrefab && flameSpawnPoint)
        {
            Instantiate(flamePrefab, flameSpawnPoint.position, flameSpawnPoint.rotation);
        }
    }
    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
        {
            player.Hit();  // server deals damage once
            return;
        }

        if (collision.gameObject.TryGetComponent(out Fireball fireball))
        {
            if (IsServer)
            {
                RegisterHit();                    // host increment
                fireball.NetworkObject.Despawn();
            }
            else
            {
                RegisterHitServerRpc();           // clients request increment
                fireball.DespawnServerRpc();
            }
        }
    }

    

    private void RegisterHit()
    {
        hitCount.Value++;
        
        if (hitCount.Value >= hitsToKill)
        {
            // Destroy on all clients
            OnNetworkDespawn();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RegisterHitServerRpc()
    {
        RegisterHit();     // runs on the server, so it can write hitCount
    }
    
    public override void OnNetworkDespawn()
    {
        // Stop behaviour
        StopAllCoroutines();

        // Hide visuals & disable collisions on *all* peers
        GetComponent<SpriteRenderer>().enabled = false;   // or MeshRenderer
        GetComponent<Collider2D>().enabled     = false;
        rb.simulated = false;
    
        // If you truly never need Bowser again, destroy the GameObject:
        Destroy(gameObject);   // safe: runs on every client locally
    }

    // Visualize ground check radius in editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}