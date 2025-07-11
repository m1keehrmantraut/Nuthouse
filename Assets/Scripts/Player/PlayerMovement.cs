using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController2D controller;
    [SerializeField] private PlayerHealth playerStats;
    [SerializeField] private Animator playerAnimator;

    [Header("Movement")]
    [SerializeField] private float defaultSpeed = 300f;
    [SerializeField] private float runSpeed = 500f;

    [Header("Audio")]
    [SerializeField] private AudioSource movementAudioSource;
    [SerializeField] private AudioClip walkLoop;
    [SerializeField] private AudioClip runLoop;

    private float speed;
    private float horizontalMove = 0f;
    private bool crouch = false;
    private bool hit = false;
    private bool isRunning = false;

    private void Start()
    {
        speed = defaultSpeed;
    }

    private void Update()
    {
        if (Mathf.Approximately(Time.timeScale, 1f))
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * speed;
            bool isMoving = horizontalMove != 0;

            if (isMoving && !hit)
            {
                transform.eulerAngles = new Vector3(0, horizontalMove > 0 ? 0 : 180f, 0);
            }

            if (Input.GetButton("Run") && playerStats.CanRun() && isMoving)
            {
                isRunning = true;
                playerAnimator.SetBool("Crouching", false);
                crouch = false;
            }
            else
            {
                isRunning = false;
            }

            speed = isRunning && !hit ? runSpeed : defaultSpeed;
            playerStats.SetRunning(isRunning);

            if (Input.GetButton("Crouch") && !isRunning)
            {
                playerAnimator.SetBool("Crouching", true);
                crouch = true;
            }
            else if (Input.GetButtonUp("Crouch"))
            {
                playerAnimator.SetBool("Crouching", false);
                crouch = false;
            }

            if (hit)
            {
                speed = 0f;
            }

            controller.Move(horizontalMove * Time.fixedDeltaTime, crouch);
            playerAnimator.SetFloat("Speed", Mathf.Abs(horizontalMove * Time.fixedDeltaTime));

            HandleMovementSound(isMoving);
        }
    }

    private void HandleMovementSound(bool isMoving)
    {
        if (isMoving && !hit)
        {
            AudioClip targetClip = isRunning ? runLoop : walkLoop;

            if (movementAudioSource.clip != targetClip)
            {
                movementAudioSource.clip = targetClip;
                movementAudioSource.loop = true;
                movementAudioSource.Play();
            }

            if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.Play();
            }
        }
        else
        {
            if (movementAudioSource.isPlaying)
            {
                movementAudioSource.Stop();
            }
        }
    }

    public IEnumerator StopRun(float time)
    {
        speed = 0f;
        hit = true;
        if (movementAudioSource.isPlaying)
        {
            movementAudioSource.Stop();
        }
        yield return new WaitForSeconds(time);
        speed = defaultSpeed;
        hit = false;
    }
}
