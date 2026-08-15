using Nuthouse.Combat;
using Nuthouse.Core.Events;
using UnityEngine;

namespace Nuthouse.Player.Combat
{
    public sealed class PlayerCombat
    {
        private readonly PlayerContext ctx;
        private readonly AttackDefinition primaryAttack;
        private readonly AxeThrowDefinition axeThrow;
        private Transform handAnchor;

        public int AxesCount { get; private set; }
        public bool HasAxe => AxesCount > 0;

        public PlayerCombat(PlayerContext context, AttackDefinition primary, AxeThrowDefinition axe)
        {
            ctx = context;
            primaryAttack = primary;
            axeThrow = axe;
            if (axeThrow != null) AxesCount = axeThrow.startAxeCount;
        }

        public void TakeAxe()
        {
            if (AxesCount > 0)
            {
                AxesCount--;
                CombatEvents.PublishAxeCountChanged(AxesCount);
            }
        }

        public void ReturnAxe(int count)
        {
            AxesCount += count;
            CombatEvents.PublishAxeCountChanged(AxesCount);
        }

        public Nuthouse.Combat.Projectiles.AxeProjectile SpawnAxe(AxeThrowDefinition def, int direction)
        {
            if (def == null || def.axePrefab == null) return null;

            Vector2 origin = ctx.Facade != null ? ctx.Facade.transform.position : Vector2.zero;
            Vector2 ownerVel = ctx.Motor != null ? ctx.Motor.Velocity : Vector2.zero;
            Vector2 dir = new Vector2(direction, 0f);
            Transform owner = ctx.Facade != null ? ctx.Facade.transform : null;

            var axe = Object.Instantiate(def.axePrefab, origin, Quaternion.identity);
            axe.Launch(def, dir, ownerVel, owner);
            return axe;
        }
    }
}