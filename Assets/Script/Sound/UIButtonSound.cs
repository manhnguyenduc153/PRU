using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 1f;
    
    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.5f;

    private Button button;
    private AudioSource audioSource;

    void Awake()
    {
        button = GetComponent<Button>();
        
        // Tạo AudioSource nếu chưa có
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Cấu hình AudioSource
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Phát âm thanh khi hover (nếu button không bị disable)
        if (button.interactable && hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound, hoverVolume);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Phát âm thanh khi click (nếu button không bị disable)
        if (button.interactable && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, clickVolume);
        }
    }

    // Phương thức để set sound từ code
    public void SetClickSound(AudioClip clip, float volume = 1f)
    {
        clickSound = clip;
        clickVolume = volume;
    }

    public void SetHoverSound(AudioClip clip, float volume = 0.5f)
    {
        hoverSound = clip;
        hoverVolume = volume;
    }
}
