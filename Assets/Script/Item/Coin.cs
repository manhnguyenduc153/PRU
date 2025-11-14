using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Value")]
    [SerializeField] private int minValue = 1;
    [SerializeField] private int maxValue = 5;

    [Header("Animation")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmount = 0.3f;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRadius = 3f;
    [SerializeField] private float moveSpeedToPlayer = 15f; // base speed

    [Header("Sound Effect")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;

    private int coinValue;
    private Transform player;
    private bool isAttracting = false;

    // basePos là vị trí "không có float offset", dùng để MoveTowards
    private Vector3 basePos;

    void Start()
    {
        coinValue = Random.Range(minValue, maxValue + 1);
        basePos = transform.position;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // Tính offset vertical (float) độc lập
        float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmount;

        if (player == null)
        {
            // Nếu không có player, chỉ giữ basePos và hiển thị float
            transform.position = basePos + Vector3.up * floatOffset;
            return;
        }

        float distance = Vector3.Distance(basePos, player.position);

        if (distance < pickupRadius)
            isAttracting = true;

        if (isAttracting)
        {
            // Nếu coin ở cùng vị trí với player (gần cực nhỏ), tránh vector zero
            if (distance > 0.001f)
            {
                // Muốn coin càng "xa" càng bay nhanh: multiplier dựa trên distance / pickupRadius
                float speedMultiplier = 1f + (distance / Mathf.Max(0.0001f, pickupRadius));
                // Giới hạn multiplier để không quá lớn (tuỳ bạn có muốn)
                speedMultiplier = Mathf.Clamp(speedMultiplier, 1f, 10f);

                float step = moveSpeedToPlayer * speedMultiplier * Time.deltaTime;
                // Move base position (không đụng chạm trực tiếp tới y float offset)
                basePos = Vector3.MoveTowards(basePos, player.position, step);
            }
            else
            {
                basePos = player.position;
            }
        }

        // Cuối cùng đặt transform dựa trên basePos + float offset
        transform.position = basePos + Vector3.up * floatOffset;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
