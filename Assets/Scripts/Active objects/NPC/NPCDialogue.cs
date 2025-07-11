using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class NPCDialogue : MonoBehaviour
{
    [SerializeField] private UnityEvent onDialogueStarted;
    [SerializeField] private UnityEvent onDialogueFinished;
    
    [SerializeField] private DisplayText displayText;
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private float fadeDuration = 2f;

    private int currentLine = 0;
    private bool isPlayerInside = false;
    private bool isDialogueActive = false;
    private bool isPrinting = false;
    private bool isEnding = false;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isDialogueActive && !isEnding)
        {
            isPlayerInside = true;
            StartDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            EndDialogue();
        }
    }

    private void Update()
    {
        if (isDialogueActive && isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            if (!isPrinting)
                ShowNextLine();
        }
    }

    private void StartDialogue()
    {
        onDialogueStarted.Invoke();
        isDialogueActive = true;
        currentLine = 0;
        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue();
            isEnding = true;
            StartCoroutine(FadeAndDestroy());
            return;
        }

        isPrinting = true;
        displayText.StartPrinting(dialogueLines[currentLine], () =>
        {
            isPrinting = false;
        });

        currentLine++;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        isPrinting = false;
        displayText.ClearText();
    }

    private IEnumerator FadeAndDestroy()
    {
        onDialogueFinished.Invoke();
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        Destroy(gameObject);
    }
}
