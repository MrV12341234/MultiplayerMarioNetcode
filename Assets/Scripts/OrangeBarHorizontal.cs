using Unity.Netcode;
using UnityEngine;

public class OrangeBarHorizontal : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float moveDistance = 5f;
    [SerializeField] private bool startMovingRight = true;

    private bool _isMovingRight;
    private Vector3 _rightTarget;
    private Vector3 _leftTarget;
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
        
        // Calculate movement endpoints
        _rightTarget = _startPosition + Vector3.right * moveDistance;
        _leftTarget = _startPosition + Vector3.left * moveDistance;
        
        // Set initial direction
        _isMovingRight = startMovingRight;
        _currentTarget = _isMovingRight ? _rightTarget : _leftTarget;
    }

    private void Update()
    {
        // Only run movement logic on the server
        if (!IsServer) return;

        // Move platform towards current target
        transform.position = Vector3.MoveTowards(
            transform.position,
            _currentTarget,
            movementSpeed * Time.deltaTime
        );

        // Check if reached target
        if (Vector3.Distance(transform.position, _currentTarget) < 0.01f)
        {
            // Switch direction and update target
            _isMovingRight = !_isMovingRight;
            _currentTarget = _isMovingRight ? _rightTarget : _leftTarget;
        }
    }
}