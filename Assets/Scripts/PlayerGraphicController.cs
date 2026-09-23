using Animancer;
using Fusion;
using UnityEngine;

public class PlayerGraphicController : NetworkBehaviour
{
    private AnimancerComponent animancer;
    private NetworkPlayerController controller;
    [SerializeField] private AnimationClip _Idle;
    [SerializeField] private AnimationClip _Walk;
    [SerializeField] private AnimationClip _OnAir;


    private AnimancerState idleState;
    private AnimancerState walkState;
    private AnimancerState onAirState;

    public override void Spawned()
    {
        controller = GetComponentInParent<NetworkPlayerController>();
        animancer = GetComponent<AnimancerComponent>();

        //idleState = animancer.States.GetOrCreate(_Idle);
        //idleState.SetWeight(0);

        //walkState = animancer.States.GetOrCreate(_Walk);
        //walkState.SetWeight(0);

        idleState = animancer.Layers[0].GetOrCreateState(_Idle);
        walkState = animancer.Layers[0].GetOrCreateState(_Walk);
        onAirState = animancer.Layers[0].GetOrCreateState(_OnAir);

        idleState.SetWeight(0);
        walkState.SetWeight(0);
        onAirState.SetWeight(0);

        animancer.Layers[0].Weight = 0;
    }


    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        //if (controller == null) return;

        if (!controller.OnGround)
        {
            idleState.SetWeight(0);
            walkState.SetWeight(0);
            onAirState.SetWeight(1);
        }
        else
        {
            if (controller.MoveDir.magnitude >= 0.01f)
            {
                idleState.SetWeight(0);
                walkState.SetWeight(1);
            }
            else
            {
                idleState.SetWeight(1);
                walkState.SetWeight(0);
            }
            onAirState.SetWeight(0);
        }

        idleState.Time = Runner.SimulationTime;
        walkState.Time = Runner.DeltaTime * Runner.Tick;
        onAirState.Time = Runner.SimulationTime;

        animancer.Evaluate();

    }


    public override void Render()
    {
        //return;
        if (HasStateAuthority) return;
        if (controller == null) return;
        if (animancer == null) return;

        float time = IsProxy ? Runner.RemoteRenderTime : Runner.LocalRenderTime;

        if (!controller.OnGround)
        {
            walkState.Weight = 0;
            idleState.Weight = 0;
            onAirState.Weight = 1;
        }
        else
        {

            if (controller.MoveDir.magnitude >= 0.01f)
            {
                walkState.Weight = 1;
                idleState.Weight = 0;
            }
            else
            {
                walkState.Weight = 0;
                idleState.Weight = 1;
            }
            onAirState.Weight = 0;
        }

        walkState.Time = time;
        idleState.Time = time;
        onAirState.Time = time;

        animancer.Evaluate();
    }
}
