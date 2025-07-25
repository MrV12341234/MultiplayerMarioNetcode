using System;
using Unity.VisualScripting;
using UnityEngine;

public class BridgeLever : MonoBehaviour
{

    public GameObject bridgePrefab;
    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        
        if (player)
        {
            Destroy(bridgePrefab);
            Destroy(gameObject);
        }
    }
}
