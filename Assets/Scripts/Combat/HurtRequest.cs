using UnityEngine;

namespace Nuthouse.Combat
{
    public readonly struct HurtRequest
    {
        public readonly DamageInfo Info;
        public readonly Vector2 Direction;
        public readonly float Force;

        public HurtRequest(DamageInfo info, Vector2 direction, float force)
        {
            Info = info;
            Direction = direction;
            Force = force;
        }
    }
}
