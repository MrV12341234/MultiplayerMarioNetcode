using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class BlockHit : MonoBehaviour
{
    public GameObject smallItem; // item spawned when mario is little
    public GameObject bigItem; // item that is spawned if mario hits bottom and is big status
    
    public Sprite emptyBlock; // leave empty in inspector if you want block to become invisible after breaking
    public int maxHits = -1; // for how many times a block is able to be hit (-1 means it can be hit over & over w/out breaking b/c the Hit() function only destroys if 0)
    
    private bool _animating; // when block is hit there is an animation (eg mushroom comes out).We need to track if animation is playing so player cant hit many times at once
    
    //detect when mario collides with a block and spawn 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_animating || maxHits == 0) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        // check Mario is hitting the bottom of the block. Also checks if mario is big or small
        if (collision.transform.DotTest(transform, Vector2.up))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            GameObject prefab = (player != null && (player.big || player.fire)) ? bigItem : smallItem; // checks if player is big or firepower, then spawns big item. otherwise small item

            Hit(prefab);
        }
    }
    private void Hit(GameObject itemToSpawn)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true; // revels block if its hidden
        maxHits--;
        
        if (maxHits == 0)
        {
            spriteRenderer.sprite = emptyBlock;
        }
        

        if (itemToSpawn != null)
        {
            Instantiate(itemToSpawn, transform.position, Quaternion.identity); //spawns an item if object is attached in inspector
        }

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        _animating = true;

        Vector3 restingPosition = transform.localPosition;
        Vector3 animatedPosition = restingPosition + Vector3.up * 0.5f;

        yield return Move(restingPosition, animatedPosition);
        yield return Move(animatedPosition, restingPosition);

        _animating = false;
    }

    private IEnumerator Move(Vector3 from, Vector3 to)
    {
        float elapsed = 0f; // keeps track of how much time has elapsed
        float duration = 0.125f; // how long the animation is

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            transform.localPosition = Vector3.Lerp(from, to, t);
            elapsed += Time.deltaTime;

            yield return null; //waits until next frame to continue on
        }

        transform.localPosition = to;
    }
}



