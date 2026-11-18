using UnityEngine;

public class SolarPanel : MonoBehaviour
{
    [Header("Carregar (dia)")]
    [SerializeField] private float chargePerSecond = 12f;
    [SerializeField] private float decayPerSecond = 0f;
    [SerializeField] private float panelRadius = 0.3f;     // área da placa que encosta no feixe

    [Header("Dano (noite)")]
    [SerializeField] private float nightPenalty = 20f;
    [SerializeField] private float invulnerableTime = 1.5f;

    [Header("Posição (gira em volta do player, sem rotacionar o sprite)")]
    [SerializeField] private SpriteRenderer playerSprite;  // arraste o SpriteRenderer do Visual
    [SerializeField] private bool autoDistance = true;     // calcula a distância pelo tamanho dos sprites
    [SerializeField] private float gap = 0.1f;             // espacinho entre a placa e o corpo
    [SerializeField] private float orbitSpeed = 10f;
    [SerializeField] private bool flipWhenLeft = true;     // espelha o sprite quando vai pra esquerda

    [Header("Distância manual (só se Auto Distance estiver desligado)")]
    [SerializeField] private float orbitRadiusX = 1f;
    [SerializeField] private float orbitRadiusUp = 0.8f;
    [SerializeField] private float orbitRadiusDown = 1f;
    [SerializeField] private Vector2 orbitCenterOffset = new Vector2(0f, 0.2f);

    [Header("Visual")]
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color chargingColor = new Color(1f, 0.95f, 0.4f);
    [SerializeField] private Color dangerColor = new Color(1f, 0.35f, 0.35f);

    private PlayerTopDownSolar player;
    private SpriteRenderer sr;
    private Vector3 baseScale;
    private Vector2 lastDir = Vector2.right;
    private float currentAngle;
    private float invulnerableTimer;
    private bool facingLeft;

    // Valores usados de verdade (automáticos ou manuais)
    private float rx, rUp, rDown;
    private Vector2 centerOffset;

    private void Awake()
    {
        player = GetComponentInParent<PlayerTopDownSolar>();
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    private void Start()
    {
        rx = orbitRadiusX;
        rUp = orbitRadiusUp;
        rDown = orbitRadiusDown;
        centerOffset = orbitCenterOffset;

        if (autoDistance && playerSprite != null)
        {
            Bounds pb = playerSprite.bounds;
            Bounds b = sr.bounds;
            Transform pivot = player != null ? player.transform : transform.parent;

            rx = pb.extents.x + b.extents.x + gap;                 // ao lado: não encosta no corpo
            rUp = pb.extents.y * 0.5f + b.extents.y * 0.5f;        // em cima: fica atrás, pode sobrepor
            rDown = pb.extents.y + b.extents.y + gap;              // embaixo: não cobre o corpo
            if (pivot != null) centerOffset = (Vector2)(pb.center - pivot.position);
        }

        UpdateOrbit(1f);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        invulnerableTimer -= dt;

        UpdateOrbit(dt);

        LightBeam beam = LightBeam.FindOver(transform.position, panelRadius);
        bool inBeam = beam != null;
        bool charging = false;

        if (SolarCycle.IsDay)
        {
            if (inBeam)
            {
                charging = true;
                AddEnergy(chargePerSecond * dt);
            }
            else if (decayPerSecond > 0f)
            {
                AddEnergy(-decayPerSecond * dt);
            }
        }
        else if (inBeam && SolarCycle.Dangerous && invulnerableTimer <= 0f)
        {
            TakeHit(beam);
        }

        UpdateVisual(dt, charging);
    }

    private void UpdateOrbit(float dt)
    {
        Transform pivot = player != null ? player.transform : transform.parent;
        if (pivot == null) return;

        if (player != null && player.MoveInput.sqrMagnitude > 0.01f)
            lastDir = player.MoveInput.normalized;

        float targetAngle = Mathf.Atan2(lastDir.y, lastDir.x) * Mathf.Rad2Deg;
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, dt * orbitSpeed);

        float rad = currentAngle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        float ry = sin > 0f ? rUp : rDown;

        // Órbita oval em volta do corpo
        Vector3 center = pivot.position + (Vector3)centerOffset;
        transform.position = center + new Vector3(cos * rx, sin * ry, 0f);
        transform.rotation = Quaternion.identity; // NÃO gira o sprite (pixel art fica limpo)

        // Espelha o sprite quando está do lado esquerdo
        if (flipWhenLeft)
        {
            if (cos < -0.2f) facingLeft = true;
            else if (cos > 0.2f) facingLeft = false;
            sr.flipX = facingLeft;
        }

        // Em cima do player = atrás dele; nos outros lados = na frente
        if (playerSprite != null)
            sr.sortingOrder = playerSprite.sortingOrder + (sin > 0.3f ? -1 : 1);
    }

    private void TakeHit(LightBeam beam)
    {
        invulnerableTimer = invulnerableTime;
        AddEnergy(-nightPenalty);

        if (SolarCycle.Instance != null)
            SolarCycle.Instance.ShowMessage("Queimou! -" + nightPenalty);

        if (player != null)
            player.Hit(transform.position - beam.transform.position);
    }

    private void AddEnergy(float amount)
    {
        if (SolarCycle.Instance != null) SolarCycle.Instance.AddEnergy(amount);
    }

    private void UpdateVisual(float dt, bool charging)
    {
        // Pulsa quando está carregando
        float pulse = charging ? 1f + Mathf.Sin(Time.time * 14f) * 0.08f : 1f;
        transform.localScale = baseScale * pulse;

        // Cor: amarela carregando, vermelha ao levar dano
        Color target = idleColor;
        if (charging) target = chargingColor;
        else if (invulnerableTimer > 0f) target = dangerColor;
        sr.color = Color.Lerp(sr.color, target, dt * 12f);

        // Pisca junto com o player
        sr.enabled = player == null || !player.BlinkOff;
    }

    // Ciano = caminho da placa | Verde = área que carrega
    private void OnDrawGizmosSelected()
    {
        Transform pivot = transform.parent;
        if (pivot == null) return;

        bool playing = Application.isPlaying;
        float gx = playing ? rx : orbitRadiusX;
        float gUp = playing ? rUp : orbitRadiusUp;
        float gDown = playing ? rDown : orbitRadiusDown;
        Vector2 off = playing ? centerOffset : orbitCenterOffset;

        Vector3 center = pivot.position + (Vector3)off;
        Gizmos.color = Color.cyan;
        Vector3 prev = Vector3.zero;
        for (int i = 0; i <= 32; i++)
        {
            float a = i / 32f * Mathf.PI * 2f;
            float s = Mathf.Sin(a);
            Vector3 p = center + new Vector3(Mathf.Cos(a) * gx, s * (s > 0f ? gUp : gDown), 0f);
            if (i > 0) Gizmos.DrawLine(prev, p);
            prev = p;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, panelRadius);
    }
}