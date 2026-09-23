using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public struct NCharInput : INetworkInput
{
    public Vector2 MoveInput;
    public bool JumpInput;
}

public class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] private InputActionReference moveInput;
    [SerializeField] private InputActionReference jumpInput;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rayLength = 0.05f;
    [SerializeField] private LayerMask layerMask = -1;

    [Networked] public Vector3 Velocity { get; private set; }
    private readonly List<LagCompensatedHit> _hits = new List<LagCompensatedHit>();

    [Networked] public Vector3 MoveDir { get; private set; }
    [Networked] public bool Jumping { get; private set; }
    [Networked] public bool OnGround { get; private set; }

    [SerializeField] private NetworkTransform netTransform;


    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Runner.GetBehaviour<NetworkEvents>().OnInput.AddListener(OnInput);
        }

    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput<NCharInput>(out var input))
            return;


        OnGround = Physics.Raycast(transform.position, Vector3.down, rayLength, layerMask);

        Vector3 velocity = Velocity;

        if (OnGround && velocity.y < 0f)
            velocity.y = -2f;

        Jumping = input.JumpInput && OnGround;

        if (Jumping)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (!OnGround)
            velocity.y += gravity * Runner.DeltaTime;
        else
        {
            if (!Jumping)
                velocity = Vector3.zero;
        }

        Velocity = velocity;

        MoveDir = new Vector3(
            input.MoveInput.x,
            0f,
            input.MoveInput.y
        );

        if (MoveDir.sqrMagnitude > 1f)
            MoveDir.Normalize();

        Vector3 movement = MoveDir * moveSpeed;
        movement.y = Velocity.y;

        netTransform.Teleport(this.transform.position + movement * Runner.DeltaTime);
        //this.transform.position += movement * Runner.DeltaTime;
        if (MoveDir.sqrMagnitude > 0.01f)
            this.transform.forward = MoveDir;

    }

    private NCharInput charInput;

    private void OnInput(NetworkRunner runner, NetworkInput input)
    {
        charInput.MoveInput = moveInput.action.ReadValue<Vector2>();
        charInput.JumpInput = jumpInput.action.IsPressed();

        input.Set(charInput);
    }
}