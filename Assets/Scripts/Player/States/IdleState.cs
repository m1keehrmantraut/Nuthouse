using Nuthouse.Core.StateMachine;
using Nuthouse.Input;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public sealed class IdleState : GroundedState
    {
        public IdleState(PlayerContext ctx) : base(ctx) { }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (!Ctx.ControlEnabled) return;

            float inputX = Ctx.Input.MoveAxis.x;
            if (Mathf.Abs(inputX) > 0.01f)
            {
                To<RunState>();
                return;
            }

            if (Ctx.Input.CrouchHeld)
            {
                To<CrouchState>();
                return;
            }
        }
    }
}
