using UnityEngine;

namespace Nuthouse.Health
{
    [CreateAssetMenu(menuName = "Nuthouse/Health Config")]
    public sealed class HealthConfig : ScriptableObject
    {
        public float maxHealth = 100f;
        public float invulnTime = 0.5f;
        public float staggerTime = 0.4f;
    }
}