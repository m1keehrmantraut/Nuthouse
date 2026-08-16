using Nuthouse.Combat;
using Nuthouse.Core.StateMachine;
using Nuthouse.Input;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public sealed class AttackState : StateBase
    {
        private AttackRunner runner;

        public AttackState(PlayerContext ctx) : base(ctx) { }

        public override void Enter()
        {
            bool heavy = Ctx.PendingHeavyAttack;   // выставлен GroundedState по HeavyAttack-команде
            Ctx.PendingHeavyAttack = false;
            AttackDefinition def = Ctx.Facade != null
                ? (heavy && Ctx.Facade.HeavyAttack != null ? Ctx.Facade.HeavyAttack : Ctx.Facade.PrimaryAttack)
                : null;
            if (runner == null) runner = new AttackRunner(Ctx);

            Ctx.Facing?.Lock();
            if (def != null && def.lockMovement && Ctx.Ground.IsGrounded)
                Ctx.Motor.ResetVerticalVelocity();   // на земле гасим vert; в воздухе сохраняем (Dead Cells)

            if (def != null && def.lungeImpulse > 0f)
            {
                int dir = Ctx.Facing != null ? Ctx.Facing.Direction : 1;
                Ctx.Motor.ApplyImpulse(new Vector2(dir * def.lungeImpulse, 0f));
            }

            runner.Start(def);
        }

        public override void Tick(float dt)
        {
            if (Ctx.Health != null && Ctx.Health.IsDead)
            {
                To<DeadState>();
                return;
            }

            if (Ctx.PendingHurt.HasValue) return;

            runner.Tick(dt);

            if (runner.Phase == AttackPhase.Recovery && runner.InCancelWindow && Ctx.ControlEnabled
                && Ctx.Buffer.TryConsume(InputAction.Attack)
                && runner.Current != null && runner.Current.nextInCombo != null)
            {
                runner.Start(runner.Current.nextInCombo);
                return;
            }

            if (runner.Phase == AttackPhase.None)
            {
                if (Ctx.Ground.IsGrounded)
                    To<IdleState>();
                else
                    To<FallState>();
            }
        }

        public override void FixedTick(float fdt)
        {
            if (currentLocksMovement())
            {
                // На земле гасим инерцию (05 §6, 04 §9); в воздухе — гравитация, горизонталь сохраняем (Dead Cells).
                if (!Ctx.Ground.IsGrounded)
                    Ctx.Motor.ApplyGravity(fdt);
                else
                    Ctx.Motor.ApplyGroundFrictionIfNoInput(0f, fdt);
                return;
            }
            Ctx.Motor.ApplyGravity(fdt);
        }

        public override void Exit()
        {
            runner?.Cancel();
            Ctx.Facing?.Unlock();
        }

        private bool currentLocksMovement()
        {
            return runner?.Current != null && runner.Current.lockMovement;
        }
    }
}