using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

// if you want mario's fireball to kill, put the prefab on layer Enemy . Will also need two colliders, one with trigger and one without. Trigger collider
public class UpDownHazard : MonoBehaviour
{
    [Header("Movement Settings")]
    public float distanceUp = 1f;
    public float movementSpeed = 1f;
    public float pauseAtTop = 1f;
    public bool invertSpriteAtTop = false;

    [Header("References")]
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float pauseTimer;
    private bool isMovingUp = true;
    private bool isPaused = false;
    private bool originalFlipState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
            originalFlipState = spriteRenderer;
    }

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition + Vector2.up * distanceUp;
    }

    private void Update()
    {
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0)
            {
                isPaused = false;
                
                // Toggle sprite when pause ends if needed
                if (invertSpriteAtTop && spriteRenderer != null)
                {
                    spriteRenderer.flipY = isMovingUp ? !originalFlipState : originalFlipState;
                }
            }
            return;
        }

        // Calculate movement direction based on state
        Vector2 target = isMovingUp ? targetPosition : startPosition;
        Vector2 newPosition = Vector2.MoveTowards(rb.position, target, movementSpeed * Time.deltaTime);
        rb.MovePosition(newPosition);

        // Check if reached position
        if (Vector2.Distance(rb.position, target) < 0.01f)
        {
            StartPause();
        }
    }

    private void StartPause()
    {
        isPaused = true;
        pauseTimer = pauseAtTop;
        isMovingUp = !isMovingUp;
        
        // Immediately toggle sprite when pausing at top if needed
        if (invertSpriteAtTop && spriteRenderer != null && !isMovingUp)
        {
            spriteRenderer.flipY = !spriteRenderer.flipY;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) // if mario hits the collider set to isTrigger. Make sure the trigger collider is slighltly larger than collision collier
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.Hit();
        }
    }

    // For visual debugging in editor
    private void OnDrawGizmosSelected()
    {
        Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startPos, startPos + Vector3.up * distanceUp);
        Gizmos.DrawWireCube(startPos + Vector3.up * distanceUp, Vector3.one * 0.5f);
    }
}