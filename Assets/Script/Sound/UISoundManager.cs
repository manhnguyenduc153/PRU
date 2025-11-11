using UnityEngine;
using UnityEngine.UI;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip panelOpenSound;
    [SerializeField] private AudioClip panelCloseSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.3f;
    [Range(0f, 1f)]
    [SerializeField] private float panelVolume = 0.7f;

    private AudioSource audioSource;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    void Start()
    {
        // Tự động thêm UIButtonSound cho tất cả buttons trong scene
        AutoSetupButtons();
    }

    // Tự động setup sound cho tất cả buttons
    public void AutoSetupButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);
        foreach (Button button in allButtons)
        {
            UIButtonSound buttonSound = button.GetComponent<UIButtonSound>();
            if (buttonSound == null)
            {
                buttonSound = button.gameObject.AddComponent<UIButtonSound>();
            }

            // Set default sounds
            if (buttonClickSound != null)
            {
                buttonSound.SetClickSound(buttonClickSound, clickVolume);
            }
            if (buttonHoverSound != null)
            {
                buttonSound.SetHoverSound(buttonHoverSound, hoverVolume);
            }
        }

        Debug.Log($"[UISoundManager] Auto-setup sound for {allButtons.Length} buttons");
    }

    // Phát âm thanh click
    public void PlayClickSound()
    {
        if (buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound, clickVolume);
        }
    }

    // Phát âm thanh hover
    public void PlayHoverSound()
    {
        if (buttonHoverSound != null)
        {
            audioSource.PlayOneShot(buttonHoverSound, hoverVolume);
        }
    }

    // Phát âm thanh mở panel
    public void PlayPanelOpenSound()
    {
        if (panelOpenSound != null)
        {
            audioSource.PlayOneShot(panelOpenSound, panelVolume);
        }
    }

    // Phát âm thanh đóng panel
    public void PlayPanelCloseSound()
    {
        if (panelCloseSound != null)
        {
            audioSource.PlayOneShot(panelCloseSound, panelVolume);
        }
    }

    // Play custom sound
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}
