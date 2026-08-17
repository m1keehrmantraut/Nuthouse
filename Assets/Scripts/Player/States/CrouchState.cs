using Nuthouse.Core.StateMachine;
using Nuthouse.Input;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public sealed class CrouchState : GroundedState
    {
        public CrouchState(PlayerContext ctx) : base(ctx) { }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (!Ctx.ControlEnabled) return;

            if (!Ctx.Input.CrouchHeld && !Ctx.Ground.CeilingBlocked)
            {
                To<IdleState>();
                return;
            }

            // Уберем временно переход из сидячего в бег
            // if (Ctx.Input.RunHeld)
            // {
            //     To<RunState>();
            //     return;
            // }
        }

        public override void FixedTick(float fdt)
        {
            base.FixedTick(fdt);
            float inputX = Ctx.Input.MoveAxis.x;
            Ctx.Motor.MoveHorizontal(inputX, Ctx.MovementConfig.crouchSpeed, Ctx.MovementConfig.accel, fdt);
        }
    }
}
