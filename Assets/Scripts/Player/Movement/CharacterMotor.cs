using UnityEngine;

namespace Nuthouse.Player.Movement
{
    public sealed class CharacterMotor
    {
        private readonly Rigidbody2D body;
        private readonly MovementConfig move;
        private readonly JumpConfig jump;

        public Vector2 Velocity => body.linearVelocity;
        public float VerticalVelocity => body.linearVelocity.y;

        public CharacterMotor(Rigidbody2D body, MovementConfig move, JumpConfig jump)
        {
            this.body = body;
            this.move = move;
            this.jump = jump;
            body.gravityScale = 0f;
        }

        public void MoveHorizontal(float inputX, float targetSpeed, float accel, float fdt)
{
    float currentVx = body.linearVelocity.x;
    float targetVx = inputX * targetSpeed;
    float acceleration;
    float desiredVx;

    if (Mathf.Approximately(inputX, 0f))
    {
        // Торможение (нет ввода)
        acceleration = move.decel;
        desiredVx = 0f;
    }
    else
    {
        // Если скорость направлена против ввода — применяем decel для быстрой смены направления
        bool isOpposing = Mathf.Sign(currentVx) != Mathf.Sign(inputX) && Mathf.Abs(currentVx) > 0.01f;
        acceleration = isOpposing ? (move.decel + accel / 2) : accel;
        desiredVx = targetVx;
    }

    float newVx = Mathf.MoveTowards(currentVx, desiredVx, acceleration * fdt);
    body.linearVelocity = new Vector2(newVx, body.linearVelocity.y);
}

        public void ApplyGroundFrictionIfNoInput(float inputX, float fdt)
        {
            if (Mathf.Approximately(inputX, 0f))
            {
                float vx = Mathf.MoveTowards(body.linearVelocity.x, 0f, move.decel * fdt);
                body.linearVelocity = new Vector2(vx, body.linearVelocity.y);
            }
        }

        public void Jump(float force)
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, force);
        }

        public void CutJump()
        {
            if (body.linearVelocity.y > 0f)
                body.linearVelocity = new Vector2(body.linearVelocity.x, body.linearVelocity.y * jump.jumpCutMultiplier);
        }

        public void ApplyGravity(float fdt)
        {
            if (jump == null) return;

            float vy = body.linearVelocity.y;
            float g = Physics2D.gravity.y * jump.baseGravityMultiplier;

            float mult;
            if (Mathf.Abs(vy) < jump.apexThreshold) mult = jump.apexGravityMultiplier;
            else if (vy < 0f) mult = jump.fallGravityMultiplier;
            else mult = jump.riseGravityMultiplier;

            vy += g * mult * fdt;
            if (vy < -jump.maxFallSpeed) vy = -jump.maxFallSpeed;
            body.linearVelocity = new Vector2(body.linearVelocity.x, vy);
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            body.linearVelocity += impulse;
        }

        public void Knockback(Vector2 direction, float force)
        {
            body.linearVelocity = direction.normalized * force;
        }

        public void ResetVerticalVelocity()
        {
            body.linearVelocity = new Vector2(body.linearVelocity.x, 0f);
        }
    }
}