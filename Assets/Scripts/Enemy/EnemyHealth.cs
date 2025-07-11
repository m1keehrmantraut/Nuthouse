using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float healthEnemy = 100f;
    [SerializeField] private Animator _animator;
    [SerializeField] private Animator _bloodController;
    [SerializeField] private float fadeDuration = 2.5f; 
    [SerializeField] private float delayBeforeFade = 1.5f; 

    private SpriteRenderer _spriteRenderer;
    [HideInInspector] public bool isDead = false;
    private bool deathAnimPlayed = false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>(); 
    }

    public void TakeDamageEnemy(float damage)
    {
        if (isDead) return;

        healthEnemy -= damage;
        _bloodController.SetTrigger("Hit");

        if (healthEnemy <= 0)
        {
            DieEnemy();
        }
    }
    
    void DieEnemy()
    {
        if (deathAnimPlayed) return;

        isDead = true;
        deathAnimPlayed = true;
        _animator.SetBool("IsDead", true);
        GetComponent<MeleeEnemy>().enabled = false;  
        GetComponent<EnemyMovement>().enabled = false;  

        _animator.SetTrigger("Death");

        StartCoroutine(FreezeLastFrameAndFadeOut());
    }

    private IEnumerator FreezeLastFrameAndFadeOut()
    {
        float deathAnimTime = GetAnimationLength("EnemyDeath");
        yield return new WaitForSeconds(deathAnimTime); 

        _animator.enabled = false;

        yield return new WaitForSeconds(delayBeforeFade); 

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
        AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == animationName) return clip.length;
        }
        Debug.LogError("Animation clip not found: " + animationName);
        return 1f;
    }
}
