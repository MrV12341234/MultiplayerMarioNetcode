using UnityEngine;

public class RedKoopaMovement : MonoBehaviour
{
    public float speed = 1f;
    public float gravity = -0.25f;
    public Vector2 direction = Vector2.left;
    
    [Header("Edge Detection")]
    [SerializeField] private float edgeCheckDistance = 0.5f;
    // [SerializeField] private float edgeCheckOffset = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundedRayDistance = 0.1f; // Specific distance for grounded check
    
    private Rigidbody2D rb;
    private Vector2 velocity;
    private BoxCollider2D _boxCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        enabled = false;
    }
    
    private void OnBecameVisible()
    {
        enabled = true;
    }

    private void OnBecameInvisible()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        rb.WakeUp();
    }
    
    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
        rb.Sleep();
    }

    private void FixedUpdate()
    {
        // Calculate movement
        velocity.x = direction.x * speed;
        velocity.y += gravity;
        
        // Apply movement
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        
        // Wall collision detection
        if (Raycast(direction, 0.6f))
        {
            direction = -direction;
        }

        // Grounded check with improved positioning
        bool isGrounded = Raycast(Vector2.down, groundedRayDistance);

        if (isGrounded)
        {
            velocity.y = Mathf.Max(velocity.y, 0f);
            CheckForDropOff();
        }
    }

    private void CheckForDropOff()
    {
        // Calculate position at the bottom front of koopa
        Vector2 rayOrigin = GetFrontBottomPosition();
        
        // Perform raycast to detect ground ahead
        bool isGroundAhead = Physics2D.Raycast(
            rayOrigin, 
            Vector2.down, 
            edgeCheckDistance, 
            groundLayer
        );

        // Debug visualization
        Debug.DrawRay(
            rayOrigin, 
            Vector2.down * edgeCheckDistance, 
            isGroundAhead ? Color.green : Color.red,
            0.1f
        );

        // Turn around only if there's no ground ahead
        if (!isGroundAhead)
        {
            direction = -direction;
        }
    }

    // Improved raycast method with better positioning
    private bool Raycast(Vector2 direction, float distance)
    {
        // Calculate ray origin at bottom center
        Vector2 rayOrigin = GetBottomCenterPosition();
        
        // Perform the raycast
        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin, 
            direction, 
            distance, 
            groundLayer
        );
        
        // Debug visualization
        Debug.DrawRay(
            rayOrigin, 
            direction * distance, 
            hit.collider ? Color.green : Color.red,
            0.1f
        );
        
        return hit.collider != null;
    }

    // Gets position at bottom center of collider
    private Vector2 GetBottomCenterPosition()
    {
        return new Vector2(
            rb.position.x,
            rb.position.y - _boxCollider.bounds.extents.y
        );
    }

    // Gets position at bottom front of collider (for edge detection)
    private Vector2 GetFrontBottomPosition()
    {
        return new Vector2(
            rb.position.x + (direction.x * _boxCollider.bounds.extents.x * 0.9f),
            rb.position.y - _boxCollider.bounds.extents.y
        );
    }
}