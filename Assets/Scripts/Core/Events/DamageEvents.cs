using Nuthouse.Combat;
using UnityEngine;

namespace Nuthouse.Core.Events
{
    public static class DamageEvents
    {
        public static event System.Action<DamageInfo, GameObject> DamageTaken;
        public static event System.Action<GameObject> Died;

        public static void PublishDamageTaken(in DamageInfo info, GameObject target)
            => DamageTaken?.Invoke(info, target);

        public static void PublishDied(GameObject target)
            => Died?.Invoke(target);
    }
}
