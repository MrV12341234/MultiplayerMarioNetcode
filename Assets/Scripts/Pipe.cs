using System.Collections;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public Transform connection; // reference another transform (location) in our world for mario to transport to
    public KeyCode enterKeyCode = KeyCode.S;
    public Vector3 enterDirection = Vector3.down;
    public Vector3 exitDirection = Vector3.zero;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (connection != null && other.CompareTag("Player"))
        {
            if (Input.GetKey(enterKeyCode))
            {
                StartCoroutine(Enter(other.transform)); // other is mario, transform is his position
            }
        }
    }

    private IEnumerator Enter(Transform player)
    {
        player.GetComponent<PlayerMovement>().enabled = false;
        // animate mario down a few units
        Vector3 enteredPosition = transform.position + enterDirection;
        Vector3 enteredScale = Vector3.one * 0.5f; // shrink mario some so he stays inside the pipe

        yield return Move(player, enteredPosition, enteredScale);
        yield return new WaitForSeconds(1f);

        bool underground = connection.position.y < 0f;
        Camera.main.GetComponent<SideScrolling>().SetUnderground(underground);

        if (exitDirection != Vector3.zero)
        {
            // transport player to the gameobjects position attached in inspector
            player.position = connection.position - exitDirection;
            yield return Move(player, connection.position + exitDirection, Vector3.one);
        }
        else
        {
            player.position = connection.position;
            player.localScale = Vector3.one;
        }
        player.GetComponent<PlayerMovement>().enabled = true;
    }
    

    private IEnumerator Move(Transform player, Vector3 endPosition, Vector3 endScale)
    {
        float elapsed = 0f;
        float duration = 1f;
        Vector3 startPosition = player.position;
        Vector3 startScale = player.localScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            player.position = Vector3.Lerp(startPosition, endPosition, t);
            player.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;

            yield return null;
        }
        
        player.position = endPosition;
        player.localScale = endScale;
    }
}
