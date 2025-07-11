using System.Collections;
using UnityEngine;

public class MeleeCombat : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float attackDamage = 50f;
    [SerializeField] private float attackRange = 0f;
    [SerializeField] private Animator _animator;

    [SerializeField] private bool attackStatus = true;
    
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip missSound;

    [HideInInspector] public bool GameIsPaused = false;

    private PlayerMovement movement;
    private float boost = 0f;
    private float timeBtwAttacks = 0.5f;

    private void Start()
    {
        movement = gameObject.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !GameIsPaused)
        {
            MeleeAttack();
        }
    }

    IEnumerator AttackTime(float timeBtwAttacks)
    {
        attackStatus = false;
        StartCoroutine(movement.StopRun(timeBtwAttacks));
        yield return new WaitForSeconds(timeBtwAttacks);
        attackStatus = true;
    }

    public void CallAttackDellay()
    {
        StartCoroutine(AttackTime(timeBtwAttacks));
    }

    public void MeleeAttack()
    {
        if (!attackStatus) return;

        _animator.SetTrigger("Fight");

        var hitEnemy = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        bool hasHit = false;

        foreach (var enemy in hitEnemy)
        {
            enemy.GetComponent<EnemyHealth>().TakeDamageEnemy(attackDamage + attackDamage * boost);
            hasHit = true;
        }

        if (sfxSource != null)
        {
            sfxSource.pitch = Random.Range(0.65f, 1.35f);

            if (hasHit && hitSound != null)
                sfxSource.PlayOneShot(hitSound);
            else if (!hasHit && missSound != null)
                sfxSource.PlayOneShot(missSound);

            sfxSource.pitch = 1f;
        }

        StartCoroutine(AttackTime(timeBtwAttacks));
    }

    public void IncreaseBoost(float velocity)
    {
        boost += velocity;
    }

    private void OnDrawGizmos()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void EnableMelee()
    {
        attackStatus = true;
    }
}
