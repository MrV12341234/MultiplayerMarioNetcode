using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public PlayerSpriteRenderer smallRenderer;
    public PlayerSpriteRenderer bigRenderer;
    private PlayerSpriteRenderer activeRenderer;
    public PlayerSpriteRenderer fireRenderer;

    public DeathAnimation deathAnimation;
    private CapsuleCollider2D capsuleCollider;
    private Rigidbody2D rb;  

    public bool big => bigRenderer.enabled;
    public bool small => smallRenderer.enabled;
    public bool fire => fireRenderer.enabled;
    public bool dead => deathAnimation.enabled;
    public bool starpower { get; private set; }
    public bool firepower { get; private set; }

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
            if (big || firepower)
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
        fireRenderer.enabled = false;
        firepower = false;
        deathAnimation.enabled = true;
        
        // add trivia activation here
        if (IsOwner)
        {
            GameManager.Instance.ShowQuiz();
           
            deathAnimation.enabled = false;
        }
        
        /* if (IsOwner)
        {
            Debug.Log("inside IsOwner before Reset Level");
           GameManager.Instance.ResetLevel(2f);
           Debug.Log("after ResetLevel");
           deathAnimation.enabled = false;
        } */
        
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
        fireRenderer.enabled = false;
        firepower = false;
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

    public void Firepower()
    {
        firepower = true;
        //  coding to make player shoot fire, Called in PowerUp.cs
        // you must already be big to use a flower, so ensure collider is tall. Shouldnt be problem because flower only spawns if mario is big
        if (!big) Grow();                          // small → big first

        smallRenderer.enabled = false;
        bigRenderer.enabled   = false;
        fireRenderer.enabled  = true;
        activeRenderer        = fireRenderer;

        capsuleCollider.size   = new Vector2(1f, 2f);
        capsuleCollider.offset = new Vector2(0f, 0.5f);
        
    }
    
    [ClientRpc]
    public void TeleportOwnerClientRpc(Vector3 pos, ClientRpcParams rpc = default)
    {
        if (!IsOwner) return;                    // runs only on the targeted client
        transform.SetPositionAndRotation(pos, Quaternion.identity);
    }


    
}
