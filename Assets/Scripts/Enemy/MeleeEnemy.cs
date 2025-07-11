using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour
{
    private PlayerHealth player;

    [HideInInspector] public bool attackStatus = true;
    private float attackDamage = 10f;
    private float timeBtwAttacks = 1.6f;

    [SerializeField] private Animator _animator;

    private EnemyHealth _enemy;

    private int kickCounter = 0;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        _enemy = gameObject.GetComponent<EnemyHealth>();
    }
    
    IEnumerator AttackTime(float count)
    {
        attackStatus = false;
        yield return new WaitForSeconds(count);
        _animator.SetTrigger("Attack1");
        attackStatus = true;
    }

    IEnumerator HitTime()
    {
        float time = 0.1f;
        if (kickCounter == 2 && !_enemy.isDead)
        {
            time = 0.3f;
        }
        else
        {
            if (!_enemy.isDead)
            {
                time = 0.1f;
            }
        }
        
        if (!_enemy.isDead)
        {
            yield return new WaitForSeconds(time);
            player.TakeDamage(attackDamage);    
        }
    }
        
    private void Update()
    {
        float distance = Mathf.Abs(player.transform.position.x - gameObject.transform.position.x);
        if (distance < 14f && !_enemy.isDead)
        {
            if (attackStatus && !_enemy.isDead)
            {
                StartCoroutine(HitTime());

                if (kickCounter == 2)
                {
                    _animator.SetTrigger("Attack2");
                    kickCounter = 0;
                }
                else
                {
                    _animator.SetTrigger("Attack1");
                    kickCounter++;
                }
                
            
                StartCoroutine(AttackTime(timeBtwAttacks));
            }
        }
    }

}
