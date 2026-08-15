using System;
using Nuthouse.Core.Events;
using Nuthouse.Player;
using UnityEngine;

namespace Nuthouse.Combat
{
    public enum AttackPhase { None, Startup, Active, Recovery }

    public sealed class AttackRunner
    {
        private readonly PlayerContext ctx;
        private AttackDefinition current;
        private AttackPhase phase = AttackPhase.None;
        private float phaseTimer;
        private float recoveryElapsed;
        private bool inCancelWindow;
        private GameObject hitboxGo;
        private BoxCollider2D hitboxCollider;
        private Hitbox hitbox;

        public AttackPhase Phase => phase;
        public AttackDefinition Current => current;
        public bool InCancelWindow => inCancelWindow;
        public event Action<AttackDefinition> AttackFinished;

        public AttackRunner(PlayerContext context)
        {
            ctx = context;
        }

        public void Start(AttackDefinition def)
        {
            current = def;
            phase = AttackPhase.Startup;
            phaseTimer = def.startup;
            recoveryElapsed = 0f;
            inCancelWindow = false;
            EnsureHitbox();
            if (hitboxGo != null) hitboxGo.SetActive(false);
            hitbox?.ResetHits();

            CombatEvents.PublishAttackStarted(def);
        }

        public void Tick(float dt)
        {
            if (phase == AttackPhase.None || current == null) return;

            phaseTimer -= dt;

            if (phase == AttackPhase.Startup)
            {
                if (phaseTimer <= 0f)
                {
                    phase = AttackPhase.Active;
                    phaseTimer = current.active;
                    ActivateHitbox();
                }
            }
            else if (phase == AttackPhase.Active)
            {
                if (phaseTimer <= 0f)
                {
                    phase = AttackPhase.Recovery;
                    phaseTimer = current.recovery;
                    recoveryElapsed = 0f;
                    DeactivateHitbox();
                }
            }
            else if (phase == AttackPhase.Recovery)
            {
                recoveryElapsed += dt;
                inCancelWindow = recoveryElapsed >= current.recovery * current.cancelWindowStart;

                if (phaseTimer <= 0f)
                {
                    phase = AttackPhase.None;
                    current = null;
                    inCancelWindow = false;
                    AttackFinished?.Invoke(null);
                }
            }
        }

        public void Cancel()
        {
            phase = AttackPhase.None;
            current = null;
            inCancelWindow = false;
            DeactivateHitbox();
        }

        private void EnsureHitbox()
        {
            if (hitboxGo != null) return;
            if (ctx?.HitboxRoot == null) return;

            hitboxGo = new GameObject("MeleeHitbox");
            hitboxGo.transform.SetParent(ctx.HitboxRoot, false);
            var rb = hitboxGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            hitboxCollider = hitboxGo.AddComponent<BoxCollider2D>();
            hitboxCollider.isTrigger = true;
            hitbox = hitboxGo.AddComponent<Hitbox>();
            hitboxGo.SetActive(false);
        }

        private void ActivateHitbox()
        {
            if (hitboxGo == null || current == null) return;

            int dir = ctx.Facing != null ? ctx.Facing.Direction : 1;
            Vector2 offset = current.hitboxOffset;
            // HitboxRoot — чайлд корня игрока, который уже отзеркален Facing через
            // localScale.x. Смещение НЕ умножаем на dir, иначе хитбокс съезжает на
            // одну сторону при повороте (05 §4).
            hitboxGo.transform.localPosition = offset;
            hitboxCollider.size = current.hitboxSize;

            int ownerLayer = ctx.Facade != null ? ctx.Facade.gameObject.layer : 0;
            var info = new DamageInfo(current.damage, current.damageType, new Vector2(dir, 0f), current.knockback, ctx.Facade != null ? ctx.Facade.gameObject : null);
            hitbox.Setup(info, ctx.Facade != null ? ctx.Facade.transform : null);
            hitbox.ResetHits();
            hitboxGo.SetActive(true);
        }

        private void DeactivateHitbox()
        {
            if (hitboxGo != null) hitboxGo.SetActive(false);
        }
    }
}