using UnityEngine;

namespace Nuthouse.Player.Movement
{
    [CreateAssetMenu(menuName = "Nuthouse/Jump Config")]
    public sealed class JumpConfig : ScriptableObject
    {
        [Header("Прыжок")]
        public float jumpForce = 17f;              // вертикальная скорость при прыжке (полная высота при удержании)
        public float jumpCutMultiplier = 0.5f;    // тап (быстрое отпускание): vy *= это → низкий прыжок. 1 = нет обрезки

        [Header("Гравитация (вертикаль)")]
        public float baseGravityMultiplier = 3f;  // множитель к Physics2D.gravity
        public float riseGravityMultiplier = 1f;  // пока поднимаемся (vy > apex)
        public float fallGravityMultiplier = 2f;  // пока падаем (ниже apex)
        public float maxFallSpeed = 32f;         // потолок скорости падения

        [Header("Зависание на вершине (hang)")]
        public float apexThreshold = 1.5f;       // |vy| ниже этого — считаем вершиной дуги
        public float apexGravityMultiplier = 0.5f;// множитель гравитации у вершины: меньше = зависчивее
    }
}