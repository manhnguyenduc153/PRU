using UnityEngine;
using System.Collections;

public class LevelUpEffect : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private GameObject levelUpVFXPrefab; // Prefab hiệu ứng particles
    [SerializeField] private Vector3 vfxOffset = Vector3.zero; // Offset vị trí VFX

    [Header("Screen Flash")]
    [SerializeField] private bool useScreenFlash = true;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private Color flashColor = Color.yellow;
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private int flashCount = 3;

    [Header("Scale Pulse")]
    [SerializeField] private bool useScalePulse = true;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private float pulseDuration = 0.3f;

    [Header("Text Popup")]
    [SerializeField] private GameObject levelUpTextPrefab; // Prefab "LEVEL UP!" text
    [SerializeField] private Vector3 textOffset = new Vector3(0, 2, 0);

    [Header("Camera Shake")]
    [SerializeField] private bool useCameraShake = true;
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeDuration = 0.3f;

    private PlayerExperience playerExperience;
    private Color originalColor;

    private void Start()
    {
        playerExperience = GetComponent<PlayerExperience>();

        if (playerExperience == null)
        {
            Debug.LogError("PlayerExperience not found on " + gameObject.name);
            return;
        }

        // Lấy sprite renderer nếu chưa có
        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (playerSpriteRenderer != null)
        {
            originalColor = playerSpriteRenderer.color;
        }

        // Delay đăng ký event để tránh trigger lúc khởi tạo
        StartCoroutine(DelayedEventRegistration());
    }

    private IEnumerator DelayedEventRegistration()
    {
        yield return null; // Đợi 1 frame
        
        // Đăng ký event SAU KHI khởi tạo xong
        if (playerExperience != null)
        {
            playerExperience.OnLevelUp += PlayLevelUpEffect;
        }
    }

    private void OnDestroy()
    {
        if (playerExperience != null)
        {
            playerExperience.OnLevelUp -= PlayLevelUpEffect;
        }
    }

    private void PlayLevelUpEffect(int newLevel)
    {
        Debug.Log($"🎆 Playing Level Up Effect for Level {newLevel}");

        // 1. Spawn VFX
        if (levelUpVFXPrefab != null)
        {
            GameObject vfx = Instantiate(levelUpVFXPrefab, transform.position + vfxOffset, Quaternion.identity);
            Destroy(vfx, 3f); // Tự động xóa sau 3 giây
        }

        // 2. Flash Effect
        if (useScreenFlash && playerSpriteRenderer != null)
        {
            StartCoroutine(FlashCoroutine());
        }

        // 3. Scale Pulse
        if (useScalePulse)
        {
            StartCoroutine(ScalePulseCoroutine());
        }

        // 4. Text Popup
        if (levelUpTextPrefab != null)
        {
            GameObject textObj = Instantiate(levelUpTextPrefab, transform.position + textOffset, Quaternion.identity);
            Destroy(textObj, 2f);
        }

        // 5. Camera Shake
        if (useCameraShake)
        {
            StartCoroutine(CameraShakeCoroutine());
        }
    }

    private IEnumerator FlashCoroutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            playerSpriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));
            
            playerSpriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration / (flashCount * 2));
        }
    }

    private IEnumerator ScalePulseCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * pulseScale;

        float elapsed = 0f;
        float halfDuration = pulseDuration / 2f;

        // Scale up
        while (elapsed < halfDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        // Scale down
        while (elapsed < halfDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator CameraShakeCoroutine()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        Vector3 originalPos = mainCam.transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity, shakeIntensity);

            mainCam.transform.position = originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.transform.position = originalPos;
    }

    // Test trong editor
    [ContextMenu("Test Level Up Effect")]
    private void TestEffect()
    {
        PlayLevelUpEffect(99);
    }
}

