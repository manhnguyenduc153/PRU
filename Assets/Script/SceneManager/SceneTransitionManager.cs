using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float blackScreenDuration = 0.5f; // Thời gian giữ màn hình đen
    [SerializeField] private Color fadeColor = Color.black;

    private bool isFading = false;
    private bool isFirstSceneLoad = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupFadeCanvas();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupFadeCanvas()
    {
        if (fadeImage == null)
        {
            // ✅ Tạo Canvas tự động nếu chưa có
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767; // Luôn trên cùng

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            canvasObj.AddComponent<GraphicRaycaster>();

            // ✅ Tạo Image để fade
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(canvasObj.transform);

            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = fadeColor;
            fadeImage.raycastTarget = false;

            RectTransform rect = fadeImage.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            // ✅ Đảm bảo phủ kín toàn màn hình (kể cả safe area)
            rect.offsetMin = new Vector2(-200, -200);
            rect.offsetMax = new Vector2(200, 200);

            // ✅ Buộc Unity cập nhật layout ngay
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            // ✅ Đảm bảo vẫn phủ kín sau 1 frame
            StartCoroutine(EnsureFullScreenFade(rect));
        }

        // Đảm bảo Canvas luôn ở trên cùng
        Canvas fadeCanvas = fadeImage.GetComponentInParent<Canvas>();
        if (fadeCanvas != null)
        {
            fadeCanvas.sortingOrder = 32767;
            fadeCanvas.overrideSorting = true;
        }

        // Bắt đầu với màn hình đen hoàn toàn cho scene đầu tiên
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
    }

    private IEnumerator EnsureFullScreenFade(RectTransform rect)
    {
        yield return null; // đợi 1 frame để canvas update kích thước
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(-200, -200);
        rect.offsetMax = new Vector2(200, 200);
        Canvas.ForceUpdateCanvases();
    }

    private void Start()
    {
        if (isFirstSceneLoad)
        {
            StartCoroutine(FadeInFirstScene());
            isFirstSceneLoad = false;
        }
    }

    public void TransitionToScene(string sceneName, bool useAsync = true)
    {
        Debug.Log($"[SceneTransition] TransitionToScene called: {sceneName}, isFading: {isFading}");
        if (!isFading)
        {
            StartCoroutine(TransitionCoroutine(sceneName, useAsync));
        }
        else
        {
            Debug.LogWarning("[SceneTransition] Already fading, ignoring request");
        }
    }

    private IEnumerator TransitionCoroutine(string sceneName, bool useAsync)
    {
        Debug.Log("[SceneTransition] Starting transition coroutine");
        isFading = true;

        Canvas fadeCanvas = fadeImage.GetComponentInParent<Canvas>();
        if (fadeCanvas != null)
        {
            fadeCanvas.sortingOrder = 32767;
        }

        // 1. Fade out
        Debug.Log("[SceneTransition] Starting fade out");
        yield return StartCoroutine(FadeOut());

        // 2. Giữ màn hình đen
        Debug.Log("[SceneTransition] Black screen hold");
        yield return new WaitForSeconds(blackScreenDuration);

        // 3. Load scene mới
        Debug.Log($"[SceneTransition] Loading scene: {sceneName}");
        if (GameManager.Instance != null)
        {
            if (useAsync)
            {
                yield return StartCoroutine(LoadSceneAsyncWithFade(sceneName));
            }
            else
            {
                GameManager.Instance.LoadScene(sceneName);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            Debug.LogError("GameManager not found!");
        }

        // 4. Giữ màn đen thêm chút
        yield return new WaitForSeconds(blackScreenDuration);

        // 5. Fade in
        Debug.Log("[SceneTransition] Starting fade in");
        yield return StartCoroutine(FadeIn());

        Debug.Log("[SceneTransition] Transition complete");
        isFading = false;
    }

    private IEnumerator LoadSceneAsyncWithFade(string sceneName)
    {
        var asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color c = fadeImage.color;
        float startAlpha = c.a;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
    }

    private IEnumerator FadeInFirstScene()
    {
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FadeIn());
    }

    public IEnumerator FadeOutManual()
    {
        yield return StartCoroutine(FadeOut());
    }

    public IEnumerator FadeInManual()
    {
        yield return StartCoroutine(FadeIn());
    }
}
