using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class CanvasFadeTransition : MonoBehaviour
{
    public static CanvasFadeTransition Instance { get; private set; }

    [Header("Fade Settings")]
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;

    [Header("References")]
    public Image fadeImage; // Gắn Image vào đây (FadeImage)
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeImage == null)
            {
                fadeImage = GetComponentInChildren<Image>();
            }

            canvasGroup = fadeImage.GetComponentInParent<CanvasGroup>();
            fadeImage.color = fadeColor;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName, 0f));
    }

    public void LoadSceneWithDelay(string sceneName, float delay)
    {
        StartCoroutine(FadeAndLoadScene(sceneName, delay));
    }

    private IEnumerator FadeAndLoadScene(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Fade out
        yield return StartCoroutine(Fade(0f, 1f));

        SceneManager.LoadScene(sceneName);

        // Fade in
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    public void FadeIn() => StartCoroutine(Fade(1f, 0f));
    public void FadeOut() => StartCoroutine(Fade(0f, 1f));
}
