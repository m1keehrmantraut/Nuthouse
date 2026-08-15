using UnityEngine;

namespace Nuthouse.Player.Movement
{
    [CreateAssetMenu(menuName = "Nuthouse/Movement Config")]
    public sealed class MovementConfig : ScriptableObject
    {
        [Header("Горизонталь")]
        public float walkSpeed = 6f;
        public float runSpeed = 10f;
        public float crouchSpeed = 2.5f;
        public float accel = 90f;        // разгон на земле
        public float airAccel = 55f;     // разгон в воздухе
        public float decel = 130f;       // торможение без ввода

        [Header("Прыжок")]
        public float coyoteTime = 0.12f; // сек: можно прыгнуть после схода с края
    }
}