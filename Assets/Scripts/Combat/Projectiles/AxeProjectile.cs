using Nuthouse.Combat;
using Nuthouse.Core.Events;
using Nuthouse.World.Interact;
using UnityEngine;

namespace Nuthouse.Combat.Projectiles
{
    public enum AxeState { Flying, Stuck, Grounded }

    public sealed class AxeProjectile : MonoBehaviour
    {
        private AxeThrowDefinition def;
        private Rigidbody2D body;
        private Hitbox hitbox;
        private AxeState state = AxeState.Flying;
        private Transform visual;
        private float spin;

        public AxeState State => state;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponentInChildren<Hitbox>();
            visual = transform.childCount > 0 ? transform.GetChild(0) : null;
        }

        public void Launch(AxeThrowDefinition definition, Vector2 direction, Vector2 ownerVelocity, Transform owner = null)
        {
            def = definition;
            state = AxeState.Flying;

            Vector2 vel = direction.normalized * def.throwForce + Vector2.up * def.upwardBias;
            if (def.inheritOwnerVelocity) vel += ownerVelocity;
            body.linearVelocity = vel;

            if (def.physicalSpin)
            {
                body.freezeRotation = false;
                body.angularVelocity = def.spinSpeed * (direction.x >= 0f ? 1f : -1f);
            }
            else
            {
                body.freezeRotation = true;
                spin = def.spinSpeed * (direction.x >= 0f ? 1f : -1f);
            }

            // Владелец хитбокса — игрок: чтобы Hurtbox игрока НЕ получал урон от своего
            // же топора (IsOwnedBy). Owner-аргумент — отклонение от сигнатуры спеки ради
            // корректности; PlayerCombat передаёт transform фасада (05 §5).
            Transform ownerRoot = owner != null ? owner : transform;
            if (hitbox != null)
            {
                var info = new DamageInfo(def.damage, DamageType.Ranged, direction, def.knockback, owner != null ? owner.gameObject : null);
                hitbox.Setup(info, ownerRoot);
            }

            var interactable = gameObject.GetComponent<AxeInteractable>();
            if (interactable == null) interactable = gameObject.AddComponent<AxeInteractable>();
            interactable.Setup(this);
            interactable.enabled = false;
            enabled = true;
        }

        private void FixedUpdate()
        {
            if (state != AxeState.Flying) return;

            if (!def.physicalSpin && visual != null)
                visual.Rotate(Vector3.forward, spin * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (state != AxeState.Flying) return;
            HandleHit(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (state != AxeState.Flying) return;
            HandleHit(collision.collider);
        }

        private void HandleHit(Collider2D other)
        {
            if (other.TryGetComponent<IDamageable>(out var target))
            {
                if (hitbox == null || !hitbox.TryHit(target)) return;

                target.TakeDamage(hitbox.CurrentDamageInfo);
                CombatEvents.PublishAttackHit(hitbox.CurrentDamageInfo, other.gameObject);

                if (def.stickToEnemy)
                {
                    StickTo(other.transform);
                    CombatEvents.PublishAxeStuck(this, other.gameObject);
                }
                else
                {
                    Bounce();
                }
                return;
            }

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (other.gameObject.layer == groundLayer || ((def.stickToWorld && IsGroundLayer(other))))
            {
                if (def.stickToWorld)
                {
                    StickToWorld();
                    CombatEvents.PublishAxeStuck(this, other.gameObject);
                }
                else
                {
                    Bounce();
                }
                return;
            }

            Bounce();
        }

        private bool IsGroundLayer(Collider2D other)
        {
            return ((1 << other.gameObject.layer) & LayerMask.GetMask("Ground")) != 0;
        }

        private void StickTo(Transform parent)
        {
            state = AxeState.Stuck;
            body.linearVelocity = Vector2.zero;
            body.bodyType = RigidbodyType2D.Kinematic;
            transform.SetParent(parent, true);
            SetHitboxActive(false);
            EnableInteractable();
        }

        private void StickToWorld()
        {
            state = AxeState.Stuck;
            body.linearVelocity = Vector2.zero;
            body.bodyType = RigidbodyType2D.Kinematic;
            SetHitboxActive(false);
            EnableInteractable();
        }

        private void Bounce()
        {
            state = AxeState.Grounded;
            Vector2 v = body.linearVelocity;
            v.y = Mathf.Abs(v.y) * def.bounceOnNoStick;
            v.x *= def.bounceOnNoStick;
            body.linearVelocity = v;
            SetHitboxActive(false);
            EnableInteractable();
        }

        private void SetHitboxActive(bool active)
        {
            if (hitbox != null) hitbox.gameObject.SetActive(active);
        }

        private void EnableInteractable()
        {
            var interactable = GetComponent<AxeInteractable>();
            if (interactable != null) interactable.enabled = true;
        }

        public void PickUp()
        {
            CombatEvents.PublishAxePickedUp();
            if (gameObject != null)
                Destroy(gameObject);
        }
    }
}