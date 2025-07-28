using UnityEngine;
using Unity.Netcode;

public class DeathBarrier : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            player.Death();
            // other.gameObject.SetActive(false); // disable all scripts attached to player
            // GameManager.Instance.ResetLevel(3f);
        }
        else
        {
            Destroy(other.gameObject); // any other game object gets destroyed if it touches the barrier
        }
    }

    
}
