using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public PlayerSpriteRenderer smallRenderer;
    public PlayerSpriteRenderer bigRenderer;
    private PlayerSpriteRenderer activeRenderer;
    
    private DeathAnimation deathAnimation;
    private CapsuleCollider2D capsuleCollider;

    public bool big => bigRenderer.enabled;
    public bool small => smallRenderer.enabled;
    public bool dead => deathAnimation.enabled;
    public bool starpower { get; private set; }

    public void Awake()
    {
        deathAnimation = GetComponent<DeathAnimation>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
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
        // add trivia activation here
        
        GameManager.Instance.ResetLevel(3.2f);
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
}
