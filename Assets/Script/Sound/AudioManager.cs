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
    [SerializeField, Range(0f, 1f)] private float volume = 1f; // 👈 Có thể chỉnh trong Inspector
    [SerializeField] private float fadeDuration = 1f;

    private bool isInCombat = false;

    void Awake()
    {
        // Singleton pattern để giữ AudioManager xuyên suốt game
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

        // Bắt đầu với nhạc thường
        PlayNormalMusic();
    }

    void Update()
    {
        // Nếu bạn chỉnh volume trong lúc game đang chạy (Inspector), nó sẽ cập nhật ngay
        if (musicSource.volume != volume)
        {
            musicSource.volume = volume;
        }
    }

    public void PlayNormalMusic()
    {
        if (!isInCombat && musicSource.clip != normalMusic)
        {
            SwitchMusic(normalMusic);
        }
    }

    public void PlayCombatMusic()
    {
        if (!isInCombat && musicSource.clip != combatMusic)
        {
            isInCombat = true;
            SwitchMusic(combatMusic);
        }
    }

    public void StopCombatMusic()
    {
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
        StartCoroutine(FadeAndSwitch(newClip));
    }

    private IEnumerator FadeAndSwitch(AudioClip newClip)
    {
        // Fade out
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration / 2)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (fadeDuration / 2));
            yield return null;
        }

        // Đổi nhạc
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        elapsed = 0f;
        while (elapsed < fadeDuration / 2)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, volume, elapsed / (fadeDuration / 2));
            yield return null;
        }

        musicSource.volume = volume;
    }

    // 👉 Nếu muốn chỉnh bằng code cũng vẫn có thể
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        musicSource.volume = volume;
    }
}
