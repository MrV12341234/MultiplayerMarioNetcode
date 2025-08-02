using TMPro;
using UnityEngine;

/// <summary>
/// Handles the floating label; no networking logic here – the Player feeds us.
/// </summary>
public class NameTagUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private float verticalOffset = 1.5f; // Height above player
    private Vector3 worldPositionOffset;
    
    private Vector3 _originalScale;
    
    public void Refresh(string newText)
    {
        nameText.text = newText;
    }
    private void Start()
    {
        // Store initial offset in world space
        worldPositionOffset = transform.position - transform.parent.position;
    }

    private void LateUpdate()
    {
        // Keep the tag facing the camera
        var cam = Camera.main;
        if (!cam) return;

        // Maintain world-space position regardless of parent rotation
        transform.position = transform.parent.position + worldPositionOffset;

        transform.rotation = Quaternion.Euler(
            cam.transform.eulerAngles.x,
            cam.transform.eulerAngles.y,
            0 // Lock Z rotation
        );
    }
}