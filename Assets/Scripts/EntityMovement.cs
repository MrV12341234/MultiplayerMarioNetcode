using System;
using UnityEngine;

public class EntityMovement : MonoBehaviour
{
    public float speed = 1f;
    public float gravity = -.25f;
    public Vector2 direction = Vector2.left;
    
    private new Rigidbody2D rigidbody;
    private Vector2 velocity;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        enabled = false;
    }
    
    // dectects when something becomes visible
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
        rigidbody.WakeUp();
    }
    
    private void OnDisable()
    {
        rigidbody.linearVelocity = Vector2.zero;
        rigidbody.Sleep();
    }

    private void FixedUpdate() // fixed update is called at a set intravel, not linked to the users FPS. Use FixedUpdate for physics so its the same on all fps
    {
        velocity.x = direction.x * speed;
        velocity.y += gravity * speed; // calling Physics2D is a built-in gravity function in Unity. goto Project Settings, Physics2D to change gravity on enimies, or define variable gravity at top
        
        rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);
        
        //flip direction when enemy hits something
        if (rigidbody.Raycast(direction))
        {
            direction = -direction;
        }

        
        if (rigidbody.Raycast(Vector2.down))
        {
            velocity.y = Mathf.Max(velocity.y, 0f);
        }
    }
}
