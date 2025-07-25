using UnityEngine;
using Unity.Netcode;

public class Flame : NetworkBehaviour
{
    public float speed = 8f;
    public float lifeTime = 1.5f;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
        
        {
            // Move left by default (Bowser faces left)
            rb.linearVelocity = transform.right * -speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        
        if (player)
        {
            player.Hit();
        }
        
    }
    
}