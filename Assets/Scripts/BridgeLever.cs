using System;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;

public class BridgeLever : NetworkBehaviour
{

    [SerializeField] private GameObject bridgeInstance;   // bridge that resides next to lever
    public GameObject princessTextBubble;

    /* ---------- TRIGGER ---------- */
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (IsServer)
        {
            OnNetworkDespawn();          // host path.
        }
        else
        {
            DespawnLeverAndBridgeServerRpc(); // client asks host through an RPC
        }
    }

    /* ---------- RPC ---------- */
    [ServerRpc(RequireOwnership = false)]
    private void DespawnLeverAndBridgeServerRpc()
    {
        OnNetworkDespawn();;              // runs on the server
    }

    /* ---------- CORE LOGIC ---------- */
    public override void OnNetworkDespawn() // "OnNetworkDespawn"()" is a netcode method that processes all the lines of code inside on all loaded players
    {
       GetComponent<SpriteRenderer>().enabled = false;   
        GetComponent<Collider2D>().enabled     = false;

        var sr = bridgeInstance.GetComponent<SpriteRenderer>();
        var col = bridgeInstance.GetComponent<Collider2D>();
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;
        
        Destroy(gameObject);
        Destroy(bridgeInstance);
        
        princessTextBubble.SetActive(true);
    }
}