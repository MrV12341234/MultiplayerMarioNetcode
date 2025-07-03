using UnityEngine;

public class Goomba : MonoBehaviour
{
    public Sprite flatSprite;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // check what Goomba collided with
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();

            if (player.starpower) // if the player in star power call Hit(); killing goomba
            {
                Hit();
            }
            
            // dot task to ensure kill only when mario hits goomba going down
           else if (collision.transform.DotTest(transform, Vector2.down))
            {
                Flatten();
            }
            else
            {
                player.Hit(); // player hits enemy and either shrinks or dies(inside the Hit() function in Player.cs) 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Shell"))
        {
            Hit();
        }
    }

    private void Flatten() // function when enemy is killed
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<EntityMovement>().enabled = false;
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = flatSprite;
        Destroy(gameObject, 0.5f); // wait 5 seconds before destroying so player has .5 seconds to see flattened goomba
    }

    private void Hit()
    {
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<DeathAnimation>().enabled = true;
        Destroy(gameObject, 3f);
    }
}
