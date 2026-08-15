using System.Collections;
using UnityEngine;
using Nuthouse.Player;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private PlayerFacade playerFacade;
    [SerializeField] private RectTransform staminaLeft;
    [SerializeField] private RectTransform staminaRight;
    [SerializeField] private float maxWidth = 100f;
    [SerializeField] private CanvasGroup staminaUI;

    private bool isVisible = false;

    private void Awake()
    {
        if (playerFacade == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) playerFacade = go.GetComponent<PlayerFacade>();
        }
    }

    private void Update()
    {
        float fillAmount = playerFacade != null ? playerFacade.StaminaNormalized : 0f;
        float newWidth = maxWidth * fillAmount;

        staminaLeft.sizeDelta = new Vector2(newWidth, staminaLeft.sizeDelta.y);
        staminaRight.sizeDelta = new Vector2(newWidth, staminaRight.sizeDelta.y);

        HandleStaminaUI(fillAmount);
    }

    private void HandleStaminaUI(float fillAmount)
    {
        bool shouldBeVisible = fillAmount < 1;

        if (shouldBeVisible != isVisible)
        {
            isVisible = shouldBeVisible;
            StartCoroutine(FadeStaminaUI(isVisible ? 1 : 0));
        }
    }

    private IEnumerator FadeStaminaUI(float targetAlpha)
    {
        float duration = 0.5f;
        float startAlpha = staminaUI.alpha;
        float time = 0;

        while (time < duration)
        {
            staminaUI.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        staminaUI.alpha = targetAlpha;
    }
}