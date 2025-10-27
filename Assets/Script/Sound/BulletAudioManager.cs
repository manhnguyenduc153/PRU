using UnityEngine;

public class BulletAudioManager : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("Âm thanh khi đạn va chạm với Player")]
    public AudioClip hitPlayerSound;

    [Tooltip("Âm thanh khi đạn va chạm với tường")]
    public AudioClip hitWallSound;

    [Tooltip("Âm thanh khi đạn được bắn ra (tùy chọn)")]
    public AudioClip shootSound;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("Pitch ngẫu nhiên để tạo sự đa dạng")]
    public bool randomizePitch = true;

    [Range(0.8f, 1.2f)]
    public float minPitch = 0.9f;

    [Range(0.8f, 1.2f)]
    public float maxPitch = 1.1f;

    [Header("3D Audio Settings (Optional)")]
    [Tooltip("Bật để sử dụng spatial audio")]
    public bool use3DAudio = false;

    [Range(0f, 500f)]
    public float maxDistance = 50f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Tạo AudioSource nếu chưa có (chỉ dùng cho shoot sound)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Cấu hình AudioSource
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = use3DAudio ? 1f : 0f;

        if (use3DAudio)
        {
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = maxDistance;
        }
    }

    private void Start()
    {
        // Phát âm thanh bắn nếu có
        if (shootSound != null)
        {
            PlayShootSound();
        }
    }

    /// <summary>
    /// Phát âm thanh bắn (không bị ảnh hưởng khi destroy)
    /// </summary>
    private void PlayShootSound()
    {
        if (shootSound != null)
        {
            if (use3DAudio)
            {
                Create3DAudioAtPosition(shootSound, transform.position);
            }
            else
            {
                AudioSource.PlayClipAtPoint(shootSound, transform.position, volume);
            }
        }
    }

    /// <summary>
    /// Phát âm thanh khi va chạm với Player
    /// </summary>
    public void PlayHitPlayerSound()
    {
        if (hitPlayerSound != null)
        {
            PlaySoundAtPosition(hitPlayerSound);
        }
    }

    /// <summary>
    /// Phát âm thanh khi va chạm với tường
    /// </summary>
    public void PlayHitWallSound()
    {
        if (hitWallSound != null)
        {
            PlaySoundAtPosition(hitWallSound);
        }
    }

    /// <summary>
    /// Phát âm thanh tại vị trí hiện tại (không bị ảnh hưởng khi bullet bị destroy)
    /// </summary>
    private void PlaySoundAtPosition(AudioClip clip)
    {
        if (clip == null) return;

        float pitch = randomizePitch ? Random.Range(minPitch, maxPitch) : 1f;

        if (use3DAudio)
        {
            Create3DAudioAtPosition(clip, transform.position, pitch);
        }
        else
        {
            // Tạo GameObject tạm để phát âm thanh với pitch tùy chỉnh
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = transform.position;
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();

            tempSource.clip = clip;
            tempSource.volume = volume;
            tempSource.pitch = pitch;
            tempSource.spatialBlend = 0f;
            tempSource.Play();

            Destroy(tempAudio, clip.length / pitch + 0.1f);
        }
    }

    /// <summary>
    /// Tạo 3D audio tại vị trí cụ thể
    /// </summary>
    private void Create3DAudioAtPosition(AudioClip clip, Vector3 position, float pitch = 1f)
    {
        GameObject audioObj = new GameObject("3DAudio_" + clip.name);
        audioObj.transform.position = position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.maxDistance = maxDistance;
        source.Play();

        Destroy(audioObj, clip.length / pitch + 0.1f);
    }
}