using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Coin : MonoBehaviour
{
    [Header("Coin Value")]
    [SerializeField] private int minValue = 1;
    [SerializeField] private int maxValue = 5;

    [Header("Animation")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmount = 0.3f;

    [Header("Sound Effect")]
    [SerializeField] private AudioClip pickupSound;  // Gắn file âm thanh ở đây
    [SerializeField] private float soundVolume = 1f; // Điều chỉnh âm lượng

    private int coinValue;
    private Vector3 startPos;

    void Start()
    {
        // Random value cho coin
        coinValue = Random.Range(minValue, maxValue + 1);
        startPos = transform.position;
    }

    void Update()
    {
        // Animation lơ lửng cho coin
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Thêm coin vào player
            CoinManager.Instance.AddCoins(coinValue);

            // Phát âm thanh pickup (độc lập, không bị destroy)
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
            }

            // Xóa coin
            Destroy(gameObject);
        }
    }
}
