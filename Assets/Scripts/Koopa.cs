using UnityEngine;

public class Koopa : MonoBehaviour
{
    public Sprite shellSprite;
    public float shellSpeed = 12f;

    private bool shelled;
    private bool pushed;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // check what Koopa collided with
        if (!shelled && collision.gameObject.CompareTag("Player") && collision.gameObject.TryGetComponent(out Player player)) // confirms its the player that collides with koopa
        {
            if (player.starpower)
            {
                Hit();
            }
            // dot task to ensure kill only when mario hits goomba going down
            else if (collision.transform.DotTest(transform, Vector2.down))
            {
                EnterShell(); // Koopa enters stationary shell
            }
            else
            {
                player.Hit(); // player hits enemy and either shrinks or dies(inside the Hit() function in Player.cs) 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (shelled && other.CompareTag("Player")) // confirms its the player that collides with koopa in a shell
        {
            if (!pushed)
            {
                Vector2 direction = new Vector2(transform.position.x - other.transform.position.x, 0f);
                PushShell(direction);
            }
            else
            {
                Player player = other.GetComponent<Player>(); // gets a reference to the player

                if (player.starpower)
                {
                    Hit();
                }
                else
                {
                    player.Hit();
                }
            }
        }
        else if (!shelled && other.gameObject.layer == LayerMask.NameToLayer("Shell"))
        {
            Hit();
        }
    }
    
    private void EnterShell() // function when enemy is killed
    {
        shelled = true;
        
        GetComponent<EntityMovement>().enabled = false;
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = shellSprite;
    }

    private void PushShell(Vector2 direction)
    {
        pushed = true;

        // GetComponent<Rigidbody2D>().isKinematic = false;
        // NEW
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        EntityMovement movement = GetComponent<EntityMovement>();
        movement.direction = direction.normalized;
        movement.speed = shellSpeed;
        movement.enabled = true;

        gameObject.layer = LayerMask.NameToLayer("Shell");
        
    }

    private void Hit()
    {
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<DeathAnimation>().enabled = true;
        Destroy(gameObject, 3f);
    }
    
    private void OnBecameInvisible()
    {
        if (pushed)
        {
            Destroy(gameObject);
        }
    }
}
