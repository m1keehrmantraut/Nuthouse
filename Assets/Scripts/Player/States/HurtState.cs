using Nuthouse.Core.StateMachine;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public sealed class HurtState : StateBase
    {
        private float timer;

        public HurtState(PlayerContext ctx) : base(ctx) { }

        public override void Enter()
        {
            timer = Ctx.HealthConfig != null && Ctx.HealthConfig.staggerTime > 0f
                ? Ctx.HealthConfig.staggerTime
                : 0.4f;

            if (Ctx.PendingHurt.HasValue)
            {
                var req = Ctx.PendingHurt.Value;
                Ctx.Motor.Knockback(req.Direction, req.Force);
                Ctx.PendingHurt = null;
            }
        }

        public override void Tick(float dt)
        {
            if (Ctx.Health != null && Ctx.Health.IsDead)
            {
                To<DeadState>();
                return;
            }

            timer -= dt;
            if (timer <= 0f)
            {
                if (Ctx.Ground.IsGrounded)
                    To<IdleState>();
                else
                    To<FallState>();
            }
        }

        public override void FixedTick(float fdt)
        {
            if (!Ctx.Ground.IsGrounded)
                Ctx.Motor.ApplyGravity(fdt);
        }
    }
}