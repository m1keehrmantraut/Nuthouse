using System.Collections;
using UnityEngine;
using Nuthouse.Combat;
using Nuthouse.Player;

namespace Nuthouse.Enemies
{
    public class MeleeEnemy : MonoBehaviour
    {
        private PlayerFacade player;

        [HideInInspector] public bool attackStatus = true;
        private float attackDamage = 10f;
        private float knockback = 3f;
        private float timeBtwAttacks = 1.6f;

        [SerializeField] private Animator _animator;

        private EnemyHealth _enemy;
        private int kickCounter = 0;

        void Start()
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null) player = playerGo.GetComponent<PlayerFacade>();
            _enemy = gameObject.GetComponent<EnemyHealth>();
        }

        IEnumerator AttackTime(float count)
        {
            attackStatus = false;
            yield return new WaitForSeconds(count);
            if (_animator != null) _animator.SetTrigger("Attack1");
            attackStatus = true;
        }

        IEnumerator HitTime()
        {
            float time = 0.1f;
            if (kickCounter == 2 && !_enemy.isDead) time = 0.3f;
            else if (!_enemy.isDead) time = 0.1f;

            if (!_enemy.isDead)
            {
                yield return new WaitForSeconds(time);
                if (player != null)
                {
                    Vector2 dir = (player.transform.position - transform.position).normalized;
                    var info = new DamageInfo(attackDamage, DamageType.Physical, dir, knockback, gameObject);
                    player.ApplyDamage(in info);
                }
            }
        }

        private void Update()
        {
            if (player == null) return;
            float distance = Mathf.Abs(player.transform.position.x - gameObject.transform.position.x);
            if (distance < 14f && !_enemy.isDead)
            {
                if (attackStatus && !_enemy.isDead)
                {
                    StartCoroutine(HitTime());

                    if (kickCounter == 2)
                    {
                        if (_animator != null) _animator.SetTrigger("Attack2");
                        kickCounter = 0;
                    }
                    else
                    {
                        if (_animator != null) _animator.SetTrigger("Attack1");
                        kickCounter++;
                    }

                    StartCoroutine(AttackTime(timeBtwAttacks));
                }
            }
        }
    }
}