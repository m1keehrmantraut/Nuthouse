using UnityEngine;

namespace Nuthouse.Combat
{
    public enum DamageType { Physical, Ranged }

    public readonly struct DamageInfo
    {
        public readonly float Amount;
        public readonly DamageType Type;
        public readonly Vector2 Direction;
        public readonly float Knockback;
        public readonly GameObject Source;

        public DamageInfo(float amount, DamageType type, Vector2 direction, float knockback, GameObject source)
        {
            Amount = amount;
            Type = type;
            Direction = direction;
            Knockback = knockback;
            Source = source;
        }
    }
}
