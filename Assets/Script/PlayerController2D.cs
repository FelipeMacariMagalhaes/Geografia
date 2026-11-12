using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float maxMoveForce = 35f;   // vento precisa passar disso pra vencer
    [SerializeField] private float brakeForce = 12f;     // freio ao soltar o botão
    [SerializeField, Range(0f, 1f)] private float airControl = 0.6f;

    [Header("Pulo")]
    [SerializeField] private float jumpSpeed = 14f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float fallGravityMultiplier = 1.6f;
    [SerializeField] private float lowJumpGravityMultiplier = 2.5f;

    [Header("Chão")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Visual (boing)")]
    [SerializeField] private Transform visual; // filho com o SpriteRenderer
    [SerializeField] private float springStrength = 260f;  // maior = volta mais rápido
    [SerializeField] private float springDamping = 9f;     // menor = quica mais vezes
    [SerializeField] private float jumpKick = 6f;          // esticada ao pular
    [SerializeField] private float landKick = 0.35f;       // amassada ao pousar
    [SerializeField] private float stretchPerSpeed = 0.025f;
    [SerializeField] private float maxStretch = 0.35f;
    [SerializeField] private float walkFrequency = 16f;
    [SerializeField] private float walkSquash = 0.10f;
    [SerializeField] private float bobHeight = 0.06f;
    [SerializeField] private float maxLean = 5f;          // 0 = sem inclinação
    [SerializeField] private float leanSmooth = 10f;

    public float MoveInput { get; private set; }
    public bool IsGrounded { get; private set; }
    public int Facing { get; private set; } = 1;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float defaultGravity;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private float lastFallSpeed;
    private bool jumpHeld;

    // Visual
    private Vector3 basePos, baseScale;
    private Vector2 squash, squashVel;
    private float walkTime, currentLean;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;

        PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction")
        {
            friction = 0f,
            bounciness = 0f
        };
        rb.sharedMaterial = noFriction;
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.sharedMaterial = noFriction;

        sr = visual.GetComponentInChildren<SpriteRenderer>();
        basePos = visual.localPosition;
        baseScale = visual.localScale;
    }

    private void Update()
    {
        // Input
        MoveInput = Input.GetAxisRaw("Horizontal");
        if (MoveInput > 0.01f) Facing = 1;
        else if (MoveInput < -0.01f) Facing = -1;

        jumpHeld = Input.GetButton("Jump");
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // Chão
        bool wasGrounded = IsGrounded;
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!IsGrounded)
            lastFallSpeed = rb.linearVelocity.y;

        if (!wasGrounded && IsGrounded && lastFallSpeed < -1f)
            HandleLand(Mathf.Abs(lastFallSpeed));

        // Coyote time
        if (IsGrounded && rb.linearVelocity.y <= 0.01f) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.deltaTime;

        // Pulo
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            HandleJump();
        }

        UpdateVisual();
    }

    private void FixedUpdate()
    {
        // Movimento por FORÇA (vento e redemoinho conseguem empurrar)
        float diff = MoveInput * moveSpeed - rb.linearVelocity.x;
        float control = IsGrounded ? 1f : airControl;
        float limit = Mathf.Abs(MoveInput) > 0.01f ? maxMoveForce : brakeForce;
        float force = Mathf.Clamp(diff * acceleration * control, -limit, limit);
        rb.AddForce(Vector2.right * force);

        // Gravidade melhor
        if (rb.linearVelocity.y < 0f)
            rb.gravityScale = defaultGravity * fallGravityMultiplier;
        else if (rb.linearVelocity.y > 0f && !jumpHeld)
            rb.gravityScale = defaultGravity * lowJumpGravityMultiplier;
        else
            rb.gravityScale = defaultGravity;
    }

    // ---------- BOING ----------

    private void HandleJump()
    {
        squashVel += new Vector2(-jumpKick * 0.6f, jumpKick);
    }

    private void HandleLand(float impact)
    {
        impact = Mathf.Min(impact, 20f);
        squashVel += new Vector2(landKick * impact, -landKick * impact);
    }

    private void UpdateVisual()
    {
        float dt = Time.deltaTime;
        Vector2 vel = rb.linearVelocity;

        // Alvo da mola: no ar estica conforme a velocidade vertical
        Vector2 target = Vector2.zero;
        if (!IsGrounded)
        {
            float s = Mathf.Min(Mathf.Abs(vel.y) * stretchPerSpeed, maxStretch);
            target = new Vector2(-s * 0.5f, s);
        }

        // Andando: pulsa a cada passo
        float walkFactor = IsGrounded ? Mathf.Clamp01(Mathf.Abs(vel.x) / moveSpeed) : 0f;
        walkTime += dt * walkFrequency * walkFactor;
        float step = Mathf.Sin(walkTime * 2f) * walkSquash * walkFactor;
        target += new Vector2(-step * 0.5f, step);

        // Mola com overshoot (o "boing")
        Vector2 accel = (target - squash) * springStrength - squashVel * springDamping;
        squashVel += accel * dt;
        squash += squashVel * dt;

        visual.localScale = new Vector3(
            baseScale.x * (1f + squash.x),
            baseScale.y * (1f + squash.y),
            baseScale.z);

        float bob = Mathf.Abs(Mathf.Sin(walkTime)) * bobHeight * walkFactor;
        visual.localPosition = basePos + Vector3.up * bob;

        float leanTarget = Mathf.Clamp(-vel.x * 0.6f, -maxLean, maxLean);
        currentLean = Mathf.Lerp(currentLean, leanTarget, dt * leanSmooth);
        visual.localRotation = Quaternion.Euler(0f, 0f, currentLean);

        sr.flipX = Facing < 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}