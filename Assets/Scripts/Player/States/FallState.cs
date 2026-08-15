using Nuthouse.Core.StateMachine;
using Nuthouse.Input;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public sealed class FallState : AirborneState
    {
        public FallState(PlayerContext ctx) : base(ctx) { }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (Ctx.Ground.IsGrounded) return;   // переход в Idle/Crouch делает AirborneState.Tick
        }

        // Горизонталь — в физике (FixedTick), в воздухе facing не меняем (04 §5).
        public override void FixedTick(float fdt)
        {
            base.FixedTick(fdt);   // ApplyGravity
            if (!Ctx.ControlEnabled) return;
            float inputX = Ctx.Input.MoveAxis.x;
            Ctx.Motor.MoveHorizontal(inputX, Ctx.MovementConfig.walkSpeed, Ctx.MovementConfig.airAccel, fdt);
        }
    }
}
