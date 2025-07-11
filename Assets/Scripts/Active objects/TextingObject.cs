using UnityEngine;

public class TextingObject : MonoBehaviour
{
    [SerializeField] private DisplayText textDisplayer;
    [SerializeField] private string text;
    [SerializeField] private float holdAfterExit = 2f;
    [SerializeField] private bool showText = true;

    private bool hasTriggered = false;
    private bool playerInside = false;
    private bool waitingToClear = false;
    private bool clearScheduled = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered && showText)
        {
            playerInside = true;

            textDisplayer.StartPrinting(text, () =>
            {
                hasTriggered = true;

                if (!playerInside && waitingToClear)
                {
                    ScheduleClear();
                    waitingToClear = false;
                }
            });
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (!hasTriggered)
            {
                waitingToClear = true;
            }
            else if (!clearScheduled && !textDisplayer.IsPrinting)
            {
                ScheduleClear();
            }
        }
    }

    private void ScheduleClear()
    {
        clearScheduled = true;
        Invoke(nameof(ClearAfterDelay), holdAfterExit);
    }

    public void ClearAfterDelay()
    {
        if (!textDisplayer.IsPrinting || textDisplayer.allowClearDuringPrint)
        {
            textDisplayer.ClearText();
            clearScheduled = false;
        }
    }

    public void ChangeTextingStatus(bool mode)
    {
        showText = mode;
    }
}