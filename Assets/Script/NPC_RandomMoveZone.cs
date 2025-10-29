using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class NPC_RandomMoveZone : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;                  // tốc độ di chuyển
    public float waitTimeMin = 1f;                // thời gian nghỉ min
    public float waitTimeMax = 3f;                // thời gian nghỉ max

    [Header("Movement Zone")]
    public Collider2D moveZone;                   // vùng giới hạn di chuyển

    private Rigidbody2D rb;
    private Vector2 targetPos;
    private bool isMoving = false;
    private bool canMove = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (moveZone == null)
        {
            Debug.LogWarning($"{name}: Chưa gán MoveZone!");
        }
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            if (canMove)
            {
                PickNewTarget();
                isMoving = true;

                // Di chuyển tới khi gần target
                while (Vector2.Distance(transform.position, targetPos) > 0.1f)
                {
                    Vector2 newPos = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                    rb.MovePosition(newPos);
                    yield return null;
                }

                isMoving = false;

                // Nghỉ ngẫu nhiên 1 khoảng
                float waitTime = Random.Range(waitTimeMin, waitTimeMax);
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                yield return null;
            }
        }
    }

    private void PickNewTarget()
    {
        if (moveZone != null)
        {
            Bounds bounds = moveZone.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            targetPos = new Vector2(randomX, randomY);
        }
        else
        {
            // nếu chưa gán vùng, chỉ di chuyển quanh vị trí hiện tại
            Vector2 offset = Random.insideUnitCircle * 2f;
            targetPos = (Vector2)transform.position + offset;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Vẽ điểm đích để debug
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPos, 0.1f);
    }
}
