using Unity.Netcode;
using UnityEngine;

public class OrangeBarVertical : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float moveDistance = 3f; // Vertical distance to travel
    [SerializeField] private bool startMovingUp = true; // Check for initial direction

    private bool _isMovingUp;
    private Vector3 _upTarget;
    private Vector3 _downTarget;
    private Vector3 _currentTarget;
    private Vector3 _startPosition;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            InitializeMovement();
        }
    }

    private void InitializeMovement()
    {
        _startPosition = transform.position;
        
        // Calculate movement endpoints (up/down)
        _upTarget = _startPosition + Vector3.up * moveDistance;
        _downTarget = _startPosition + Vector3.down * moveDistance;
        
        // Set initial direction
        _isMovingUp = startMovingUp;
        _currentTarget = _isMovingUp ? _upTarget : _downTarget;
    }

    private void Update()
    {
        // Only run movement logic on the server
        if (!IsServer) return;

        // Move platform vertically toward current target
        transform.position = Vector3.MoveTowards(
            transform.position,
            _currentTarget,
            movementSpeed * Time.deltaTime
        );

        // Check if reached target
        if (Vector3.Distance(transform.position, _currentTarget) < 0.01f)
        {
            // Switch direction and update target
            _isMovingUp = !_isMovingUp;
            _currentTarget = _isMovingUp ? _upTarget : _downTarget;
        }
    }
}