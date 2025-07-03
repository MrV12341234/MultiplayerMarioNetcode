using UnityEngine;
using System.Collections;

public class BlockCoin : MonoBehaviour
{
    public void Start()
    {
        GameManager.Instance.AddCoin();

        StartCoroutine(Animate());

    }
    private IEnumerator Animate()
    {

        Vector3 restingPosition = transform.localPosition;
        Vector3 animatedPosition = restingPosition + Vector3.up * 2f;

        yield return Move(restingPosition, animatedPosition);
        yield return Move(animatedPosition, restingPosition);
        
        Destroy(gameObject);
    }

    private IEnumerator Move(Vector3 from, Vector3 to)
    {
        float elapsed = 0f; // keeps track of how much time has elapsed
        float duration = 0.25f; // how long the animation is

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
