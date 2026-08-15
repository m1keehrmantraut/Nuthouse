using Nuthouse.Core.StateMachine;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public sealed class DeadState : StateBase
    {
        public DeadState(PlayerContext ctx) : base(ctx) { }

        public override void Enter()
        {
            Ctx.Motor.ResetVerticalVelocity();
        }

        public override void Tick(float dt)
        {
        }

        public override void FixedTick(float fdt)
        {
            // Труп не должен катиться/ехать по инерции.
            Ctx.Motor.ApplyGroundFrictionIfNoInput(0f, fdt);
            if (!Ctx.Ground.IsGrounded)
                Ctx.Motor.ApplyGravity(fdt);
        }
    }
}