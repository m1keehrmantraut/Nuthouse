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

            // Атака в воздухе как в Dead Cells: буфер ловится прямо в полёте.
            if (Ctx.ControlEnabled)
            {
                if (Ctx.Buffer.TryConsume(InputAction.HeavyAttack)) { Ctx.PendingHeavyAttack = true;  To<AttackState>(); return; }
                if (Ctx.Buffer.TryConsume(InputAction.Attack))      { Ctx.PendingHeavyAttack = false; To<AttackState>(); return; }
            }

            if (Ctx.Ground.JustLanded)
            {
                if (Ctx.Input.CrouchHeld) To<CrouchState>();
                else To<IdleState>();
                return;
            }
        }

        public override void FixedTick(float fdt)
        {
            Ctx.Motor.ApplyGravity(fdt);
            if (Ctx.ControlEnabled)
                Ctx.Facing.UpdateByInput(Ctx.Input.MoveAxis.x);   // разворот в воздухе (Dead Cells)
        }
    }
}
