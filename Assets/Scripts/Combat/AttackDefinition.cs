using UnityEngine;

namespace Nuthouse.Combat
{
    [CreateAssetMenu(menuName = "Nuthouse/Attack Definition")]
    public sealed class AttackDefinition : ScriptableObject
    {
        [Header("Фазы, сек")]
        public float startup = 0.08f;
        public float active = 0.10f;
        public float recovery = 0.22f;

        [Header("Урон")]
        public float damage = 25f;
        public DamageType damageType = DamageType.Physical;
        public float knockback = 4f;

        [Header("Хитбокс")]
        public Vector2 hitboxOffset = new(0.9f, 0f);
        public Vector2 hitboxSize = new(1.2f, 1.0f);

        [Header("Движение")]
        public float lungeImpulse = 0f;
        public bool lockMovement = true;

        [Header("Комбо")]
        public AttackDefinition nextInCombo;
        public float cancelWindowStart = 0.5f;

        [Header("Анимация")]
        public string animTrigger = "Attack1";
    }
}
