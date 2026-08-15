using UnityEngine;

namespace Nuthouse.Player.Movement
{
    public sealed class FacingController
    {
        private readonly Transform transform;
        private bool locked;

        public int Direction { get; private set; } = 1;

        public FacingController(Transform transform)
        {
            this.transform = transform;
        }

        public void UpdateByInput(float x)
        {
            if (locked) return;
            if (x > 0.01f) SetDirection(1);
            else if (x < -0.01f) SetDirection(-1);
        }

        public void Lock() => locked = true;
        public void Unlock() => locked = false;

        private void SetDirection(int dir)
        {
            if (Direction == dir) return;
            Direction = dir;
            Vector3 scale = transform.localScale;
            scale.x = dir * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}
