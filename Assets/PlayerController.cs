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

    private ObjetoInteragivel objetoInteragivelProximo;

    public float damagePerSecond = 10f;
    public float healPerSecond = 5f;


    public float moveSpeed = 5f;

    [Header("Jump Tuning (Height + Time)")]
    public float jumpHeight = 4.5f;   // world units
    public float timeToApex = 0.35f;  // seconds (smaller = snappier)
    public float lowJumpMultiplier = 2.0f; // extra gravity when jump released early

    private bool jumpHeld;
    public float jumpForce = 7f;
    public int maxJumps = 2;

    public LayerMask groundLayer;
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.5f);

    public float baseGravity = 2f;
    public float fallSpeedMultiplier = 2f;
    public float maxFallSpeed = 18f;

    public bool lightPlayer = true;

    public float glideGravityScale = 0.3f;
    public float glideUpBoost = 3f;
    public bool isGliding = false;
    private bool glideQueued = false;

    [Header("Grapple Settings")]
    public Vector2 grappleAnchorOffset = new Vector2(2f, 3f);
    public float maxGrappleDistance = 15f;
    public LayerMask grapplePointLayer;
    public string grapplePointTag = "GrapplePoint";
    public float swingImpulseForce = 15f;

    [Tooltip("Limite total do arco de balanço (200 recomendado).")]
    public float maxGrappleAngle = 200f;

    [Range(0f, 50f)]
    public float grappleBrakingForce = 20f; // Força da 'gravidade fake' no topo
    [Range(1f, 10f)]
    public float brakingSmoothness = 3f;    // Curva de suavidade da parada

    public float forceChargeRate = 50f;
    public float minReboundForce = 15f;

    private float currentSwingForce = 0f;
    private bool isAbilityButtonHeld = false;

    public PlayerInput playerInput;
    private Vector2 moveInput;
    public float grappleCheckRadius = 0.1f;
    private LineRenderer lr;
    private DistanceJoint2D dj;

    [HideInInspector] public bool isGrappling = false;
    private Vector2 grapplePoint;
    private float lastMoveDirection = 1f;
    private float lockedSwingDirection = 0f;

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
        RecalculateJump();

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

    private void OnValidate()
    {
        if (timeToApex < 0.05f) timeToApex = 0.05f;
        if (jumpHeight < 0.1f) jumpHeight = 0.1f;
        RecalculateJump();
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
            controlledAnimator.SetBool("IsGrappling", false);
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

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (PlayerGlobalLock.movementLocked)
            return;
        moveInput = ctx.ReadValue<Vector2>();
        if (isGrappling && Mathf.Abs(moveInput.x) > 0.1f)
        {
            lockedSwingDirection = Mathf.Sign(moveInput.x);
        }
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (PlayerGlobalLock.movementLocked)
            return;

        if (lightPlayer && isGrappling)
        {
            return;
        }

        if (ctx.performed)
        {
            Debug.Log("pulei");
            Jump(true);
        }
        else if (ctx.canceled)
            Jump(false);
    }

    public void OnAbility(InputAction.CallbackContext ctx)
    {
        if (PlayerGlobalLock.movementLocked)
            return;
        if (ctx.performed)
        {
            isAbilityButtonHeld = true;
            if (!lightPlayer) StartGlide();
            else AttemptGrapple();
        }
        else if (ctx.canceled)
        {
            isAbilityButtonHeld = false;
            if (!lightPlayer) StopGlide();
            else ReleaseGrapple();
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (PlayerGlobalLock.movementLocked)
            return;

        if (ctx.performed)
        {
            Debug.Log("INTERAGI");

            if (objetoInteragivelProximo != null)
            {
                if (lightPlayer)
                {
                    objetoInteragivelProximo.PressionarBotao("PLATAFORMA");
                }
                else
                {
                    objetoInteragivelProximo.PressionarBotao("VINHA");
                }
            }
        }
    }

    void StartGlide()
    {
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
        if (!lightPlayer || isGrappling || dj == null) return;

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

            if (targetPosition.y <= transform.position.y)
            {
                continue;
            }

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
            controlledAnimator.SetBool("IsGrappling", true);
            currentSwingForce = 0f;

            lockedSwingDirection = (grapplePoint.x > transform.position.x) ? 1f : -1f;

            if (lr != null && dj != null)
            {
                lr.enabled = true;

                dj.enabled = true;
                dj.connectedAnchor = grapplePoint;
                dj.distance = closestDistance;
                Vector2 worldAnchor = transform.TransformPoint(dj.anchor);
                dj.distance = Vector2.Distance(worldAnchor, grapplePoint);
                rb.gravityScale = baseGravity;
                rb.linearVelocity *= 0.1f;
            }
        }

    }

    private void ReleaseGrapple()
    {
        if (!isGrappling) return;

        controlledAnimator.SetBool("IsGrappling", false);
        isGrappling = false;
        currentSwingForce = 0f;
        lockedSwingDirection = 0f;

        if (lr != null && dj != null)
        {
            lr.enabled = false;
            dj.enabled = false;
        }

        // Reseta a rotação ao soltar
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        if (PlayerGlobalLock.movementLocked)
        {
            moveInput = Vector2.zero;
            Gravity();
            return;
        }

        CheckIlluminationEffects();
        Gravity();

        if (glideQueued && rb.linearVelocity.y <= 0f && !GroundCheck())
        {
            glideQueued = false;
            ActivateGlide();
        }

        if (isGrappling && lr != null)
        {
            lr.SetPosition(0, transform.TransformPoint(dj.anchor));
            lr.SetPosition(1, grapplePoint);

            // ADIÇÃO: Rotação do personagem em direção ao ponto do gancho
            Vector2 direction = grapplePoint - (Vector2)transform.position;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), Time.deltaTime * 10f);
        }
        else
        {
            // Retorna suavemente para a rotação normal quando não estiver pendurado
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 10f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ExitReady") || other.CompareTag("ExitGate"))
        {
            if (GoalManager.Instance != null)
            {
                GoalManager.Instance.EntrarPortao();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ExitReady") || other.CompareTag("ExitGate"))
        {
            if (GoalManager.Instance != null)
            {
                GoalManager.Instance.SairPortao();
            }
        }
    }

    private void CheckIlluminationEffects()
    {
        if (health == null || SistemaDiaNoite.Instance == null) return;
        if (!health.isAlive) return;

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
        if (rb == null) return;

        if (isGrappling)
        {
            rb.gravityScale = baseGravity;
            return;
        }

        if (isGliding)
        {
            rb.gravityScale = glideGravityScale;

            if (rb.linearVelocity.y < 0f)
            {
                rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Max(rb.linearVelocity.y, -maxFallSpeed / 3f)
                );
            }

            if (rb.linearVelocity.y > 30f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 30f);
            }

            return;
        }

        float g = baseGravity;

        if (rb.linearVelocity.y < 0f)
            g *= fallSpeedMultiplier;
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
            g *= lowJumpMultiplier;

        rb.gravityScale = g;

        if (rb.linearVelocity.y < -maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
    }


    void FixedUpdate()
    {
        if (PlayerGlobalLock.movementLocked)
        {
            moveInput = Vector2.zero;

            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            if (isGrappling) ReleaseGrapple();
            if (isGliding) StopGlide();

            return;
        }

        CheckGroundState();
        if (isGrappling)
        {
            HandleGrappleForces();
            LimitVerticalMovement();
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

        if (rb != null)
        {
            controlledAnimator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        }
    }

    private void HandleGrappleForces()
    {
        Vector2 ropeVector = grapplePoint - (Vector2)transform.position;
        Vector2 swingTangent = new Vector2(-ropeVector.y, ropeVector.x).normalized;

        if (isAbilityButtonHeld)
        {
            currentSwingForce = Mathf.MoveTowards(
                currentSwingForce,
                swingImpulseForce,
                forceChargeRate * Time.fixedDeltaTime
            );
        }
        else
        {
            currentSwingForce = Mathf.MoveTowards(
                currentSwingForce,
                0f,
                forceChargeRate * Time.fixedDeltaTime * 0.5f
            );
        }

        float desiredDirection = 0f;
        float forceToApply = 0f;

        if (Mathf.Abs(moveInput.x) > 0.1f)
        {
            desiredDirection = Mathf.Sign(moveInput.x);
            forceToApply = currentSwingForce + minReboundForce;
        }

        if (Mathf.Abs(desiredDirection) < 0.1f) return;

        if (Mathf.Sign(swingTangent.x) != desiredDirection)
        {
            swingTangent *= -1f;
        }

        if (forceToApply > 0f)
        {
            rb.AddForce(swingTangent * forceToApply, ForceMode2D.Force);
        }
    }

    private void LimitVerticalMovement()
    {
        if (!isGrappling) return;

        Vector2 directionToPlayer = (Vector2)transform.position - grapplePoint;
        float currentAngle = Vector2.SignedAngle(Vector2.right, directionToPlayer);

        float extraAngle = (maxGrappleAngle - 180f) / 2f;
        float topLimit = 180f - extraAngle;
        float bottomLimit = extraAngle;

        if (currentAngle > 0 && currentAngle < 180)
        {
            float proximity = 0f;

            if (currentAngle < topLimit && currentAngle > 90)
                proximity = Mathf.InverseLerp(90f, topLimit, currentAngle);
            else if (currentAngle > bottomLimit && currentAngle <= 90)
                proximity = Mathf.InverseLerp(90f, bottomLimit, currentAngle);

            if (proximity > 0.05f && rb.linearVelocity.y > 0)
            {
                float brakingIntensity = Mathf.Pow(proximity, brakingSmoothness);
                rb.AddForce(Vector2.down * brakingIntensity * grappleBrakingForce, ForceMode2D.Force);
                rb.linearVelocity *= (1f - (0.05f * proximity));
            }
        }
    }

    private void RecalculateJump()
    {
        float gravity = (2f * jumpHeight) / (timeToApex * timeToApex); // m/s^2
        jumpForce = gravity * timeToApex;                               // m/s

        baseGravity = gravity / Mathf.Abs(Physics2D.gravity.y);         // gravityScale
    }


    void Jump(bool jumping)
    {
        if (rb == null) return;

        if (jumping)
        {
            jumpHeld = true;

            if (isGrappling)
            {
                ReleaseGrapple();
                rb.linearVelocity = new Vector2(rb.linearVelocity.x * 1.5f, jumpForce * 1.2f);

                if (controlledAnimator != null && isAnimatorInitialized)
                {
                    controlledAnimator.SetBool("IsGrounded", false);
                    controlledAnimator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
                }
                return;
            }

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
            jumpHeld = false;
        }
    }

    public void SetProximoInteragivel(ObjetoInteragivel interagivel)
    {
        objetoInteragivelProximo = interagivel;
        Debug.Log($"Jogador perto de {interagivel.gameObject.name}");
    }

    public void ClearProximoInteragivel(ObjetoInteragivel interagivel)
    {
        if (objetoInteragivelProximo == interagivel)
        {
            objetoInteragivelProximo = null;
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