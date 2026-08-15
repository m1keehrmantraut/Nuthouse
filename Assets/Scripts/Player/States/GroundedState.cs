using Nuthouse.Core.StateMachine;
using Nuthouse.Input;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public abstract class GroundedState : StateBase
    {
        protected GroundedState(PlayerContext ctx) : base(ctx) { }

        public override void Enter() => Ctx.Motor.ResetVerticalVelocity();

        public override void Tick(float dt)
        {
            if (Ctx.Health != null && Ctx.Health.IsDead) { To<DeadState>(); return; }
            if (!Ctx.Ground.IsGrounded) { To<FallState>(); return; }

            if (!Ctx.ControlEnabled) return;

            if (Ctx.Ground.CoyoteTimeLeft > 0f
                && Ctx.Buffer.TryConsume(InputAction.Jump)) { To<JumpState>(); return; }
            if (Ctx.Buffer.TryConsume(InputAction.HeavyAttack)) { Ctx.PendingHeavyAttack = true;  To<AttackState>(); return; }
            if (Ctx.Buffer.TryConsume(InputAction.Attack))    { Ctx.PendingHeavyAttack = false; To<AttackState>(); return; }
            if (Ctx.Combat != null && Ctx.Combat.HasAxe
                && Ctx.Buffer.TryConsume(InputAction.Throw)) { To<AxeThrowState>(); return; }
        }

        public override void FixedTick(float fdt)
        {
            Ctx.Motor.ApplyGroundFrictionIfNoInput(Ctx.Input.MoveAxis.x, fdt);
            Ctx.Facing.UpdateByInput(Ctx.Input.MoveAxis.x);
        }
    }
}
