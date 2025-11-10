using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip normalMusic;
    [SerializeField] private AudioClip combatMusic;

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private float fadeDuration = 1f;

    [HideInInspector] public bool isCutscenePlaying = false;
    private bool isInCombat = false;

    void Awake()
    {
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
    }

    void Start()
    {
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.volume = volume;
        PlayNormalMusic();
    }

    void Update()
    {
        if (musicSource.volume != volume)
        {
            musicSource.volume = volume;
        }
    }

    public void PlayNormalMusic()
    {
        // ✅ CHẶN nếu đang cutscene
        if (isCutscenePlaying) return;

        if (!isInCombat && musicSource.clip != normalMusic)
        {
            SwitchMusic(normalMusic);
        }
    }

    public void PlayCombatMusic()
    {
        // ✅ CHẶN nếu đang cutscene
        if (isCutscenePlaying) return;

        if (!isInCombat)
        {
            isInCombat = true;
            SwitchMusic(combatMusic);
        }
    }

    public void StopCombatMusic()
    {
        // ✅ CHẶN nếu đang cutscene
        if (isCutscenePlaying) return;

        if (isInCombat)
        {
            isInCombat = false;
            SwitchMusic(normalMusic);
        }
    }

    private void SwitchMusic(AudioClip newClip)
    {
        if (newClip == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeAndSwitch(newClip, fadeDuration));
    }

    private IEnumerator FadeAndSwitch(AudioClip newClip, float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        // Fade out
        while (elapsed < duration / 2)
        {
            elapsed += Time.unscaledDeltaTime; // ✅ Dùng unscaledDeltaTime để hoạt động khi Time.timeScale = 0
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2));
            yield return null;
        }

        // Đổi nhạc
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < duration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, volume, elapsed / (duration / 2));
            yield return null;
        }

        musicSource.volume = volume;
    }

    // ✅ Phương thức cho cutscene music (ưu tiên cao nhất)
    public void PlayCutsceneMusic(AudioClip cutsceneClip, float duration)
    {
        if (cutsceneClip == null) return;

        // Đánh dấu cutscene đang chạy
        isCutscenePlaying = true;

        // Dừng mọi coroutine đang chạy
        StopAllCoroutines();

        // Chuyển sang nhạc cutscene
        StartCoroutine(FadeAndSwitch(cutsceneClip, duration));
    }

    // ✅ Phương thức tạm thời (dùng cho area music, event music...)
    // Phương thức này KHÔNG set isCutscenePlaying, nên vẫn có thể bị ghi đè bởi combat
    public void PlayTemporaryMusic(AudioClip tempClip, float duration)
    {
        if (tempClip == null) return;

        // ✅ CHẶN nếu đang cutscene
        if (isCutscenePlaying) return;

        StopAllCoroutines();
        StartCoroutine(FadeAndSwitch(tempClip, duration));
    }

    public void ResumeAfterCutscene()
    {
        // ✅ Tắt cờ cutscene
        isCutscenePlaying = false;

        // Reset trạng thái combat về false vì boss đã chết
        isInCombat = false;

        // Phát nhạc bình thường
        PlayNormalMusic();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        musicSource.volume = volume;
    }

    public void SetSceneMusic(AudioClip normal, AudioClip combat)
    {
        normalMusic = normal;
        combatMusic = combat;
        PlayNormalMusic();
    }
}