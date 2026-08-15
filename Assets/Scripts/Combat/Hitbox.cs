using System.Collections.Generic;
using Nuthouse.Core.Events;
using UnityEngine;

namespace Nuthouse.Combat
{
    public sealed class Hitbox : MonoBehaviour
    {
        public DamageInfo CurrentDamageInfo { get; private set; }
        private Transform ownerRoot;
        private readonly HashSet<IDamageable> alreadyHit = new();

        public void Setup(in DamageInfo info, Transform ownerRoot)
        {
            CurrentDamageInfo = info;
            this.ownerRoot = ownerRoot;
        }

        public void ResetHits() => alreadyHit.Clear();

        public bool TryHit(IDamageable target) => alreadyHit.Add(target);

        public bool IsOwnedBy(Transform root) => ownerRoot == root;

        // Transition fallback: наносит урон целям без Hurtbox (старые враги).
        // Удаляется, когда у всех врагов появится Hurtbox (05 §5).
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActiveAndEnabled) return;
            if (other.TryGetComponent<Hurtbox>(out _)) return;
            if (!other.TryGetComponent<IDamageable>(out var target)) return;
            if (!TryHit(target)) return;
            target.TakeDamage(CurrentDamageInfo);
            CombatEvents.PublishAttackHit(CurrentDamageInfo, other.gameObject);
        }
    }
}