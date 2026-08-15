using Nuthouse.Core.StateMachine;
using Nuthouse.Input;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public abstract class AirborneState : StateBase
    {
        protected AirborneState(PlayerContext ctx) : base(ctx) { }

        public override void Tick(float dt)
        {
            if (Ctx.Health != null && Ctx.Health.IsDead)
            {
                To<DeadState>();
                return;
            }

            if (Ctx.Ground.IsGrounded)
            {
                // На землю из воздуха — в присед, если зажат Crouch, иначе в Idle (02 §5).
                if (Ctx.Input.CrouchHeld) To<CrouchState>();
                else To<IdleState>();
                return;
            }
        }

        public override void FixedTick(float fdt)
        {
            Ctx.Motor.ApplyGravity(fdt);
        }
    }
}
