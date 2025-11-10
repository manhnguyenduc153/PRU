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
    [SerializeField] private float moveSpeedToPlayer = 15f; // tăng tốc cơ bản

    [Header("Sound Effect")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;

    private int coinValue;
    private Vector3 startPos;
    private Transform player;
    private bool isAttracting = false;

    void Start()
    {
        coinValue = Random.Range(minValue, maxValue + 1);
        startPos = transform.position;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        // Float animation
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < pickupRadius)
            isAttracting = true;

        if (isAttracting)
        {
            // Bay thẳng về player với lực tăng theo khoảng cách
            Vector3 dir = (player.position - transform.position).normalized;

            // Nhân thêm hệ số (distance / pickupRadius) để coin càng xa càng bay nhanh
            float speedMultiplier = 1f + (pickupRadius - distance);

            transform.position += dir * moveSpeedToPlayer * speedMultiplier * Time.deltaTime;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
