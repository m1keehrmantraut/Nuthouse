using Nuthouse.Combat;
using Nuthouse.Core.Events;
using Nuthouse.Core.StateMachine;
using Nuthouse.Input;
using UnityEngine;

namespace Nuthouse.Player.States
{
    public sealed class AxeThrowState : StateBase
    {
        private enum Phase { Windup, Throw, Recovery, Done }
        private Phase phase;
        private float timer;
        private AxeThrowDefinition def;

        public AxeThrowState(PlayerContext ctx) : base(ctx) { }

        public override void Enter()
        {
            def = Ctx.Facade != null ? Ctx.Facade.AxeThrow : null;
            if (def == null)
            {
                phase = Phase.Done;
                return;
            }

            Ctx.Facing?.Lock();
            CombatEvents.PublishThrowStarted(def);

            phase = Phase.Windup;
            timer = def.windup;
        }

        public override void Tick(float dt)
        {
            if (Ctx.Health != null && Ctx.Health.IsDead)
            {
                To<DeadState>();
                return;
            }

            if (Ctx.PendingHurt.HasValue) return;

            timer -= dt;

            switch (phase)
            {
                case Phase.Windup:
                    if (timer <= 0f)
                    {
                        DoThrow();
                        phase = Phase.Recovery;
                        timer = def.recovery;
                    }
                    break;
                case Phase.Recovery:
                    if (timer <= 0f)
                    {
                        phase = Phase.Done;
                    }
                    break;
                case Phase.Done:
                    if (Ctx.Ground.IsGrounded)
                        To<IdleState>();
                    else
                        To<FallState>();
                    break;
            }
        }

        public override void FixedTick(float fdt)
        {
            if (!Ctx.Ground.IsGrounded)
                Ctx.Motor.ApplyGravity(fdt);
        }

        public override void Exit()
        {
            Ctx.Facing?.Unlock();
        }

        private void DoThrow()
        {
            int dir = Ctx.Facing != null ? Ctx.Facing.Direction : 1;
            Ctx.Combat?.TakeAxe();          // расход боезапаса строго в момент броска (нет потери при отмене на windup)
            Ctx.Combat?.SpawnAxe(def, dir);
            CombatEvents.PublishProjectileThrown();
            // AxeCountChanged публикуется внутри PlayerCombat.TakeAxe — второй раз не дублируем.
        }
    }
}