using System;
using UnityEngine;

namespace Nuthouse.Health
{
    public sealed class Stamina
    {
        private readonly StaminaConfig cfg;
        private float current;
        private bool sprinting;
        private bool inRecovery;
        private float recoveryTimer;

        public event Action<bool> SprintBlockedChanged;

        public Stamina(StaminaConfig config)
        {
            cfg = config;
            current = cfg != null ? cfg.maxStamina : 100f;
        }

        public bool CanSprint => !inRecovery && current > 0f;

        public float Normalized => cfg != null && cfg.maxStamina > 0f
            ? Mathf.Clamp01(current / cfg.maxStamina)
            : 0f;

        public void SetSprinting(bool active)
        {
            sprinting = active && CanSprint;
        }

        public void Tick(float dt)
        {
            if (cfg == null) return;

            if (sprinting && CanSprint)
            {
                current -= cfg.decreaseRate * dt;
                current = Mathf.Max(0f, current);
                if (current <= 0f)
                {
                    sprinting = false;
                    inRecovery = true;
                    recoveryTimer = 0f;
                    SprintBlockedChanged?.Invoke(false);
                }
            }
            else
            {
                current += cfg.increaseRate * dt;
                current = Mathf.Min(cfg.maxStamina, current);

                if (inRecovery)
                {
                    recoveryTimer += dt;
                    if (current >= cfg.recoveryThreshold)
                    {
                        inRecovery = false;
                        SprintBlockedChanged?.Invoke(true);
                    }
                }
            }
        }
    }
}