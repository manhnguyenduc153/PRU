using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public GameObject instructionPanel;
    public CanvasGroup instructionGroup;

    [Header("Resume Game")]
    public Button resumeButton; // Nút Resume (BẮT BUỘC)
    public GameObject noSaveText; // Text hiển thị "No save file" (TÙY CHỌN - có thể để trống)

    void Start()
    {
        // Đảm bảo SaveSystem tồn tại
        if (SaveSystem.Instance == null)
        {
            GameObject saveSystemObj = new GameObject("SaveSystem");
            saveSystemObj.AddComponent<SaveSystem>();
            Debug.Log("SaveSystem created in MainMenu");
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
        Debug.Log($"[MainMenu] SaveSystem path: {SaveSystem.Instance != null}");

        if (resumeButton != null)
        {
            resumeButton.interactable = hasSave;
            resumeButton.gameObject.SetActive(true); // Đảm bảo button được hiển thị

            // Có thể làm mờ nút nếu không có save
            var buttonImage = resumeButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                Color targetColor = hasSave ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                buttonImage.color = targetColor;
                Debug.Log($"[MainMenu] Resume button color set to: {targetColor}");
            }
        }
        else
        {
            Debug.LogError("[MainMenu] Resume button is NULL!");
        }

        if (noSaveText != null)
        {
            noSaveText.SetActive(!hasSave);
        }

        Debug.Log($"Resume button state: {(hasSave ? "Enabled" : "Disabled")}");
    }

    public void StartGame()
    {
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
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile())
        {
            Debug.Log("Resuming game from save...");
            SaveSystem.Instance.LoadGame();
        }
        else
        {
            Debug.LogWarning("No save file found!");
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
        instructionGroup.alpha = 1;
        instructionGroup.interactable = true;
        instructionGroup.blocksRaycasts = true;
    }

    public void HideInstructions()
    {
        instructionGroup.alpha = 0;
        instructionGroup.interactable = false;
        instructionGroup.blocksRaycasts = false;
    }
}
