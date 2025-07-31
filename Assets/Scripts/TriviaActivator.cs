using System;
using UnityEngine;
using Unity.Netcode;

public class TriviaActivator : NetworkBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // this is the Player that hit the power-up
        var pm       = other.GetComponent<PlayerMovement>();
        var netObj   = other.GetComponent<NetworkObject>();
       // var _collider = other.GetComponent<CapsuleCollider2D>();
      //  var rb = other.GetComponent<Rigidbody2D>();
        
        if (pm != null && netObj != null && netObj.IsOwner)
        {
            pm.enabled = false;                       // lock keyboard movements
         //   _collider.enabled = false; 
            GameManager.Instance.ShowQuiz();        // pass the instance
        }
        
    }
}
