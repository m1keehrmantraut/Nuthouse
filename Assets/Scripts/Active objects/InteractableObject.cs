using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private bool isActive;
    [SerializeField] private UnityEvent interactEvents;

    [SerializeField] private GameObject info;
    [SerializeField] private Transform infoPosition;

    [SerializeField] private Material outline;
    [SerializeField] private GameObject outlineSprite;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip soundEffect;

    private GameObject tempInfo;
    private bool isNear = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isNear)
        {
            InteractActions();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            isNear = true;

            if (info != null)
                tempInfo = Instantiate(info, infoPosition);

            if (outline != null && outlineSprite == null)
                outline.SetFloat("_OutlineEnabled", 1f);
            else if (outlineSprite != null)
                outlineSprite.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            isNear = false;

            if (tempInfo != null)
                Destroy(tempInfo);

            if (outline != null && outlineSprite == null)
                outline.SetFloat("_OutlineEnabled", 0f);
            else if (outlineSprite != null)
                outlineSprite.SetActive(false);
        }
    }

    private void InteractActions()
    {
        interactEvents.Invoke();

        if (soundEffect != null)
            sfxSource.PlayOneShot(soundEffect);

        if (tempInfo != null)
            Destroy(tempInfo);

        isNear = false;
        isActive = false;

        if (outline != null && outlineSprite == null)
            outline.SetFloat("_OutlineEnabled", 0f);
        else if (outlineSprite != null)
            outlineSprite.SetActive(false);
    }

    public void ChangeInteractStatus(bool mode)
    {
        isActive = mode;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
