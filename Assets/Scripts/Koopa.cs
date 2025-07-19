using UnityEngine;

public class Koopa : MonoBehaviour
{
    public Sprite shellSprite;
    public float shellSpeed = 12f;

    private bool shelled;
    private bool pushed;
    private MonoBehaviour movementScript; // Reference to either Koopa Movement or RedKoopaMovment component

    private void Awake()
    {
        // Try to find either movement component
        movementScript = GetComponent<EntityMovement>();
        if (movementScript == null)
        {
            movementScript = GetComponent<RedKoopaMovement>();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!shelled && collision.gameObject.CompareTag("Player") && collision.gameObject.TryGetComponent(out Player player))
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
                player.Hit(); // player hits enemy and either shrinks or dies
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (shelled && other.CompareTag("Player"))
        {
            if (!pushed)
            {
                Vector2 direction = new Vector2(transform.position.x - other.transform.position.x, 0f);
                PushShell(direction);
            }
            else
            {
                Player player = other.GetComponent<Player>();
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
    
    private void EnterShell()
    {
        shelled = true;
        
        // Disable whichever movement script is active
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }
       
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = shellSprite;
    }

    private void PushShell(Vector2 direction)
    {
        pushed = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // For red koopa, we'll use EntityMovement for shell movement
        EntityMovement shellMovement = GetComponent<EntityMovement>();
        if (shellMovement == null)
        {
            shellMovement = gameObject.AddComponent<EntityMovement>();
        }
        
        shellMovement.direction = direction.normalized;
        shellMovement.speed = shellSpeed;
        shellMovement.enabled = true;

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