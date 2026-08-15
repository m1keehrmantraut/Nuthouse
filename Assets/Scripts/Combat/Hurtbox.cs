using Nuthouse.Core.Events;
using Nuthouse.Health;
using UnityEngine;

namespace Nuthouse.Combat
{
    public sealed class Hurtbox : MonoBehaviour
    {
        [SerializeField] private DamageReceiver receiver;
        private IDamageable target;

        private Transform Root => transform.root;

        public void Bind(IDamageable damageable) => target = damageable;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Hitbox>(out var hb)) return;
            if (hb.IsOwnedBy(Root)) return;
            var who = target ?? (IDamageable)receiver;
            if (who == null || !hb.TryHit(who)) return;
            who.TakeDamage(hb.CurrentDamageInfo);
            CombatEvents.PublishAttackHit(hb.CurrentDamageInfo, other.gameObject);   // звук/тряска/«попадание» (06)
        }
    }
}
