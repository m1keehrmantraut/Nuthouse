using System.Collections;
using Nuthouse.Combat;
using Nuthouse.Core.Events;
using Nuthouse.Health;
using UnityEngine;

namespace Nuthouse.Enemies
{
    public class EnemyHealth : DamageReceiver
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Animator _bloodController;
        [SerializeField] private float fadeDuration = 2.5f;
        [SerializeField] private float delayBeforeFade = 1.5f;

        [HideInInspector] public bool isDead = false;
        private bool deathAnimPlayed = false;
        private SpriteRenderer _spriteRenderer;

        protected override void Awake()
        {
            currentHealth = config != null ? config.maxHealth : 100f;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public override void TakeDamage(in DamageInfo info)
        {
            if (isDead) return;

            currentHealth -= info.Amount;
            if (_bloodController != null) _bloodController.SetTrigger("Hit");

            if (currentHealth <= 0f)
                Die();
        }

        protected override void Die()
        {
            if (deathAnimPlayed) return;

            isDead = true;
            deathAnimPlayed = true;
            if (_animator != null)
            {
                _animator.SetBool("IsDead", true);
                _animator.SetTrigger("Death");
            }

            var melee = GetComponent<MeleeEnemy>();
            if (melee != null) melee.enabled = false;
            var move = GetComponent<EnemyMovement>();
            if (move != null) move.enabled = false;

            DamageEvents.PublishDied(gameObject);

            StartCoroutine(FreezeLastFrameAndFadeOut());
        }

        private IEnumerator FreezeLastFrameAndFadeOut()
        {
            float deathAnimTime = GetAnimationLength("EnemyDeath");
            yield return new WaitForSeconds(deathAnimTime);

            if (_animator != null) _animator.enabled = false;

            yield return new WaitForSeconds(delayBeforeFade);

            if (_spriteRenderer == null) yield break;

            float elapsedTime = 0f;
            Color originalColor = _spriteRenderer.color;

            while (elapsedTime < fadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                _spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }

        private float GetAnimationLength(string animationName)
        {
            if (_animator == null || _animator.runtimeAnimatorController == null) return 1f;
            AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name == animationName) return clip.length;
            }
            return 1f;
        }
    }
}