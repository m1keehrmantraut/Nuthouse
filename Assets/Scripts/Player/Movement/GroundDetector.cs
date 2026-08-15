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
                Collider2D[] colliders = Physics2D.OverlapCircleAll(feet.position, GroundRadius, groundLayer);
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].gameObject != feet.root.gameObject)
                    {
                        IsGrounded = true;
                        GroundNormal = Vector2.up;
                    }
                }
            }

            // Потолок: если ceilingCheck задан, проверяем, что над головой стена.
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

            if (IsGrounded)
                CoyoteTimeLeft = cfg.coyoteTime;
            else if (wasGrounded)
                CoyoteTimeLeft = cfg.coyoteTime;
            else
                CoyoteTimeLeft = Mathf.Max(0f, CoyoteTimeLeft - Time.fixedDeltaTime);
        }

        public void ConsumeCoyote()
        {
            CoyoteTimeLeft = 0f;
        }
    }
}
