using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerTopdown2D : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float deceleration = 50f;

    [Header("Visual")]
    [SerializeField] private Transform visual; // filho com o SpriteRenderer

    [Header("Mola (boing)")]
    [SerializeField] private float springStrength = 260f;
    [SerializeField] private float springDamping = 9f;   // menor = quica mais
    [SerializeField] private float startKick = 1.5f;     // esticadinha ao começar a andar
    [SerializeField] private float stopKick = 1.5f;      // amassadinha ao parar

    [Header("Andando")]
    [SerializeField] private float walkFrequency = 14f;
    [SerializeField] private float walkSquash = 0.10f;   // quanto amassa/estica a cada passo
    [SerializeField] private float bobHeight = 0.06f;
    [SerializeField] private float maxLean = 6f;         // 0 = sem inclinação
    [SerializeField] private float leanSmooth = 10f;

    [Header("Parado (respirando)")]
    [SerializeField] private float idleBreath = 0.02f;
    [SerializeField] private float idleBreathSpeed = 2f;

    public Vector2 MoveInput { get; private set; }
    public int Facing { get; private set; } = 1;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector3 basePos, baseScale;
    private Vector2 squash, squashVel;
    private float walkTime, currentLean;
    private bool wasMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Sem atrito: desliza nas paredes e canos em vez de grudar
        PhysicsMaterial2D noFriction = new PhysicsMaterial2D("NoFriction")
        {
            friction = 0f,
            bounciness = 0f
        };
        rb.sharedMaterial = noFriction;
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.sharedMaterial = noFriction;

        if (visual == null) visual = transform;
        sr = visual.GetComponentInChildren<SpriteRenderer>();
        basePos = visual.localPosition;
        baseScale = visual.localScale;
    }

    private void Update()
    {
        // Input (WASD ou setas)
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        MoveInput = Vector2.ClampMagnitude(input, 1f); // diagonal não fica mais rápida

        if (MoveInput.x > 0.01f) Facing = 1;
        else if (MoveInput.x < -0.01f) Facing = -1;

        UpdateVisual();
    }

    private void FixedUpdate()
    {
        Vector2 targetVel = MoveInput * moveSpeed;
        float rate = MoveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVel, rate * Time.fixedDeltaTime);
    }

    private void UpdateVisual()
    {
        float dt = Time.deltaTime;
        Vector2 vel = rb.linearVelocity;
        float speed01 = Mathf.Clamp01(vel.magnitude / moveSpeed);
        bool moving = MoveInput.sqrMagnitude > 0.01f;

        // Empurrãozinho na mola ao começar e ao parar
        if (moving && !wasMoving)
            squashVel += new Vector2(-startKick * 0.6f, startKick);
        else if (!moving && wasMoving)
            squashVel += new Vector2(stopKick * 0.6f, -stopKick);
        wasMoving = moving;

        // Alvo da mola: pulsa a cada passo + respira parado
        walkTime += dt * walkFrequency * speed01;
        float step = Mathf.Sin(walkTime * 2f) * walkSquash * speed01;
        float breath = Mathf.Sin(Time.time * idleBreathSpeed) * idleBreath * (1f - speed01);
        float pulse = step + breath;
        Vector2 target = new Vector2(-pulse * 0.5f, pulse);

        // Mola com overshoot (o "boing")
        Vector2 accel = (target - squash) * springStrength - squashVel * springDamping;
        squashVel += accel * dt;
        squash += squashVel * dt;

        visual.localScale = new Vector3(
            baseScale.x * (1f + squash.x),
            baseScale.y * (1f + squash.y),
            baseScale.z);

        // Quicadinha vertical e inclinação leve pro lado que anda
        float bob = Mathf.Abs(Mathf.Sin(walkTime)) * bobHeight * speed01;
        visual.localPosition = basePos + Vector3.up * bob;

        float leanTarget = Mathf.Clamp(-vel.x * 0.8f, -maxLean, maxLean);
        currentLean = Mathf.Lerp(currentLean, leanTarget, dt * leanSmooth);
        visual.localRotation = Quaternion.Euler(0f, 0f, currentLean);

        if (sr != null) sr.flipX = Facing < 0;
    }
}