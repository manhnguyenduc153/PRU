using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DarkZoneController : MonoBehaviour
{
    [Header("Dark Zone Settings")]
    [SerializeField, Range(0f, 1f)] private float targetDarkness = 0.6f; // 🔹 Độ tối khi ở trong vùng
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private Color darkColor = Color.black; // 🔹 Màu overlay (có thể chỉnh thành xanh, đỏ...)
    [SerializeField] private Sprite darkOverlaySprite;

    private Image darkOverlay;
    private Coroutine fadeCoroutine;
    private bool isInside = false;

    private void Start()
    {
        // 🔹 Tìm Canvas hiện có
        Canvas canvas = FindObjectOfType<Canvas>();

        // 🔹 Nếu chưa có Canvas, tự tạo
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("DarkZoneCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // đảm bảo nằm trên cùng
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // 🔹 Tạo Image phủ toàn màn hình (nếu chưa có)
        darkOverlay = canvas.GetComponentInChildren<Image>();
        if (darkOverlay == null || darkOverlay.name != "DarkOverlay")
        {
            GameObject overlayGO = new GameObject("DarkOverlay");
            overlayGO.transform.SetParent(canvas.transform, false);

            darkOverlay = overlayGO.AddComponent<Image>();
            darkOverlay.rectTransform.anchorMin = Vector2.zero;
            darkOverlay.rectTransform.anchorMax = Vector2.one;
            darkOverlay.rectTransform.offsetMin = Vector2.zero;
            darkOverlay.rectTransform.offsetMax = Vector2.zero;
        }

        // 🔹 Gắn sprite hoặc chỉ dùng màu
        if (darkOverlaySprite != null)
            darkOverlay.sprite = darkOverlaySprite;

        darkOverlay.color = new Color(darkColor.r, darkColor.g, darkColor.b, 0f);
        darkOverlay.preserveAspect = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isInside)
        {
            isInside = true;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeTo(targetDarkness));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isInside)
        {
            isInside = false;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeTo(0f));
        }
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = darkOverlay.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            // 🔹 Giữ nguyên màu gốc hoặc texture, chỉ thay đổi độ trong suốt
            Color currentColor = darkOverlay.color;
            currentColor.a = alpha;
            darkOverlay.color = currentColor;

            yield return null;
        }

        Color finalColor = darkOverlay.color;
        finalColor.a = targetAlpha;
        darkOverlay.color = finalColor;
    }
}
