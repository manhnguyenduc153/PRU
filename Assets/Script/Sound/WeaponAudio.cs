using UnityEngine;

public class WeaponAudio : MonoBehaviour
{
    [Header("Weapon Sound Settings")]
    [SerializeField] private AudioClip swingSound;     // Âm thanh khi tấn công
    [SerializeField] private float volume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        // Tạo AudioSource riêng cho vũ khí
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    /// <summary>
    /// Phát âm thanh khi tấn công
    /// </summary>
    public void PlaySwingSound()
    {
        if (swingSound != null)
            audioSource.PlayOneShot(swingSound, volume);
    }
}
