using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public PlayerSpriteRenderer smallRenderer;
    public PlayerSpriteRenderer bigRenderer;
    private PlayerSpriteRenderer activeRenderer;
    
    private DeathAnimation deathAnimation;
    private CapsuleCollider2D capsuleCollider;
    private Rigidbody2D rb;  

    public bool big => bigRenderer.enabled;
    public bool small => smallRenderer.enabled;
    public bool dead => deathAnimation.enabled;
    public bool starpower { get; private set; }

    public void Awake()
    {
        deathAnimation = GetComponent<DeathAnimation>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        
        activeRenderer = smallRenderer;
    }
    
    public void Hit() // if mario was hit by something
    {
        if (!dead && !starpower)
        {
            if (big)
            {
                Shrink();
            }
            else
            {
                Death();
            } 
        }
    }
    
    public void Death()
    {
        
        smallRenderer.enabled = false;
        bigRenderer.enabled = false;
        deathAnimation.enabled = true;
        // TODO: add trivia activation here
        Debug.Log("after death animation before Reset Level");

        if (IsOwner)
        {
            Debug.Log("inside IsOwner before Reset Level");
           GameManager.Instance.ResetLevel(2f);
           Debug.Log("after ResetLevel");
           deathAnimation.enabled = false;
        }
        
    }
    
    [ClientRpc]
    public void DisablePlayerClientRpc()
    {
        // runs on **all** clients
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<CapsuleCollider2D>().enabled = false;
        rb.simulated = false;   // stop physics
        smallRenderer.enabled = bigRenderer.enabled = false;
    }

    [ClientRpc]
    public void ResetStateClientRpc()
    {
        // enables movement, colliders, and the correct sprite
        GetComponent<PlayerMovement>().enabled = true;
        GetComponent<CapsuleCollider2D>().enabled = true;
        rb.simulated = true;

        smallRenderer.enabled = true;  // whatever your size logic is
        bigRenderer.enabled   =  false;
    }
    
    public void Grow()
    {
        smallRenderer.enabled = false;
        bigRenderer.enabled = true;
        activeRenderer = bigRenderer;
        // change capsule collider size when mario grows
        capsuleCollider.size = new Vector2(1f, 2f);
        capsuleCollider.offset = new Vector2(0f, 0.5f);
        
        StartCoroutine(ScaleAnimation());
    }

    private void Shrink()
    {
        smallRenderer.enabled = true;
        bigRenderer.enabled = false;
        activeRenderer = smallRenderer;
        // change capsule collider size when mario shrinks
        capsuleCollider.size = new Vector2(1f, 1f);
        capsuleCollider.offset = new Vector2(0f, 0f);
        StartCoroutine(ScaleAnimation());
    }

    private IEnumerator ScaleAnimation()
    {
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (Time.frameCount % 4 == 0) // this says every 4 frames we switch
            {
                smallRenderer.enabled = !smallRenderer.enabled;
                bigRenderer.enabled = !smallRenderer.enabled;
            }
            yield return null;
        }
        // disables both sprites and then sets the active
        smallRenderer.enabled = false;
        bigRenderer.enabled = false;
        activeRenderer.enabled = true;
    }

    public void Starpower(float duration = 10f) // setting star power default to 10 sec
    {
        StartCoroutine(StarpowerAnimation(duration));
    }

    private IEnumerator StarpowerAnimation(float duration)
    {
        starpower = true;
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (Time.frameCount % 4 == 0)
            {
                // randomizes colors between hue 0 and hue 1 (all of them) during star power
                activeRenderer.spriteRenderer.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            }
            yield return null;
        }
        activeRenderer.spriteRenderer.color = Color.white;
        starpower = false;
    }
    
    [ClientRpc]
    public void TeleportOwnerClientRpc(Vector3 pos, ClientRpcParams rpc = default)
    {
        if (!IsOwner) return;                    // runs only on the targeted client
        transform.SetPositionAndRotation(pos, Quaternion.identity);
    }


    
}
