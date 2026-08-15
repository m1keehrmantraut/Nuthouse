using Nuthouse.Combat;
using Nuthouse.Core.Events;
using UnityEngine;

namespace Nuthouse.Health
{
    public class DamageReceiver : MonoBehaviour, IDamageable
    {
        [SerializeField] protected HealthConfig config;
        protected float currentHealth;

        protected virtual void Awake()
        {
            currentHealth = config != null ? config.maxHealth : 100f;
        }

        public virtual void TakeDamage(in DamageInfo info)
        {
            currentHealth -= info.Amount;
            if (currentHealth <= 0f)
                Die();
        }

        protected virtual void Die()
        {
            DamageEvents.PublishDied(gameObject);
        }
    }
}
