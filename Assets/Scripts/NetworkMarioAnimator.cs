using Unity.Netcode;
using UnityEngine;

/// <summary>Synchronises Mario’s action & form for all clients.</summary>
public class NetworkMarioAnimator : NetworkBehaviour
{
    public enum MarioForm : byte { Small, Big, Fire }
    public enum MarioAction : byte { Idle, Run, Jump, Slide }

    // Everyone can read, only the owner can write
    private readonly NetworkVariable<MarioForm>  form   =
        new(MarioForm.Small, NetworkVariableReadPermission.Everyone);

    private readonly NetworkVariable<MarioAction> action =
        new(MarioAction.Idle,  NetworkVariableReadPermission.Everyone);

    /* --- References set in Inspector --- */
    [Header("Child Renderers")]
    [SerializeField] private GameObject smallGO;
    [SerializeField] private GameObject bigGO;
    [SerializeField] private GameObject fireGO;

    private PlayerMovement move;
    private Player        player;

    /* ---------------- */

    private void Awake()
    {
        move   = GetComponent<PlayerMovement>();
        player = GetComponent<Player>();
    }
    

    public override void OnNetworkSpawn()
    {
        // Apply initial visuals
        ApplyForm(form.Value);
        ApplyAction(action.Value);

        // React when a value changes on *any* client
        form  .OnValueChanged += (_, n) => ApplyForm(n);
        action.OnValueChanged += (_, n) => ApplyAction(n);
    }

    private void Update()
    {
        if (!IsOwner) return;            // Only the local player writes

        // --- Decide current action ---
        MarioAction a =
            move.jumping ? MarioAction.Jump :
            move.sliding ? MarioAction.Slide :
            move.running ? MarioAction.Run  :
                           MarioAction.Idle;

        if (a != action.Value)   action.Value = a;

        // --- Decide current form ---
        MarioForm f =
            player.firepower ? MarioForm.Fire :
            player.big       ? MarioForm.Big  :
                               MarioForm.Small;

        if (f != form.Value)     form.Value = f;
    }

    /* ===== Helpers to flip visuals when values arrive ===== */

    private void ApplyForm(MarioForm f)
    {
        smallGO.SetActive(f == MarioForm.Small);
        bigGO  .SetActive(f == MarioForm.Big);
        fireGO .SetActive(f == MarioForm.Fire);
    }

    private void ApplyAction(MarioAction a)
    {
        // We can simply enable / disable the run AnimatedSprite
        var rend = CurrentRenderer();
        rend.run.enabled = (a == MarioAction.Run);

        rend.spriteRenderer.sprite =
            a switch {
                MarioAction.Jump  => rend.jump,
                MarioAction.Slide => rend.slide,
                _                 => rend.idle
            };
    }

    private PlayerSpriteRenderer CurrentRenderer() =>
        form.Value switch {
            MarioForm.Big  => bigGO .GetComponent<PlayerSpriteRenderer>(),
            MarioForm.Fire => fireGO.GetComponent<PlayerSpriteRenderer>(),
            _              => smallGO.GetComponent<PlayerSpriteRenderer>(),
        };
}
