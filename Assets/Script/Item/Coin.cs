using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Value")]
    [SerializeField] private int minValue = 1;
    [SerializeField] private int maxValue = 5;

    [Header("Animation")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatAmount = 0.3f;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRadius = 3f;    // Phạm vi bắt đầu hút coin
    [SerializeField] private float moveSpeedToPlayer = 6f; // Tốc độ bay về player

    [Header("Sound Effect")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;

    private int coinValue;
    private Vector3 startPos;
    private Transform player;

    void Start()
    {
        coinValue = Random.Range(minValue, maxValue + 1);
        startPos = transform.position;

        // Lưu player để không phải tìm mỗi frame
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // Animation lơ lửng
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Nếu player tồn tại và ở gần => coin tự bay tới
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance < pickupRadius)
            {
                // Tăng tốc khi lại gần player
                float step = moveSpeedToPlayer * Time.deltaTime * (1f + (pickupRadius - distance));
                transform.position = Vector2.MoveTowards(transform.position, player.position, step);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CoinManager.Instance.AddCoins(coinValue);

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);

            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Vẽ phạm vi hút coin trong editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
