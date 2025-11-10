using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AreaMusicZone : MonoBehaviour
{
    [Header("Zone Music Settings")]
    [SerializeField] private AudioClip areaMusic; // Nhạc riêng cho khu vực này
    [SerializeField] private bool revertToSceneMusicOnExit = true;

    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || playerInside) return;
        playerInside = true;

        if (AudioManager.Instance != null && areaMusic != null)
        {
            AudioManager.Instance.PlayTemporaryMusic(areaMusic, 1f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;

        if (AudioManager.Instance != null && revertToSceneMusicOnExit)
        {
            AudioManager.Instance.PlayNormalMusic();
        }
    }
}
