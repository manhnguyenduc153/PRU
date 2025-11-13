using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 🔹 Singleton instance
    public static PlayerController Instance { get; private set; }
    public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private int dashManaCost = 10;
    [SerializeField] private TrailRenderer trailRenderer; // Optional: hiệu ứng trail khi dash

    private PlayerControls playerControl;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;
    private PlayerMana playerMana;

    private bool facingLeft = false;
    private bool isDashing = false;
    private bool canDash = true;
    private float dashCooldownTimer = 0f;

    private void Awake()
    {
        // 🧠 Singleton logic
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ⚙️ Original setup
        playerControl = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        playerMana = GetComponent<PlayerMana>();

        // ✅ Kiểm tra PlayerMana
        if (playerMana == null)
        {
            Debug.LogError("PlayerMana component not found on " + gameObject.name);
        }

        // Nếu không gán TrailRenderer trong Inspector, thử lấy trên cùng object hoặc children
        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = GetComponentInChildren<TrailRenderer>();
            }
        }

        if (trailRenderer != null)
        {
            // đảm bảo mặc định tắt emitting
            trailRenderer.emitting = false;
            // xóa trail cũ nếu có
            trailRenderer.Clear();
        }
        else
        {
            Debug.LogWarning("TrailRenderer not assigned or found on " + gameObject.name + ". Dash trail will be skipped.");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        playerControl.Enable();
        playerControl.Movement.Dash.performed += _ => TryDash();
    }

    private void OnDisable()
    {
        playerControl.Disable();
        playerControl.Movement.Dash.performed -= _ => TryDash();
    }

    private void Update()
    {
        PlayerInput();

        // ✅ Cooldown timer
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                canDash = true;
            }
        }

        // ✅ FALLBACK: Nếu không dùng Input System, dùng KeyCode
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryDash();
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            AdjustPlayerFacingDirection();
            Move();
        }
    }

    private void PlayerInput()
    {
        movement = playerControl.Movement.Move.ReadValue<Vector2>();
        myAnimator.SetFloat("moveX", movement.x);
        myAnimator.SetFloat("moveY", movement.y);
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);

        if (mousePos.x < playerScreenPoint.x)
        {
            mySpriteRenderer.flipX = true;
            FacingLeft = true;
        }
        else
        {
            mySpriteRenderer.flipX = false;
            FacingLeft = false;
        }
    }

    // ========================================
    // 🚀 DASH SYSTEM
    // ========================================

    private void TryDash()
    {
        // ❌ Kiểm tra điều kiện
        if (isDashing || !canDash)
        {
            Debug.Log("Dash is on cooldown!");
            return;
        }

        if (playerMana == null || !playerMana.HasEnoughMana(dashManaCost))
        {
            Debug.Log("Not enough mana to dash!");
            return;
        }

        // ✅ Thực hiện dash
        playerMana.UseMana(dashManaCost);
        StartCoroutine(PerformDash());
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;
        dashCooldownTimer = dashCooldown;

        Vector2 dashDirection = GetDashDirection();

        // --- Trail: chuẩn bị hiển thị ---
        if (trailRenderer != null)
        {
            // xóa trail hiện có để không thấy vệt cũ
            trailRenderer.Clear();

            // bật emitting trước khi di chuyển, rồi chờ 1 frame để Unity internal update (giúp trail bắt đầu vẽ đúng)
            trailRenderer.emitting = true;
            Debug.Log("Trail emitting set to TRUE");
            yield return null; // chờ 1 frame render/update
        }

        // Optional: Trigger dash animation
        myAnimator.SetTrigger("Dash");

        float dashTimer = 0f;

        // Di chuyển trong FixedUpdate-friendly loop
        while (dashTimer < dashDuration)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
            dashTimer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Chờ 1 frame để trail có time sample cuối cùng, và 1 tí delay để tránh bị cắt ngắn
        yield return null;
        yield return new WaitForSeconds(0.02f);

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
            Debug.Log("Trail emitting set to FALSE");
        }

        isDashing = false;
    }

    private Vector2 GetDashDirection()
    {
        // Nếu đang di chuyển, dash theo hướng di chuyển
        if (movement.magnitude > 0.1f)
        {
            return movement.normalized;
        }

        // Nếu đứng yên, dash theo hướng đang nhìn (dựa vào chuột)
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(transform.position);

        if (mousePos.x < playerScreenPoint.x)
        {
            return Vector2.left; // Dash sang trái
        }
        else
        {
            return Vector2.right; // Dash sang phải
        }
    }

    // ========================================
    // 🔍 PUBLIC GETTERS (Optional)
    // ========================================

    public bool IsDashing()
    {
        return isDashing;
    }

    public bool CanDash()
    {
        return canDash && playerMana != null && playerMana.HasEnoughMana(dashManaCost);
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        Debug.Log($"Move speed set to: {moveSpeed}");
    }
}
