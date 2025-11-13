using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingStoryController : MonoBehaviour
{
    public TextMeshProUGUI storyText;
    public Button nextButton;
    public Button previousButton;
    public Button skipButton;
    public AudioSource audioSource;
    public AudioClip typingSound;

    [Header("Story Data")]
    [TextArea(3, 10)]
    public string[] storyLines;
    public Sprite[] backgroundImages;   // mảng ảnh nền
    public Image backgroundImageUI;     // tham chiếu đến UI Image nền

    private int currentLine = 0;
    private bool isTyping = false;
    private float typingSpeed = 0.05f;

    void Start()
    {
        nextButton.onClick.AddListener(NextLine);
        previousButton.onClick.AddListener(PreviousLine);
        skipButton.onClick.AddListener(SkipToMenu);
        UpdatePreviousButtonState();
        UpdateBackground();
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        storyText.text = "";
        int counter = 0;

        foreach (char c in storyLines[currentLine])
        {
            storyText.text += c;
            counter++;
            if (typingSound && audioSource && counter % 2 == 0)
                audioSource.PlayOneShot(typingSound, 0.4f);
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void NextLine()
    {
        StopAllCoroutines();

        if (isTyping)
        {
            // Hiển thị full text của câu hiện tại
            storyText.text = storyLines[currentLine];
            isTyping = false;
        }
        else
        {
            currentLine++;
            if (currentLine < storyLines.Length)
            {
                // Còn câu tiếp theo → hiển thị
                UpdateBackground();
                UpdatePreviousButtonState();
                StartCoroutine(TypeLine());
            }
            else
            {
                // Hết câu chuyện → quay về menu chính
                ReturnToMainMenu();
            }
        }
    }

    public void PreviousLine()
    {
        // Không cho quay lại nếu đang ở câu đầu tiên
        if (currentLine <= 0) return;

        StopAllCoroutines();

        // Nếu đang typing, hiển thị full câu hiện tại
        if (isTyping)
        {
            storyText.text = storyLines[currentLine];
            isTyping = false;
        }

        // Quay lại câu trước đó
        currentLine--;
        UpdateBackground();
        UpdatePreviousButtonState();
        StartCoroutine(TypeLine());
    }

    private void UpdatePreviousButtonState()
    {
        // Vô hiệu hóa nút previous nếu đang ở câu đầu tiên
        if (previousButton != null)
        {
            previousButton.interactable = (currentLine > 0);
        }
    }

    public void SkipToMenu()
    {
        // Nút skip → quay về menu luôn
        ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {
        Debug.Log("[EndingStory] Story completed! Returning to Main Menu...");

        // Xóa save game vì đã hoàn thành
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.DeleteSaveFile();
            Debug.Log("[EndingStory] Save file deleted - game completed!");
        }

        // Reset tất cả DontDestroyOnLoad objects
        CleanupGameObjects();

        // Load về main menu
        SceneManager.LoadScene("StartMenu");
    }

    private void CleanupGameObjects()
    {
        // Destroy tất cả DontDestroyOnLoad objects
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj != null && obj.scene.name == null) // DontDestroyOnLoad objects không có scene name
            {
                Debug.Log($"[EndingStory] Destroying DontDestroyOnLoad object: {obj.name}");
                Destroy(obj);
            }
        }
    }

    private void UpdateBackground()
    {
        if (backgroundImages != null && currentLine < backgroundImages.Length && backgroundImages[currentLine] != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeBackground(backgroundImages[currentLine]));
        }
    }

    IEnumerator FadeBackground(Sprite newSprite)
    {
        // Nếu chưa có ảnh nền ban đầu
        if (backgroundImageUI.sprite == null)
        {
            backgroundImageUI.sprite = newSprite;
            yield break;
        }

        float duration = 0.8f; // thời gian hiệu ứng (giây)
        float elapsed = 0f;

        Color originalColor = backgroundImageUI.color;

        // 1️⃣ Fade out (ẩn nền cũ)
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / (duration / 2));
            backgroundImageUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 2️⃣ Đổi sang ảnh mới
        backgroundImageUI.sprite = newSprite;
        elapsed = 0f;

        // 3️⃣ Fade in (hiện nền mới)
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / (duration / 2));
            backgroundImageUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Đảm bảo alpha = 1
        backgroundImageUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }
}
