using Nuthouse.Combat.Projectiles;
using UnityEngine;

namespace Nuthouse.Combat
{
    [CreateAssetMenu(menuName = "Nuthouse/Axe Throw Definition")]
    public sealed class AxeThrowDefinition : ScriptableObject
    {
        [Header("Фазы броска")]
        public float windup = 0.12f;
        public float recovery = 0.30f;

        [Header("Полёт")]
        public float throwForce = 18f;
        public float upwardBias = 2f;
        public bool inheritOwnerVelocity = true;
        public float spinSpeed = 720f;
        public bool physicalSpin = false;

        [Header("Урон")]
        public float damage = 35f;
        public float knockback = 6f;

        [Header("Поведение при контакте")]
        public bool stickToEnemy = true;
        public bool stickToWorld = true;
        public float bounceOnNoStick = 0.3f;
        public float despawnTime = 0f;

        [Header("Ссылки и боезапас")]
        public AxeProjectile axePrefab;
        public string animTrigger = "ThrowAxe";
        public int startAxeCount = 2;
    }
}
