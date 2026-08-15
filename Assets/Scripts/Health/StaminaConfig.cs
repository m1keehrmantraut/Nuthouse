using UnityEngine;

namespace Nuthouse.Health
{
    [CreateAssetMenu(menuName = "Nuthouse/Stamina Config")]
    public sealed class StaminaConfig : ScriptableObject
    {
        public float maxStamina = 100f;
        public float decreaseRate = 30f;
        public float increaseRate = 15f;
        public float recoveryThreshold = 20f;
    }
}
