using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static event Action<PlayerController, bool> OnGlideStateChanged;
    // args: (player, isGliding)

    private Rigidbody2D rb;
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public int maxJumps = 2;
    
    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.5f);

    [Header("Gravity Settings")]
    public float baseGravity = 2f;
    public float fallSpeedMultiplier = 2f;
    public float maxFallSpeed = 18f;

    [Header("Abilities")] // This can be changed later
    public bool lightPlayer = true;

    [Header("Glide Settings")]
    public float glideGravityScale = 0.3f; // how light she is when gliding
    public float glideUpBoost = 3f;        // upward force from fans
    public bool isGliding = false;

    public PlayerInput playerInput;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!playerInput) playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        var actions = playerInput.actions;
        actions["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        actions["Move"].canceled += ctx => moveInput = Vector2.zero;
        actions["Jump"].performed += ctx => Jump(true);
        actions["Jump"].canceled += ctx => Jump(false);
        actions["Ability"].performed += ctx =>
        {
            if (!lightPlayer) StartGlide();
        };

        actions["Ability"].canceled += ctx =>
        {
            if (!lightPlayer) StopGlide();
        };
    }

    void StartGlide()
    {
        if (!GroundCheck()) // only if in air and falling
        {
            isGliding = true;
            rb.gravityScale = glideGravityScale;
            Debug.Log("Started gliding");
            OnGlideStateChanged?.Invoke(this, true);
        }
    }

    void StopGlide()
    {
        if (isGliding)
        {
            isGliding = false;
            rb.gravityScale = baseGravity;
            Debug.Log("Stopped gliding");
            OnGlideStateChanged?.Invoke(this, false);
        }
    }

    void OnDisable()
    {
        playerInput.actions.Disable();
    }

    void Update()
    {
        Gravity();
    }

    private void Gravity()
    {
        if (isGliding)
        {
            // Keep gravity reduced while gliding
            rb.gravityScale = glideGravityScale;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed / 3f));
        }
        if(rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.y > 21f)
        {
            Debug.Log("Capping upward speed");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 21f);
        }
    }

    void Balance(){
        // Placeholder for future balance adjustments
    }

    void Glide(){
        // Placeholder for future glide mechanics
    }

    void Jump(bool jumping)
    {

        if (jumping)
        {
            print("Attempting to Jump");
            if (GroundCheck())
            {
                print("Jumped");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    private bool GroundCheck()
    {
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0f, groundLayer))
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        if (groundCheckPos != null)
        {
            Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        }
    }
}
