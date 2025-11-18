using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static event Action<PlayerController, bool> OnGlideStateChanged;

    private Rigidbody2D rb;
    private HealthController health;

    private Animator controlledAnimator;
    private bool isAnimatorInitialized = false;
    private bool isGrounded = false;
    private bool wasGroundedLastFrame = false;
    private int jumpCount;

    [Header("Efeitos Dia/Noite")]
    [Tooltip("Dano (perda de vida) por segundo em ambiente desfavorável.")]
    public float damagePerSecond = 10f;
    [Tooltip("Cura (ganho de vida) por segundo em ambiente favorável.")]
    public float healPerSecond = 5f;
    

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

    [Header("Abilities")]
    public bool lightPlayer = true;

    [Header("Glide Settings")]
    public float glideGravityScale = 0.3f;
    public float glideUpBoost = 3f;
    public bool isGliding = false;
    private bool glideQueued = false;

    [Header("Grapple Settings")]
    public float maxGrappleDistance = 15f;
    public LayerMask grapplePointLayer;
    public string grapplePointTag = "GrapplePoint";

    // VARIÁVEIS PARA O BALANÇO
    public float swingImpulseForce = 15f;
    public float maxSwingHeightRatio = 0.8f;

    public PlayerInput playerInput;
    private Vector2 moveInput;
    public float grappleCheckRadius = 0.1f;
    private LineRenderer lr;
    private DistanceJoint2D dj;

    [HideInInspector] public bool isGrappling = false;
    private Vector2 grapplePoint;
    private float lastMoveDirection = 1f;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction abilityAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthController>();
        if (!playerInput) playerInput = GetComponent<PlayerInput>();

        lr = GetComponent<LineRenderer>();
        dj = GetComponent<DistanceJoint2D>();

        InitializeAnimator();

        // Configuração inicial do DistanceJoint2D
        if (dj != null)
        {
            dj.autoConfigureDistance = false;
            dj.enableCollision = false;
            dj.maxDistanceOnly = false;
            dj.breakForce = Mathf.Infinity;
            dj.enabled = false;
        }

        if (lr != null) lr.enabled = false;
    }

    private void InitializeAnimator()
    {
        string spriteChildName = lightPlayer ? "SpriteKnight" : "nightgirl";
        Transform spriteChild = transform.Find(spriteChildName);
        if (spriteChild != null)
        {
            controlledAnimator = spriteChild.GetComponent<Animator>();
        }
        else
        {
            controlledAnimator = GetComponent<Animator>();
        }
        if (controlledAnimator == null)
        {
            Debug.LogError($"Animator Component not found for '{spriteChildName}'! Check if it's on the main GameObject or the '{spriteChildName}' child.");
            isAnimatorInitialized = false;
            return;
        }
        controlledAnimator.enabled = true;
        isAnimatorInitialized = true;
        if (controlledAnimator.isInitialized)
        {
            controlledAnimator.SetFloat("Speed", 0f);
            controlledAnimator.SetFloat("Direction", lastMoveDirection);
            controlledAnimator.SetBool("IsGrounded", true);
            controlledAnimator.SetFloat("VerticalSpeed", 0f);
        }
    }

    private Vector2 GetAimDirection()
    {
        if (moveInput.magnitude > 0.1f)
        {
            return moveInput.normalized;
        }
        float facingDirection = lastMoveDirection;
        return new Vector2(facingDirection, 0f).normalized;
    }

    void OnEnable()
    {
        //hmmmm
    }

    void OnDisable()
    {
        //hmmmmmmm
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (PlayerGlobalLock.movementLocked)
            return;
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (PlayerGlobalLock.movementLocked)
            return;
        if (ctx.performed)
            Jump(true);
        else if (ctx.canceled)
            Jump(false);
    }

    public void OnAbility(InputAction.CallbackContext ctx)
    {
        if (PlayerGlobalLock.movementLocked)
            return;
        if (ctx.performed)
        {
            if (!lightPlayer) StartGlide();
            else AttemptGrapple();
        }
        else if (ctx.canceled)
        {
            if (!lightPlayer) StopGlide();
            else ReleaseGrapple();
        }
    }

    void StartGlide()
    {
        //If i remove this line because of the base gravity change, the player can jump very high
        //if (GroundCheck())
            //return;

        if (rb.linearVelocity.y > 0f)
        {
            glideQueued = true;
            return;
        }

        ActivateGlide();
    }

    void ActivateGlide()
    {
        isGliding = true;
        rb.gravityScale = glideGravityScale;
        OnGlideStateChanged?.Invoke(this, true);
    }

    void StopGlide()
    {
        glideQueued = false;

        if (isGliding)
        {
            isGliding = false;
            rb.gravityScale = baseGravity;
            OnGlideStateChanged?.Invoke(this, false);
        }
    }

    private void AttemptGrapple()
    {
        if (!lightPlayer || isGrappling) return;

        int grappleLayerValue = LayerMask.NameToLayer("GrapplePoint");
        if (grappleLayerValue == -1)
        {
            Debug.LogError("Layer 'GrapplePoint' not found. Check Project Settings -> Tags and Layers.");
            return;
        }
        LayerMask forcedGrappleLayer = 1 << grappleLayerValue;

        Vector2 aimDirection = GetAimDirection();

        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(
            transform.position,
            maxGrappleDistance,
            forcedGrappleLayer
        );

        Vector2 bestGrapplePoint = Vector2.zero;
        float closestDistance = float.MaxValue;

        foreach (Collider2D targetCollider in potentialTargets)
        {
            if (!targetCollider.CompareTag(grapplePointTag))
            {
                continue;
            }

            Vector2 targetPosition = targetCollider.transform.position;
            Vector2 directionToTarget = (targetPosition - (Vector2)transform.position).normalized;
            float distanceToTarget = Vector2.Distance(transform.position, targetPosition);

            // --- Filtro de Obstrução (Raycast) com Offset ---
            float offset = 0.1f;
            Vector2 startPoint = (Vector2)transform.position + directionToTarget * offset;
            float rayDistance = distanceToTarget - offset;

            RaycastHit2D hit = Physics2D.Raycast(
                startPoint,
                directionToTarget,
                rayDistance,
                groundLayer
            );
            if (hit.collider != null)
            {
                continue;
            }
            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                bestGrapplePoint = targetPosition;
            }
        }
        if (bestGrapplePoint != Vector2.zero)
        {
            grapplePoint = bestGrapplePoint;
            isGrappling = true;
            if (lr != null && dj != null)
            {
                lr.enabled = true;
                dj.enabled = true;
                dj.connectedAnchor = grapplePoint;
                dj.distance = closestDistance;
                rb.gravityScale = baseGravity;
                rb.linearVelocity *= 0.1f;
            }
        }
    }

    private void ReleaseGrapple()
    {
        if (!isGrappling) return;

        isGrappling = false;

        if (lr != null && dj != null)
        {
            lr.enabled = false;
            dj.enabled = false;
        }
    }

    void Update()
    {
        if (PlayerGlobalLock.movementLocked)
        {
            moveInput = Vector2.zero;
            Gravity(); // or even skip gravity if you want a full freeze
            return;
        }

        CheckIlluminationEffects();
        Gravity();

        // Auto-activate queued glide when falling
        if (glideQueued && rb.linearVelocity.y <= 0f && !GroundCheck())
        {
            glideQueued = false;
            ActivateGlide();
        }

        if (isGrappling && lr != null)
        {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, grapplePoint);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se a colisão é com a porta em qualquer um dos seus estados.
        // Isso garante que o contador de entrada funcione mesmo antes de coletar os itens.
        if (other.CompareTag("ExitReady") || other.CompareTag("ExitGate"))
        {
            if (GoalManager.Instance != null)
            {
                // Informa ao GoalManager que um jogador entrou
                GoalManager.Instance.EntrarPortao();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Verifica se o jogador está saindo da área da porta em qualquer um dos seus estados.
        // Isso garante que o contador de saída funcione, prevenindo o travamento da transição.
        if (other.CompareTag("ExitReady") || other.CompareTag("ExitGate"))
        {
            if (GoalManager.Instance != null)
            {
                // Informa ao GoalManager que um jogador saiu
                GoalManager.Instance.SairPortao();
            }
        }
    }

    private void CheckIlluminationEffects()
    {
        if (health == null || SistemaDiaNoite.Instance == null) return;
        if (!health.isAlive) return;

        // 1. Verifica se a zona atual é Dia (BrightZone)
        bool isInBrightZone = SistemaDiaNoite.Instance.IsInBrightZone(transform.position.x);

        if (lightPlayer)
        {
            if (isInBrightZone)
                health.AddHealth(healPerSecond * Time.deltaTime);
            else
                health.TakeDamage(damagePerSecond * Time.deltaTime);
        }
        else
        {
            if (isInBrightZone)
                health.TakeDamage(damagePerSecond * Time.deltaTime);
            else
                health.AddHealth(healPerSecond * Time.deltaTime);
        }
    }

    private void Gravity()
    {
        if (isGrappling)
        {
            rb.gravityScale = baseGravity;
            return;
        }
        if (isGliding)
        {
            if (rb.linearVelocity.y > 0f)
            {
                rb.gravityScale = baseGravity;
                return;
            }
            rb.gravityScale = glideGravityScale;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed / 3f));
            if( rb.linearVelocity.y > 21f){
                Debug.Log("Limiting glide upward speed");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 21f);
            }
            return;
        }
        if (rb.linearVelocity.y < 0)
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
        // If movement is globally locked, clear input and stop horizontal movement
        if (PlayerGlobalLock.movementLocked)
        {
            moveInput = Vector2.zero;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            // Optional: also stop grapple and glide while locked
            if (isGrappling) ReleaseGrapple();
            if (isGliding)  StopGlide();

            // Skip the rest of FixedUpdate
            return;
        }

        CheckGroundState();
        if (isGrappling)
        {
            ApplySwingImpulse();
            LimitSwingHeight();
            return;
        }
        if (rb == null) return;

        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        if (isAnimatorInitialized)
        {
            if (isGrounded)
            {
                HandleMovementAnimation(moveInput.x);
            }
            else
            {
                HandleAirborneAnimation();
            }
        }

        if (rb.linearVelocity.y > 25f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 25f);
        }
    }

    private void CheckGroundState()
    {
        isGrounded = GroundCheck();
        if (isGrounded && !wasGroundedLastFrame)
        {
            StopGlide();
        }
        if (controlledAnimator != null && isAnimatorInitialized)
        {
            controlledAnimator.SetBool("IsGrounded", isGrounded);
            if (isGrounded)
            {
                controlledAnimator.SetFloat("VerticalSpeed", 0f);
            }
        }
        wasGroundedLastFrame = isGrounded;
    }

    private void HandleMovementAnimation(float horizontalInput)
    {
        if (!isAnimatorInitialized || controlledAnimator == null) return;
        float speed = Mathf.Abs(horizontalInput) > 0.1f ? 1f : 0f;
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            lastMoveDirection = Mathf.Sign(horizontalInput);
        }
        controlledAnimator.SetFloat("Speed", speed);
        controlledAnimator.SetFloat("Direction", lastMoveDirection);
    }
    private void HandleAirborneAnimation()
    {
        if (!isAnimatorInitialized || controlledAnimator == null) return;
        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            lastMoveDirection = Mathf.Sign(moveInput.x);
        }
        controlledAnimator.SetFloat("Direction", lastMoveDirection);
        // CORREÇÃO DE SEGURANÇA: Evita erro se rb for destruído no meio da animação
        if (rb != null)
        {
            controlledAnimator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        }
    }

    private void ApplySwingImpulse()
    {
        Vector2 ropeVector = grapplePoint - (Vector2)transform.position;
        Vector2 swingDirection = new Vector2(-ropeVector.y, ropeVector.x).normalized;
        if (moveInput.x != 0)
        {
            rb.AddForce(swingDirection * moveInput.x * swingImpulseForce * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }
    private void LimitSwingHeight()
    {
        if (dj == null) return;
        float currentDistance = Vector2.Distance(transform.position, grapplePoint);
        float originalDistance = dj.distance;
        float maxAllowedDistance = originalDistance * maxSwingHeightRatio;
        if (currentDistance < maxAllowedDistance && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    void Balance()
    {
        // Placeholder for future balance adjustments
    }

    void Glide()
    {
        // Placeholder for future glide mechanics
    }

    void Jump(bool jumping)
    {
        if (isGrappling && jumping)
        {
            ReleaseGrapple();
            // Dá um pequeno boost ao pular do gancho
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 1.5f, jumpForce * 1.2f);
            if (controlledAnimator != null && isAnimatorInitialized)
            {
                controlledAnimator.SetBool("IsGrounded", false);
                controlledAnimator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
            }
            return;
        }
        if (jumping)
        {
            if (GroundCheck())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                if (controlledAnimator != null && isAnimatorInitialized)
                {
                    controlledAnimator.SetBool("IsGrounded", false);
                    controlledAnimator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
                }
            }
        }
        else
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }
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