using Nuthouse.Combat;
using Nuthouse.Core.Events;
using Nuthouse.Player;
using UnityEngine;

namespace Nuthouse.Health
{
    public sealed class Health : IDamageable
    {
        private readonly HealthConfig cfg;
        private readonly PlayerContext ctx;
        private float current;
        private float invulnTimer;

        public Health(HealthConfig config, PlayerContext context)
        {
            cfg = config;
            ctx = context;
            current = cfg != null ? cfg.maxHealth : 100f;
        }

        public float Current => current;
        public bool IsDead { get; private set; }
        public bool Invulnerable => invulnTimer > 0f;

        public void TakeDamage(in DamageInfo info)
        {
            if (IsDead) return;
            if (Invulnerable) return;

            current -= info.Amount;
            DamageEvents.PublishDamageTaken(info, null);

            if (current <= 0f)
            {
                current = 0f;
                IsDead = true;
                DamageEvents.PublishDied(null);
                if (ctx != null) ctx.PendingHurt = null;
                return;
            }

            if (cfg != null)
                invulnTimer = cfg.invulnTime;

            if (ctx != null)
                ctx.PendingHurt = new HurtRequest(info, info.Direction, info.Knockback);
        }

        public void Tick(float dt)
        {
            if (invulnTimer > 0f)
                invulnTimer = Mathf.Max(0f, invulnTimer - dt);
        }
    }
}