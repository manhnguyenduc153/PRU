using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Menu UI")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button closeButton;
    public Button quitButton;

    [Header("Settings")]
    public KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;

    void Start()
    {
        // Đảm bảo pause menu ẩn khi bắt đầu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Thêm listeners cho các nút
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ResumeGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    void Update()
    {
        // Kiểm tra nếu bấm phím Pause (mặc định là ESC)
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        // Hiển thị pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Dừng thời gian trong game
        Time.timeScale = 0f;

        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        Debug.Log("ResumeGame() called!");

        isPaused = false;

        // Ẩn pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Tiếp tục thời gian trong game
        Time.timeScale = 1f;

        Debug.Log("Game Resumed");
    }

    public void QuitGame()
    {
        Debug.Log("QuitGame() called - Saving game and returning to Start Menu!");

        // LƯU GAME TRƯỚC KHI THOÁT
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
        }
        else
        {
            Debug.LogWarning("SaveSystem not found! Creating one...");
            GameObject saveSystemObj = new GameObject("SaveSystem");
            SaveSystem saveSystem = saveSystemObj.AddComponent<SaveSystem>();
            saveSystem.SaveGame();
        }

        // QUAN TRỌNG: Đặt lại timeScale TRƯỚC KHI làm bất cứ điều gì
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Bắt đầu coroutine cleanup
        StartCoroutine(CleanupAndQuit());
    }

    private System.Collections.IEnumerator CleanupAndQuit()
    {
        // Dừng tất cả audio
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }

        // Destroy tất cả objects trong scene hiện tại
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj != null && obj != gameObject) // Không destroy chính script này
            {
                DestroyImmediate(obj);
            }
        }

        // Destroy tất cả DontDestroyOnLoad objects NGOẠI TRỪ SaveSystem
        Scene ddolScene = SceneManager.GetSceneByName("DontDestroyOnLoad");
        if (ddolScene.IsValid())
        {
            GameObject[] rootObjects = ddolScene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                // KHÔNG destroy SaveSystem!
                if (obj != null && obj.GetComponent<SaveSystem>() == null)
                {
                    Debug.Log($"Destroying DontDestroyOnLoad object: {obj.name}");
                    DestroyImmediate(obj);
                }
                else if (obj != null && obj.GetComponent<SaveSystem>() != null)
                {
                    Debug.Log($"Keeping SaveSystem: {obj.name}");
                }
            }
        }

        yield return null;

        // Reset trạng thái
        isPaused = false;

        // Load Start Menu
        SceneManager.LoadScene("StartMenu");
    }

    void OnDestroy()
    {
        // Đảm bảo timeScale được reset khi scene bị destroy
        Time.timeScale = 1f;
    }
}
