using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DisplayText : MonoBehaviour
{
    private TMP_Text textField;
    public float fadeDuration = 0.5f;
    public float typingSpeed = 0.03f;
    public float glitchDuration = 0.05f;

    [Tooltip("Разрешить ClearText() во время печати текста")]
    public bool allowClearDuringPrint = false;

    private string randomChars = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя0123456789!\"#$%&'()*+,-./:;<=>?@[]^_`{|}~";

    private Coroutine printCoroutine;
    private Action onPrintComplete;

    public bool IsPrinting { get; private set; } = false;

    private void Start()
    {
        textField = GetComponent<TMP_Text>();
        textField.alpha = 0;
    }

    public void StartPrinting(string text, Action onComplete)
    {
        if (printCoroutine != null)
        {
            StopCoroutine(printCoroutine);
            printCoroutine = null;
        }

        textField.text = "";
        textField.alpha = 1;
        onPrintComplete = onComplete;
        IsPrinting = true;
        printCoroutine = StartCoroutine(TypeAndGlitchText(text));
    }

    private IEnumerator TypeAndGlitchText(string sentence)
    {
        for (int i = 0; i < sentence.Length; i++)
        {
            string randomChar = GetRandomCharacter();
            textField.text = sentence.Substring(0, i) + randomChar;
            yield return new WaitForSeconds(glitchDuration);

            textField.text = sentence.Substring(0, i + 1);
            yield return new WaitForSeconds(typingSpeed);
        }

        textField.text = sentence;
        printCoroutine = null;
        IsPrinting = false;
        onPrintComplete?.Invoke();
    }

    private string GetRandomCharacter()
    {
        int index = UnityEngine.Random.Range(0, randomChars.Length);
        return randomChars[index].ToString();
    }

    public void ClearText()
    {
        if (IsPrinting && !allowClearDuringPrint)
            return;

        if (printCoroutine != null)
        {
            StopCoroutine(printCoroutine);
            printCoroutine = null;
        }

        IsPrinting = false;
        StartCoroutine(FadeTextAndClear());
    }

    private IEnumerator FadeTextAndClear()
    {
        yield return StartCoroutine(FadeText(1, 0));
        textField.text = "";
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            textField.alpha = alpha;
            yield return null;
        }

        textField.alpha = endAlpha;
    }
}
