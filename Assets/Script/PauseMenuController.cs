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
        Debug.Log("[PauseMenu] PauseMenuController Start() called");

        // Kiểm tra và log các field
        if (pauseMenuPanel == null)
        {
            Debug.LogError("[PauseMenu] pauseMenuPanel is NULL! Please assign it in Inspector!");
        }
        else
        {
            Debug.Log("[PauseMenu] pauseMenuPanel found: " + pauseMenuPanel.name);
            pauseMenuPanel.SetActive(false);
        }

        if (resumeButton == null)
        {
            Debug.LogWarning("[PauseMenu] resumeButton is NULL!");
        }
        else
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (closeButton == null)
        {
            Debug.LogWarning("[PauseMenu] closeButton is NULL!");
        }
        else
        {
            closeButton.onClick.AddListener(ResumeGame);
        }

        if (quitButton == null)
        {
            Debug.LogWarning("[PauseMenu] quitButton is NULL!");
        }
        else
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    void Update()
    {
        // Kiểm tra nếu bấm phím Pause (mặc định là ESC)
        if (Input.GetKeyDown(pauseKey))
        {
            Debug.Log($"[PauseMenu] Pause key ({pauseKey}) pressed! isPaused: {isPaused}");

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
        Debug.Log("[PauseMenu] PauseGame() called!");

        isPaused = true;

        // Hiển thị pause menu
        if (pauseMenuPanel != null)
        {
            Debug.Log("[PauseMenu] Setting pauseMenuPanel active to TRUE");
            pauseMenuPanel.SetActive(true);
            Debug.Log("[PauseMenu] pauseMenuPanel.activeSelf = " + pauseMenuPanel.activeSelf);
        }
        else
        {
            Debug.LogError("[PauseMenu] Cannot show pause menu - pauseMenuPanel is NULL!");
        }

        // Phát âm thanh mở panel
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayPanelOpenSound();
        }

        // Dừng thời gian trong game
        Time.timeScale = 0f;

        Debug.Log("[PauseMenu] Game Paused - Time.timeScale = " + Time.timeScale);
    }

    public void ResumeGame()
    {
        Debug.Log("ResumeGame() called!");

        // Phát âm thanh click/đóng panel
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayPanelCloseSound();
        }

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

        // Phát âm thanh click
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayClickSound();
        }

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
