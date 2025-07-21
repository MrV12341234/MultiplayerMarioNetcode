using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public enum SpinDirection { Clockwise, CounterClockwise }

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
public class SpinningRod : NetworkBehaviour
{
    [Header("Rotation Settings")]
    public float spinSpeed = 180f; // Degrees per second
    public SpinDirection direction = SpinDirection.Clockwise;
    public Transform pivotPoint; // Assign in inspector - should be the base of the rod

    [Header("Collision Settings")]
    public CapsuleCollider2D rodCollider;
    
    private float _currentRotation;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    private void Awake()
    {
        // Set up network components
        GetComponent<NetworkObject>().AutoObjectParentSync = true;
        GetComponent<NetworkTransform>().InLocalSpace = true;
        
        // Save initial state for reference
        if (pivotPoint != null)
        {
            _initialPosition = pivotPoint.position;
            _initialRotation = pivotPoint.rotation;
        }
    }
    

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize rotation state on server
            _currentRotation = transform.rotation.eulerAngles.z;
            UpdateRotationClientRpc(_currentRotation);
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        // Calculate rotation based on direction
        float rotationAmount = spinSpeed * Time.deltaTime;
        if (direction == SpinDirection.CounterClockwise)
        {
            rotationAmount *= -1;
        }

        // Update rotation
        _currentRotation += rotationAmount;
        _currentRotation %= 360f; // Keep within 0-360 range
        
        // Apply rotation
        transform.RotateAround(pivotPoint.position, Vector3.forward, rotationAmount);
        
        // Sync with clients
        UpdateRotationClientRpc(_currentRotation);
    }

    [ClientRpc]
    private void UpdateRotationClientRpc(float rotation)
    {
        if (IsServer) return; // Server already has the correct rotation
        
        // Apply synced rotation on clients
        transform.rotation = Quaternion.Euler(0, 0, rotation);
        
        // Ensure position is correct (handles any network latency)
        if (pivotPoint != null)
        {
            pivotPoint.position = _initialPosition;
            pivotPoint.rotation = _initialRotation;
        }
    }

    // Collision handling on client side
    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            player.Hit();
        }
    }
}