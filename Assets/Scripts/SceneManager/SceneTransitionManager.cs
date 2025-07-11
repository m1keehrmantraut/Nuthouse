using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateFadeCanvas();
        }
        else Destroy(gameObject);
    }

    private void CreateFadeCanvas()
    {
        GameObject canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform);
        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;

        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(Fade(0, 1));
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(0.1f);
        
        yield return StartCoroutine(Fade(1, 0));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0f;
        Color c = fadeImage.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, endAlpha);
    }
    
    public void CompleteSceneAndLoad(string sceneName)
    {
        if (GameProgress.Instance != null)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            GameProgress.Instance.MarkSceneCompleted(currentScene);
        }

        LoadSceneWithFade(sceneName);
    }

}
