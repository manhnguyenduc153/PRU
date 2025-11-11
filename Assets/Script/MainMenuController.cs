using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject instructionPanel;
    public CanvasGroup instructionGroup;

    [Header("Resume Game")]
    public Button resumeButton; // Nút Resume (BẮT BUỘC)
    public TMPro.TextMeshProUGUI resumeButtonText; // Text của nút Resume (TÙY CHỌN - để hiển thị "Continue" hoặc "New Game")

    void Start()
    {
        // Đảm bảo SaveSystem tồn tại
        if (SaveSystem.Instance == null)
        {
            GameObject saveSystemObj = new GameObject("SaveSystem");
            saveSystemObj.AddComponent<SaveSystem>();
            Debug.Log("SaveSystem created in MainMenu");
        }

        // Khởi tạo instruction panel ở trạng thái ẩn
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true); // Panel phải active để CanvasGroup hoạt động
        }

        if (instructionGroup != null)
        {
            instructionGroup.alpha = 0;
            instructionGroup.interactable = false;
            instructionGroup.blocksRaycasts = false;
        }

        // Kiểm tra có save file không để enable/disable nút Resume
        UpdateResumeButton();
    }

    void UpdateResumeButton()
    {
        if (SaveSystem.Instance == null)
        {
            Debug.LogWarning("SaveSystem.Instance is null in UpdateResumeButton!");
            return;
        }

        bool hasSave = SaveSystem.Instance.HasSaveFile();

        Debug.Log($"[MainMenu] Checking save file... Has save: {hasSave}");

        if (resumeButton != null)
        {
            // NÚT LUÔN ĐƯỢC ENABLE
            resumeButton.interactable = true;
            resumeButton.gameObject.SetActive(true);

            // Thay đổi text nút tùy theo có save hay không
            if (resumeButtonText != null)
            {
                resumeButtonText.text = hasSave ? "Continue" : "New Game";
                Debug.Log($"[MainMenu] Resume button text set to: {resumeButtonText.text}");
            }

            // Màu sắc bình thường, không làm mờ
            var buttonImage = resumeButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.white;
            }
        }
        else
        {
            Debug.LogError("[MainMenu] Resume button is NULL!");
        }

        Debug.Log($"Resume button state: Always Enabled (Save: {hasSave})");
    }

    public void StartGame()
    {
        // Phát âm thanh click
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayClickSound();
        }

        // Xóa save cũ nếu bắt đầu game mới
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.DeleteSaveFile();
        }

        // Reset buffs cho new game
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.ResetAllBuffs();
        }

        SceneManager.LoadScene("StoryScene");
    }
    public void ResumeGame()
    {
        // Phát âm thanh click
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayClickSound();
        }

        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile())
        {
            // Có save file → Load game
            Debug.Log("Save file found! Resuming game from save...");
            SaveSystem.Instance.LoadGame();
        }
        else
        {
            // Không có save file → Bắt đầu game mới
            Debug.Log("No save file found! Starting new game...");
            StartGame();
        }
    }

    // public void ShowInstructions()
    // {
    //     instructionPanel.SetActive(true);
    // }

    // public void HideInstructions()
    // {
    //     instructionPanel.SetActive(false);
    // }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game exited."); // chỉ thấy khi chạy trong editor
    }

    public void ShowInstructions()
    {
        // Phát âm thanh mở panel
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayPanelOpenSound();
        }

        instructionGroup.alpha = 1;
        instructionGroup.interactable = true;
        instructionGroup.blocksRaycasts = true;
    }

    public void HideInstructions()
    {
        // Phát âm thanh đóng panel
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayPanelCloseSound();
        }

        instructionGroup.alpha = 0;
        instructionGroup.interactable = false;
        instructionGroup.blocksRaycasts = false;
    }
}
