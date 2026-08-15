using UnityEngine;

namespace Nuthouse.Input
{
    [CreateAssetMenu(menuName = "Nuthouse/Input Config")]
    public sealed class InputConfig : ScriptableObject
    {
        public float jumpBuffer = 0.2f;
        public float attackBuffer = 0.15f;
        public float throwBuffer = 0.15f;
        public float interactBuffer = 0.25f;
        public float heavyAttackHold = 0.25f;   // сек удержания Attack — становится тяжёлой
        public float heavyAttackBuffer = 0.15f;

        public float GetTtl(InputAction a) => a switch
        {
            InputAction.Jump => jumpBuffer,
            InputAction.Attack => attackBuffer,
            InputAction.HeavyAttack => heavyAttackBuffer,
            InputAction.Throw => throwBuffer,
            InputAction.Interact => interactBuffer,
            _ => 0.1f,
        };
    }
}
