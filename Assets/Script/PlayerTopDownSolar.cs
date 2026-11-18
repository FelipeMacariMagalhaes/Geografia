using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerTopDownSolar : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float deceleration = 50f;

    [Header("Dano (raio da noite)")]
    [SerializeField] private float knockback = 7f;
    [SerializeField] private float stunTime = 0.35f;
    [SerializeField] private float stunDeceleration = 12f;
    [SerializeField] private float blinkTime = 1.5f;
    [SerializeField] private float blinkInterval = 0.08f;

    [Header("Visual")]
    [SerializeField] private Transform visual; // filho com o SpriteRenderer do cientista

    [Header("Mola (boing)")]
    [SerializeField] private float springStrength = 260f;
    [SerializeField] private float springDamping = 9f;
    [SerializeField] private float startKick = 1.5f;
    [SerializeField] private float stopKick = 1.5f;

    [Header("Andando e parado")]
    [SerializeField] private float walkFrequency = 14f;
    [SerializeField] private float walkSquash = 0.10f;
    [SerializeField] private float bobHeight = 0.06f;
    [SerializeField] private float maxLean = 6f;
    [SerializeField] private float leanSmooth = 10f;
    [SerializeField] private float idleBreath = 0.02f;
    [SerializeField] private float idleBreathSpeed = 2f;

    public Vector2 MoveInput { get; private set; }
    public int Facing { get; private set; } = 1;
    public bool BlinkOff { get; private set; } // a placa lê isso pra piscar junto

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector3 basePos, baseScale;
    private Vector2 squash, squashVel;
    private float walkTime, currentLean, stunTimer, blinkTimer;
    private bool wasMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Sem atrito: desliza nas paredes em vez de grudar
        PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction") { friction = 0f, bounciness = 0f };
        rb.sharedMaterial = noFriction;
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.sharedMaterial = noFriction;

        if (visual == null) visual = transform;
        sr = visual.GetComponentInChildren<SpriteRenderer>();
        basePos = visual.localPosition;
        baseScale = visual.localScale;
    }

    // Chamado pela placa quando o raio da noite acerta
    public void Hit(Vector2 direction)
    {
        Vector2 dir = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.down;
        rb.linearVelocity = dir * knockback;
        stunTimer = stunTime;
        blinkTimer = blinkTime;
        squashVel += new Vector2(3f, -3f); // amassada
    }

    private void Update()
    {
        if (stunTimer > 0f) stunTimer -= Time.deltaTime;

        Vector2 input = stunTimer > 0f
            ? Vector2.zero
            : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        MoveInput = Vector2.ClampMagnitude(input, 1f);

        if (MoveInput.x > 0.01f) Facing = 1;
        else if (MoveInput.x < -0.01f) Facing = -1;

        UpdateVisual();
    }

    private void FixedUpdate()
    {
        bool stunned = stunTimer > 0f;
        Vector2 targetVel = stunned ? Vector2.zero : MoveInput * moveSpeed;
        float rate = stunned ? stunDeceleration : (MoveInput.sqrMagnitude > 0.01f ? acceleration : deceleration);
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVel, rate * Time.fixedDeltaTime);
    }

    private void UpdateVisual()
    {
        float dt = Time.deltaTime;
        Vector2 vel = rb.linearVelocity;
        float speed01 = Mathf.Clamp01(vel.magnitude / moveSpeed);
        bool moving = MoveInput.sqrMagnitude > 0.01f;

        if (moving && !wasMoving) squashVel += new Vector2(-startKick * 0.6f, startKick);
        else if (!moving && wasMoving) squashVel += new Vector2(stopKick * 0.6f, -stopKick);
        wasMoving = moving;

        walkTime += dt * walkFrequency * speed01;
        float step = Mathf.Sin(walkTime * 2f) * walkSquash * speed01;
        float breath = Mathf.Sin(Time.time * idleBreathSpeed) * idleBreath * (1f - speed01);
        float pulse = step + breath;
        Vector2 target = new Vector2(-pulse * 0.5f, pulse);

        Vector2 accel = (target - squash) * springStrength - squashVel * springDamping;
        squashVel += accel * dt;
        squash += squashVel * dt;

        visual.localScale = new Vector3(baseScale.x * (1f + squash.x), baseScale.y * (1f + squash.y), baseScale.z);

        float bob = Mathf.Abs(Mathf.Sin(walkTime)) * bobHeight * speed01;
        visual.localPosition = basePos + Vector3.up * bob;

        float leanTarget = Mathf.Clamp(-vel.x * 0.8f, -maxLean, maxLean);
        currentLean = Mathf.Lerp(currentLean, leanTarget, dt * leanSmooth);
        visual.localRotation = Quaternion.Euler(0f, 0f, currentLean);

        // Pisca depois de levar o raio
        if (blinkTimer > 0f)
        {
            blinkTimer -= dt;
            BlinkOff = Mathf.FloorToInt(blinkTimer / blinkInterval) % 2 == 0;
        }
        else BlinkOff = false;

        if (sr != null)
        {
            sr.flipX = Facing < 0;
            sr.enabled = !BlinkOff;
        }
    }
}