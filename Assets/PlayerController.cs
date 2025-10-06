using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public PlayerInput playerInput;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!playerInput) playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        var actions = playerInput.actions;
        actions["Move"].performed += ctx => {
            moveInput = ctx.ReadValue<Vector2>();
            Debug.Log($"{gameObject.name} Move: {moveInput}");
        };
        actions["Move"].canceled += ctx => {
            moveInput = Vector2.zero;
            Debug.Log($"{gameObject.name} Move canceled");
        };
        actions["Jump"].performed += ctx => {
            Debug.Log($"{gameObject.name} Jump pressed");
            Jump();
        };
    }

    void OnDisable()
    {
        var actions = playerInput.actions;
        actions["Move"].performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        actions["Move"].canceled -= ctx => moveInput = Vector2.zero;
        actions["Jump"].performed -= ctx => Jump();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }
}
