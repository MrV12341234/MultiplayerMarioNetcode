using Unity.Netcode;
using UnityEngine;

public class NetworkMarioAnimator : NetworkBehaviour
{
    private Player player;
    private PlayerMovement movement;

    // Network variables for synchronization
    private readonly NetworkVariable<int> currentForm = new NetworkVariable<int>(
        0, 
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    
    private readonly NetworkVariable<bool> isRunning = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    
    private readonly NetworkVariable<bool> isJumping = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    
    private readonly NetworkVariable<bool> isSliding = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Awake()
    {
        player = GetComponent<Player>();
        movement = GetComponent<PlayerMovement>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Only owner should update these variables
        if (IsOwner)
        {
            // Initialize with current state
            currentForm.Value = player.fire ? 2 : player.big ? 1 : 0;
            isRunning.Value = movement.running;
            isJumping.Value = movement.jumping;
            isSliding.Value = movement.sliding;
        }
        
        // All clients should apply initial state
        ApplyFormState(currentForm.Value);
        ApplyAnimationState();
        
        // Subscribe to changes
        currentForm.OnValueChanged += OnFormChanged;
        isRunning.OnValueChanged += OnRunningChanged;
        isJumping.OnValueChanged += OnJumpingChanged;
        isSliding.OnValueChanged += OnSlidingChanged;
    }

    private void Update()
    {
        // Only owner updates the network variables
        if (!IsOwner) return;
        
        // Update form state
        int formState = player.fire ? 2 : player.big ? 1 : 0;
        if (currentForm.Value != formState)
        {
            currentForm.Value = formState;
        }
        
        // Update animation states
        if (isRunning.Value != movement.running)
        {
            isRunning.Value = movement.running;
        }
        if (isJumping.Value != movement.jumping)
        {
            isJumping.Value = movement.jumping;
        }
        if (isSliding.Value != movement.sliding)
        {
            isSliding.Value = movement.sliding;
        }
    }

    private void OnFormChanged(int previous, int current)
    {
        ApplyFormState(current);
    }

    private void ApplyFormState(int form)
    {
        player.smallRenderer.enabled = (form == 0);
        player.bigRenderer.enabled = (form == 1);
        player.fireRenderer.enabled = (form == 2);

        // Update active renderer reference
        if (form == 2) player.activeRenderer = player.fireRenderer;
        else if (form == 1) player.activeRenderer = player.bigRenderer;
        else player.activeRenderer = player.smallRenderer;
        
        // Re-apply animation state after form change
        ApplyAnimationState();
    }

    private void OnRunningChanged(bool previous, bool current)
    {
        ApplyAnimationState();
    }

    private void OnJumpingChanged(bool previous, bool current)
    {
        ApplyAnimationState();
    }

    private void OnSlidingChanged(bool previous, bool current)
    {
        ApplyAnimationState();
    }

    private void ApplyAnimationState()
    {
        if (player.activeRenderer == null) return;

        // Apply the current animation state
        player.activeRenderer.run.enabled = isRunning.Value;
        
        if (isJumping.Value)
        {
            player.activeRenderer.spriteRenderer.sprite = player.activeRenderer.jump;
        }
        else if (isSliding.Value)
        {
            player.activeRenderer.spriteRenderer.sprite = player.activeRenderer.slide;
        }
        else if (!isRunning.Value)
        {
            player.activeRenderer.spriteRenderer.sprite = player.activeRenderer.idle;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        currentForm.OnValueChanged -= OnFormChanged;
        isRunning.OnValueChanged -= OnRunningChanged;
        isJumping.OnValueChanged -= OnJumpingChanged;
        isSliding.OnValueChanged -= OnSlidingChanged;
    }
}