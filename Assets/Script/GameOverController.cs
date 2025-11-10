using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameOverController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI finalScoreText;

    private void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "GameOver")
        {
            // Lấy root objects của scene đặc biệt "DontDestroyOnLoad"
            GameObject temp = null;
            Scene dontDestroyScene = SceneManager.GetSceneByName("DontDestroyOnLoad");

            if (dontDestroyScene.IsValid())
            {
                GameObject[] rootObjects = dontDestroyScene.GetRootGameObjects();
                foreach (GameObject obj in rootObjects)
                {
                    Destroy(obj);
                }
            }
        }
    }

    void Start()
    {
        var es = EventSystem.current;
        if (es != null)
        {
            if (es.GetComponent<BaseInputModule>() == null)
            {
                Debug.LogWarning("[GameOver] EventSystem missing Input Module, adding StandaloneInputModule...");
                es.gameObject.AddComponent<StandaloneInputModule>();
            }
        }
        else
        {
            Debug.LogWarning("[GameOver] No EventSystem found, creating one...");
            var newEs = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    public void RestartGame()
    {
        Debug.Log("[GameOver] Restart button clicked!");
        SceneManager.LoadScene("PlayScene");
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("[GameOver] ReturnToMainMenu clicked!");
        SceneManager.LoadScene("StartMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}