using UnityEngine;

namespace Nuthouse.Player.Movement
{
    public sealed class GroundDetector
    {
        private readonly Transform feet;
        private readonly MovementConfig cfg;
        private readonly LayerMask groundLayer;
        private readonly Transform ceiling;
        private const float GroundRadius = 0.2f;
        private const float CeilingRadius = 0.2f;

        public bool IsGrounded { get; private set; }
        public bool JustLanded { get; private set; }
        public bool CeilingBlocked { get; private set; }
        public Vector2 GroundNormal { get; private set; }
        public float CoyoteTimeLeft { get; private set; }

        public GroundDetector(Transform feet, MovementConfig cfg, LayerMask groundLayer, Transform ceiling = null)
        {
            this.feet = feet;
            this.cfg = cfg;
            this.groundLayer = groundLayer;
            this.ceiling = ceiling;
        }

        public void UpdateGround()
        {
            bool wasGrounded = IsGrounded;
            IsGrounded = false;
            CeilingBlocked = false;
            GroundNormal = Vector2.up;

            if (feet != null)
            {
                // Используем CircleCast строго вниз с небольшим радиусом,
                // чтобы захватывать поверхность под ногами, но не стены.
                float castDistance = GroundRadius + 0.05f;
                RaycastHit2D hit = Physics2D.CircleCast(
                    feet.position,
                    GroundRadius,
                    Vector2.down,
                    castDistance,
                    groundLayer
                );

                if (hit.collider != null && hit.collider.gameObject != feet.root.gameObject)
                {
                    // Проверяем, что нормаль направлена достаточно вертикально (угол < 45°)
                    if (Vector2.Angle(hit.normal, Vector2.up) < 45f)
                    {
                        IsGrounded = true;
                        GroundNormal = hit.normal;
                    }
                }
            }

            // Проверка потолка (оставляем как есть)
            if (ceiling != null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(ceiling.position, CeilingRadius, groundLayer);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].gameObject != ceiling.root.gameObject)
                    {
                        CeilingBlocked = true;
                        break;
                    }
                }
            }

            // Логика CoyoteTime и JustLanded
            if (IsGrounded)
                CoyoteTimeLeft = cfg.coyoteTime;
            else if (wasGrounded)
                CoyoteTimeLeft = cfg.coyoteTime;
            else
                CoyoteTimeLeft = Mathf.Max(0f, CoyoteTimeLeft - Time.fixedDeltaTime);

            JustLanded = IsGrounded && !wasGrounded;
        }

        public void ConsumeCoyote()
        {
            CoyoteTimeLeft = 0f;
        }
    }
}