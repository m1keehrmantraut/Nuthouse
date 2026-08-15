using Nuthouse.Combat;
using Nuthouse.Combat.Projectiles;

namespace Nuthouse.Core.Events
{
    public static class CombatEvents
    {
        public static event System.Action<AttackDefinition> AttackStarted;
        public static event System.Action<DamageInfo, UnityEngine.GameObject> AttackHit;
        public static event System.Action<AttackDefinition> AttackWhiffed;
        public static event System.Action<AxeThrowDefinition> ThrowStarted;
        public static event System.Action ProjectileThrown;
        public static event System.Action<AxeProjectile, UnityEngine.GameObject> AxeStuck;
        public static event System.Action AxePickedUp;
        public static event System.Action<int> AxeCountChanged;

        public static void PublishAttackStarted(AttackDefinition def) => AttackStarted?.Invoke(def);
        public static void PublishAttackHit(DamageInfo info, UnityEngine.GameObject target) => AttackHit?.Invoke(info, target);
        public static void PublishAttackWhiffed(AttackDefinition def) => AttackWhiffed?.Invoke(def);
        public static void PublishThrowStarted(AxeThrowDefinition def) => ThrowStarted?.Invoke(def);
        public static void PublishProjectileThrown() => ProjectileThrown?.Invoke();
        public static void PublishAxeStuck(AxeProjectile projectile, UnityEngine.GameObject surface) => AxeStuck?.Invoke(projectile, surface);
        public static void PublishAxePickedUp() => AxePickedUp?.Invoke();
        public static void PublishAxeCountChanged(int count) => AxeCountChanged?.Invoke(count);
    }
}
