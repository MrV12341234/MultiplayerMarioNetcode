using System;
using UnityEngine;

public class Princess : MonoBehaviour
{
    //todo: 

    // princess is jumping up and down with text box saying 'come save me'



    // after player collides with Princess, game end screen appears.
    // display quit button
    // force game quit after 15 

    [Header("UI References")]
    [SerializeField] private GameObject mainMenuCanvas;  
    [SerializeField] private GameObject endGameCanvas;  
    
    [Header("Princess Motions")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;          // what counts as ground
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField] private Vector2 groundCheckOffset = Vector2.zero;

    private Rigidbody2D rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    private void FixedUpdate()
    {
        if (IsGrounded())
            Jump();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.collider.CompareTag("Player"))
            return;
        
        mainMenuCanvas.SetActive(false);
        endGameCanvas.SetActive(true);
    }

    private void Jump()
    {
        // zero any downward velocity so small bumps don't cancel the jump
        if (rb.linearVelocity.y < 0f) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private bool IsGrounded()
    {
        // cast from a point slightly above the bottom of the collider
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down,
            groundCheckDistance, groundLayer);

        // Debug gizmo (green when grounded, red otherwise)
        Debug.DrawRay(origin, Vector2.down * groundCheckDistance,
            hit ? Color.green : Color.red);

        return hit;

        
    }


}
