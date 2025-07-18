using System.Collections;          // IEnumerator / Coroutine
using UnityEngine;
using Unity.Netcode;               // For NetworkBehaviour.IsOwner

/// <summary>
/// Keeps the camera locked to the local player in a side-scrolling view.
/// Works whether the player is already in the scene or spawns later via NGO.
/// </summary>
public class SideScrolling : NetworkBehaviour
{
    [Tooltip("Transform of the local player, filled automatically at runtime.")]
    public Transform player;

    [Header("Vertical camera positions")]
    public float height = 7f;          // Y value when above ground
    public float undergroundHeight = -9f;  // Y value when underground

    private void Awake()
    {
        // Start looking for a player object owned by THIS client.
        StartCoroutine(FindLocalPlayer());
    }

    private IEnumerator FindLocalPlayer()
    {
        // In case Awake fires before Netcode has even started.
        while (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            yield return null;

        // Keep searching each frame until we find the local player.
        while (player == null)
        {
            foreach (var pm in FindObjectsOfType<PlayerMovement>())
            {
                if (pm.IsOwner)          // Only true on the object this client controls
                {
                    player = pm.transform;
                    break;
                }
            }

            // Fallback: grab the first object tagged "Player" if ownership isn’t critical
            if (player == null)
            {
                var obj = GameObject.FindWithTag("Player");
                if (obj) player = obj.transform;
            }

            yield return null;          // Wait one frame, try again
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;     // Still waiting for the player to spawn

        Vector3 cameraPosition = transform.position;

        // Lock X so the camera never scrolls backwards. If you want player to move right and left, use this: cameraPosition.x = player.position.x;
        cameraPosition.x = player.position.x;
        // cameraPosition.x = Mathf.Max(cameraPosition.x, player.position.x);

        transform.position = cameraPosition;
        
        
    }

    /// <summary>Call this when Mario enters/exits an underground section.</summary>
    public void SetUnderground(bool underground)
    {
        Vector3 cameraPosition = transform.position;
        cameraPosition.y = underground ? undergroundHeight : height;
        transform.position = cameraPosition;
    }
}
