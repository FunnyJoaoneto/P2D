using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static event Action<PlayerController, bool> OnGlideStateChanged;

    private Rigidbody2D rb;
    private HealthController health;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthController>();
        if (!playerInput) playerInput = GetComponent<PlayerInput>();

        lr = GetComponent<LineRenderer>();
        dj = GetComponent<DistanceJoint2D>();

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

    private Vector2 GetAimDirection()
    {
        if (moveInput.magnitude > 0.1f)
        {
            return moveInput.normalized;
        }
        float facingDirection = Mathf.Sign(transform.localScale.x);
        return new Vector2(facingDirection, 0f).normalized;
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
            if (!lightPlayer)
            {
                StartGlide();
            }
            else
            {
                AttemptGrapple();
            }
        };

        actions["Ability"].canceled += ctx =>
        {
            if (!lightPlayer)
            {
                StopGlide();
            }
            else
            {
                ReleaseGrapple();
            }
        };
    }

    void StartGlide()
    {
        if (!GroundCheck())
        {
            isGliding = true;
            rb.gravityScale = glideGravityScale;
            OnGlideStateChanged?.Invoke(this, true);
        }
    }

    void StopGlide()
    {
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

    void OnDisable()
    {
        playerInput.actions.Disable();
    }

    void Update()
    {
        CheckIlluminationEffects();
        Gravity();

        if (isGrappling && lr != null)
        {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, grapplePoint);
        }
    }

    private void CheckIlluminationEffects()
    {
        if (health == null) return;
        bool bright = IlluminationManager.Instance.IsPointBright(transform.position);
        float damagePerSecond = 10f;
        float healPerSecond = 5f;

        if (lightPlayer)
        {
            if (bright)
                health.AddHealth(healPerSecond * Time.deltaTime);
            else
                health.TakeDamage(damagePerSecond * Time.deltaTime);
        }
        else
        {
            if (bright)
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
            rb.gravityScale = glideGravityScale;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed / 3f));
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
        // NOVO: Controle de Movimento Horizontal Condicional
        // O jogador SÓ tem controle de movimento horizontal se NÃO estiver engatado.
        if (!isGrappling)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
        // Quando ISGRAPPLING é TRUE, o movimento horizontal é controlado APENAS pelo ApplySwingImpulse (abaixo).

        if (isGrappling)
        {
            ApplySwingImpulse();
            LimitSwingHeight();
        }

        if (rb.linearVelocity.y > 21f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 21f);
        }
    }

    /// <summary>
    /// Aplica força tangencial para impulsionar o balanço.
    /// </summary>
    private void ApplySwingImpulse()
    {
        // O vetor da corda (do player ao ponto de gancho)
        Vector2 ropeVector = grapplePoint - (Vector2)transform.position;

        // O vetor de velocidade tangencial (perpendicular à corda)
        Vector2 swingDirection = new Vector2(-ropeVector.y, ropeVector.x).normalized;

        if (moveInput.x != 0)
        {
            // Aplica a força na direção tangencial (balanço), respeitando o input do jogador
            rb.AddForce(swingDirection * moveInput.x * swingImpulseForce * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }

    /// <summary>
    /// Limita a altura máxima que o jogador pode subir no balanço.
    /// </summary>
    private void LimitSwingHeight()
    {
        if (dj == null) return;

        float currentDistance = Vector2.Distance(transform.position, grapplePoint);
        float originalDistance = dj.distance;

        // Limite de distância permitido (ex: 80% da distância original da corda)
        float maxAllowedDistance = originalDistance * maxSwingHeightRatio;

        // Checa se o player está acima do limite E subindo.
        if (currentDistance < maxAllowedDistance && rb.linearVelocity.y > 0)
        {
            // Reduz drasticamente a velocidade vertical para impedir que suba mais
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
            return;
        }
        if (jumping)
        {
            if (GroundCheck())
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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