using System.Collections.Generic;
using UnityEngine;

public class LightBeam : MonoBehaviour
{
    public static readonly List<LightBeam> All = new List<LightBeam>();

    [Header("Trajeto")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float daySpeed = 2f;
    [SerializeField] private float nightSpeed = 4f;
    [SerializeField] private bool pingPong = true; // vai e volta (desmarque pra fazer loop)

    [Header("Tamanho")]
    [SerializeField] private float radius = 1.5f;

    [Header("Cores")]
    [SerializeField] private Color dayColor = new Color(1f, 0.92f, 0.3f, 0.45f);
    [SerializeField] private Color nightColor = new Color(1f, 0.2f, 0.2f, 0.55f);

    private SpriteRenderer sr;
    private Vector3[] points;
    private int index, dir = 1;

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        // Guarda as posições dos pontos ANTES de mover o feixe
        int n = waypoints != null ? waypoints.Length : 0;
        points = new Vector3[n];
        for (int i = 0; i < n; i++)
            points[i] = waypoints[i] != null ? waypoints[i].position : transform.position;

        if (n < 2)
            Debug.LogWarning("LightBeam: coloque 2 ou mais Waypoints pro feixe andar.", this);

        // Ajusta o tamanho do sprite pro raio escolhido
        float size = sr.sprite != null ? sr.sprite.bounds.size.x : 1f;
        transform.localScale = Vector3.one * (radius * 2f / size);

        if (n > 0)
        {
            transform.position = new Vector3(points[0].x, points[0].y, transform.position.z);
            index = n > 1 ? 1 : 0;
        }
    }

    private void Update()
    {
        Move();
        UpdateColor();
    }

    private void Move()
    {
        if (points.Length < 2) return;

        float speed = SolarCycle.IsDay ? daySpeed : nightSpeed;
        Vector3 target = new Vector3(points[index].x, points[index].y, transform.position.z);

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude < 0.0001f)
            NextWaypoint();
    }

    private void NextWaypoint()
    {
        if (pingPong)
        {
            if (index + dir >= points.Length || index + dir < 0) dir = -dir;
            index += dir;
        }
        else
        {
            index = (index + 1) % points.Length;
        }
    }

    private void UpdateColor()
    {
        Color target = SolarCycle.IsDay ? dayColor : nightColor;

        // Na noite, pisca enquanto ainda não queima (tempo de fugir)
        if (!SolarCycle.IsDay && !SolarCycle.Dangerous)
            target.a *= 0.3f + 0.7f * Mathf.PingPong(Time.time * 6f, 1f);

        sr.color = Color.Lerp(sr.color, target, Time.deltaTime * 8f);
    }

    public bool Contains(Vector2 p, float margin = 0f)
    {
        return Vector2.Distance(p, transform.position) <= radius + margin;
    }

    public static LightBeam FindOver(Vector2 p, float margin)
    {
        foreach (LightBeam b in All)
            if (b.Contains(p, margin)) return b;
        return null;
    }

    public static LightBeam Nearest(Vector2 p)
    {
        LightBeam best = null;
        float bestDist = float.MaxValue;
        foreach (LightBeam b in All)
        {
            float d = Vector2.Distance(p, b.transform.position);
            if (d < bestDist) { bestDist = d; best = b; }
        }
        return best;
    }

    // Mostra o raio (amarelo) e o trajeto (ciano) na Scene
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);

        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}