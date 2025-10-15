using System.Collections;
using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [SerializeField] private float knockbackDuration = 0.2f;

    private Rigidbody2D rb;
    private bool isKnockback = false;
    private Animator myAnimator;
    private PlayerController playerController; // script điều khiển player

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (!isKnockback)
        {
            if (myAnimator != null)
                myAnimator.SetTrigger("Hurt");

            StartCoroutine(KnockbackCoroutine(direction, force));
        }
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction, float force)
    {
        isKnockback = true;

        // Tạm thời khóa điều khiển
        if (playerController != null)
            playerController.enabled = false;

        rb.velocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        // Bật lại điều khiển
        if (playerController != null)
            playerController.enabled = true;

        isKnockback = false;
    }
}
