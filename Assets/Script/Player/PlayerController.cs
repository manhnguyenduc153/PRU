using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 🔹 Singleton instance
    public static PlayerController Instance { get; private set; }

    public bool FacingLeft { get { return facingLeft; } set { facingLeft = value; } }

    [SerializeField] private float moveSpeed = 8f;

    private PlayerControls playerControl;
    private Vector2 movement;
    private Rigidbody2D rb;

    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;

    private bool facingLeft = false;

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        playerControl.Enable();
    }

    private void OnDisable()
    {
        playerControl.Disable();
    }

    private void Update()
    {
        PlayerInput();
    }

    private void FixedUpdate()
    {
        AdjustPlayerFacingDirection();
        Move();
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
}
