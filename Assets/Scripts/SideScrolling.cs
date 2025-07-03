using UnityEngine;

// this script manages the movement of the camera
public class SideScrolling : MonoBehaviour
{
    public Transform player;
    public float height = 7f; // this is the Y value of your Main Camera
    public float undergroundHeight = -9f; // Y value of camera when mario goes underground

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    private void LateUpdate()
    {
        Vector3 cameraPosition = transform.position;
        // below code stops camera from allowing player to go back. If you want player to move right and left, use this: cameraPosition.x = player.position.x;
        cameraPosition.x = Mathf.Max(cameraPosition.x, player.position.x);
        transform.position = cameraPosition;
    }

    public void SetUnderground(bool underground)
    {
        Vector3 cameraPosition = transform.position;
        cameraPosition.y = underground ? undergroundHeight : height;
        transform.position = cameraPosition;
    }
}
