using Nuthouse.Core.StateMachine;
using Nuthouse.Input;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public sealed class RunState : GroundedState
    {
        public RunState(PlayerContext ctx) : base(ctx) { }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (!Ctx.ControlEnabled) return;

            float inputX = Ctx.Input.MoveAxis.x;
            if (Mathf.Abs(inputX) <= 0.01f)
            {
                To<IdleState>();
                return;
            }

            if (Ctx.Input.CrouchHeld)
            {
                To<CrouchState>();
                return;
            }

            bool wantRun = Ctx.Input.RunHeld && inputX != 0f && Ctx.Stamina.CanSprint;
            Ctx.Stamina.SetSprinting(wantRun);
        }

        public override void FixedTick(float fdt)
        {
            base.FixedTick(fdt);
            float inputX = Ctx.Input.MoveAxis.x;
            bool running = Ctx.Input.RunHeld && Ctx.Stamina.CanSprint;
            float targetSpeed = running ? Ctx.MovementConfig.runSpeed : Ctx.MovementConfig.walkSpeed;
            Ctx.Motor.MoveHorizontal(inputX, targetSpeed, Ctx.MovementConfig.accel, fdt);
        }
    }
}
