using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator camAnimator;

    [Header("Health")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Stamina")]
    [HideInInspector] public float maxStamina = 100f;
    [HideInInspector] public float currentStamina;
    private float decreaseRate = 30f;
    private float increaseRate = 15f;
    private bool isRunning = false;
    private bool canRun = true;
    private float staminaRecoveryThreshold = 20f;

    [Header("Menus")] 
    [SerializeField] private PauseMenu pauseMenu;
    
    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.Pause();
        }

        if (isRunning && canRun)
        {
            currentStamina -= decreaseRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
            playerAnimator.SetBool("Running", true);

            if (currentStamina == 0)
            {
                StartCoroutine(RecoverStamina());
            }
        }
        else
        {
            currentStamina += increaseRate * Time.deltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
            playerAnimator.SetBool("Running", false);
        }
    }

    public bool CanRun()
    {
        return canRun && currentStamina > 0;
    }

    public void SetRunning(bool running)
    {
        isRunning = running && CanRun();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        playerAnimator.SetTrigger("Hit");
        camAnimator.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }

    private IEnumerator RecoverStamina()
    {
        canRun = false;
        yield return new WaitUntil(() => currentStamina >= staminaRecoveryThreshold);
        canRun = true;
    }
}
