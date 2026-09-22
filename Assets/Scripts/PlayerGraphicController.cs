using Animancer;
using Fusion;
using Unity.VisualScripting;
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

        idleState.SetWeight(0);
        walkState.SetWeight(0);

        animancer.Layers[0].Weight = 1;
    }


    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        //if (controller == null) return;

        if (controller.MoveDir.magnitude >=0.01f)
        {
            idleState.SetWeight(0);
            walkState.SetWeight(1);
        }
        else
        {
            idleState.SetWeight(1);
            walkState.SetWeight(0);
        }

        idleState.Time = Runner.SimulationTime;
        walkState.Time = Runner.DeltaTime * Runner.Tick;
        animancer.Evaluate();
    }


    public override void Render()
    {
        return;
        if (controller == null) return;
        if (animancer == null) return;

        if (!controller.OnGround)
        {
            animancer.Play(_OnAir, 0.25f);
        }
        else
        {
            if (controller.MoveDir.magnitude >= 0.01f)
            {
                animancer.Play(_Walk, 0.25f);
                animancer.States.Current.Speed = controller.MoveDir.magnitude;
            }
            else
            {
                animancer.Play(_Idle, 0.25f);
            }
        }
    }
}
