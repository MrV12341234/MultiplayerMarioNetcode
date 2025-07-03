using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    private new Camera camera;
    private new Rigidbody2D rigidbody;
    private new Collider2D collider;

    private Vector2 velocity;
    private float inputAxis;

    public float moveSpeed = 8f;
    public float maxJumpHeight = 5f;
    public float maxJumpTime = 1f;
    public float friction = 3f; // friction added to help mario come to a stop sooner
    public float jumpForce => (2f * maxJumpHeight) / (maxJumpTime / 2f);
    public float gravity => (-2f * maxJumpHeight) / Mathf.Pow((maxJumpTime / 2f), 2);

    public bool grounded { get; private set; }
    public bool jumping { get; private set; }
    public bool running => Mathf.Abs(velocity.x) > 0.25f || Mathf.Abs(inputAxis) > 0.25f;
    // below defines sliding. if your pressing button and your velocity is opposite, you are slide.
    public bool sliding => (inputAxis > 0f && velocity.x < 0f) || (inputAxis < 0f && velocity.x > 0f);

private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        camera = Camera.main;
    }

    private void OnEnable()
    {
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        collider.enabled = true;
        velocity = Vector2.zero;
        jumping = false;
    }

private void OnDisable()
{
    rigidbody.bodyType = RigidbodyType2D.Kinematic;
    collider.enabled = false;
    velocity = Vector2.zero;
    jumping = false;
}


    private void Update()
    {
        if (!IsOwner) return; // keeps movement inputs to the game, not the other players in the room
        
        HorizontalMovement();
        // checks if you're grounded
        grounded = rigidbody.Raycast(Vector2.down);
        if (grounded)
        {
            GroundedMovement();
        }
        ApplyGravity();
    }

    
    private void HorizontalMovement()
    {
        if (!IsOwner) return;
        
        inputAxis = Input.GetAxis("Horizontal");
        velocity.x = Mathf.MoveTowards(velocity.x, inputAxis * moveSpeed, moveSpeed * friction * Time.deltaTime); // player movement. friction added to stop the long transistion time between left and right

        if (rigidbody.Raycast(Vector2.right * velocity.x))
        {
            velocity.x = 0f;
        }
        
        if (velocity.x > 0f)
        {
            transform.eulerAngles = Vector2.zero;
        } 
        else if (velocity.x < 0f)
        {
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    private void GroundedMovement()
    {
        if (!IsOwner) return;
        
        velocity.y = Mathf.Max(velocity.y, 0f);
        jumping = velocity.y > 0f;
        if (Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
            jumping = true;
        }
    }

    private void ApplyGravity()
    {
        if (!IsOwner) return;
        //below physics allow the game to feel like the real mario where holding jump button gives you less gravity but when you release you start falling.
        bool falling = velocity.y < 0f || !Input.GetButton("Jump");
        float multiplier = falling ? 2f : 1f;
        
        velocity.y += gravity * multiplier * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, gravity / 2f);
    }

    private void FixedUpdate()
    {
        Vector2 position = rigidbody.position;
        position += velocity * Time.fixedDeltaTime;
        
        // calculate screen boundries and stop player from going back
        Vector2 leftEdge = camera.ScreenToWorldPoint(Vector2.zero);
        Vector2 rightEdge = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        position.x = Mathf.Clamp(position.x, leftEdge.x + 0.5f, rightEdge.x -0.5f);

        rigidbody.MovePosition(position);
        
    }
    // OnCollisionEnter2D is a unity function that says 'do this when player has collided with something'
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (transform.DotTest(collision.transform, Vector2.down)) // if player (called transform) hits enemy object going down
            {
                velocity.y = jumpForce / 2f; // apply half your jump force
                jumping = true;
            }
        }
        if (collision.gameObject.layer != LayerMask.NameToLayer("PowerUp"))
        {
            // Stop vertical movement if mario bonks his head
            if (transform.DotTest(collision.transform, Vector2.up)) {
                 velocity.y = 0f;
             }
        }
    }
}
