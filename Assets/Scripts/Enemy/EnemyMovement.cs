using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private float speed = 40f;
    [SerializeField] private float detectionRange = 120f;
    [SerializeField] private float stopDistance = 14f;

    private Transform player;
    
    private Vector2 lastPosition;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        lastPosition = transform.position;
    }
    
    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && distanceToPlayer > stopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

            if (direction.x > 0)
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
        }
        
        float currentSpeed = ((Vector2)transform.position - lastPosition).magnitude / Time.deltaTime;
        animator.SetFloat("Speed", currentSpeed);

        lastPosition = transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
