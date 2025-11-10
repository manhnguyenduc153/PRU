using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class CutsceneBackstory : MonoBehaviour
{
    [Header("UI References")]
    public GameObject cutsceneCanvas;
    public Image backgroundImage;
    public TextMeshProUGUI storyText;
    public float textDelay = 0.05f;
    public float fadeDuration = 1f;
    public KeyCode skipKey = KeyCode.Space;

    [Header("Cutscene Data")]
    public Sprite[] backgrounds;
    public string[] storyLines;

    [Header("Target Enemy")]
    public GameObject targetEnemy;

    [Header("Cutscene Music")]
    public AudioClip cutsceneMusic;
    public float musicFadeDuration = 1f;

    private bool isPlaying = false;

    public void PlayCutscene()
    {
        if (!isPlaying && storyLines.Length == backgrounds.Length)
        {
            StartCoroutine(ShowCutscene());
        }
        else
        {
            Debug.LogWarning("CutsceneBackstory: Số lượng storyLines và backgrounds phải bằng nhau!");
        }
    }

    private IEnumerator ShowCutscene()
    {
        isPlaying = true;

        // Bật Canvas
        cutsceneCanvas.SetActive(true);

        // Dừng game
        Time.timeScale = 0f;

        // ✅ QUAN TRỌNG: Phát nhạc cutscene NGAY LẬP TỨC
        if (AudioManager.Instance != null && cutsceneMusic != null)
        {
            AudioManager.Instance.PlayCutsceneMusic(cutsceneMusic, musicFadeDuration);
        }

        // Chờ một chút để nhạc bắt đầu fade
        yield return new WaitForSecondsRealtime(0.1f);

        for (int i = 0; i < storyLines.Length; i++)
        {
            backgroundImage.sprite = backgrounds[i];
            yield return StartCoroutine(FadeInBackgroundAndText(storyLines[i]));

            float timer = 0f;
            while (!Input.GetKeyDown(skipKey) && timer < 5f)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return StartCoroutine(FadeOutBackgroundAndText());
        }

        cutsceneCanvas.SetActive(false);
        Time.timeScale = 1f;

        // Destroy enemy nếu cần
        if (targetEnemy != null)
            Destroy(targetEnemy);

        // ✅ Kết thúc cutscene, quay lại nhạc bình thường
        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAfterCutscene();

        isPlaying = false;
    }

    private IEnumerator FadeInBackgroundAndText(string line)
    {
        storyText.text = "";
        storyText.alpha = 0f;
        backgroundImage.canvasRenderer.SetAlpha(0f);
        backgroundImage.enabled = true;
        backgroundImage.CrossFadeAlpha(1f, fadeDuration, true);

        foreach (char c in line)
        {
            storyText.text += c;
            yield return new WaitForSecondsRealtime(textDelay);
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            storyText.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        storyText.alpha = 1f;
    }

    private IEnumerator FadeOutBackgroundAndText()
    {
        float timer = 0f;
        float startAlphaText = storyText.alpha;
        float startAlphaBg = backgroundImage.canvasRenderer.GetAlpha();

        while (timer < fadeDuration)
        {
            storyText.alpha = Mathf.Lerp(startAlphaText, 0f, timer / fadeDuration);
            backgroundImage.canvasRenderer.SetAlpha(Mathf.Lerp(startAlphaBg, 0f, timer / fadeDuration));
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        storyText.alpha = 0f;
        backgroundImage.canvasRenderer.SetAlpha(0f);
    }
}