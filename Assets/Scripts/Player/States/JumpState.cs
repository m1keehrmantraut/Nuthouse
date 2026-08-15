using Nuthouse.Core.StateMachine;
using Nuthouse.Input;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public sealed class JumpState : AirborneState
    {
        private bool jumpCutDone;

        public JumpState(PlayerContext ctx) : base(ctx) { }

        public override void Enter()
        {
            jumpCutDone = false;
            Ctx.Ground.ConsumeCoyote();
            Ctx.Motor.Jump(Ctx.JumpConfig != null ? Ctx.JumpConfig.jumpForce : 16f);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (Ctx.Ground.IsGrounded) return;   // база уже перевела в Idle/Crouch при приземлении

            if (Ctx.Motor.VerticalVelocity <= 0f)
            {
                To<FallState>();
                return;
            }

            // Jump-cut ровно один раз на отпускание кнопки (переменная высота прыжка, 04 §3).
            // Раньше срабатывал каждый кадр → vy схлопывался и прыжок ломался.
            if (!jumpCutDone && !Ctx.Input.JumpHeld)
            {
                Ctx.Motor.CutJump();
                jumpCutDone = true;
            }
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