using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Breaker : MonoBehaviour
{
    [Header("Conserto")]
    [SerializeField] private KeyCode repairKey = KeyCode.E;
    [SerializeField] private float repairTime = 2f;       // segundos segurando
    [SerializeField] private string playerTag = "Player";

    [Header("Casas ligadas a este disjuntor")]
    [SerializeField] private PowerHouse[] houses;

    [Header("Visual")]
    [SerializeField] private Color brokenColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color fixedColor = new Color(0.4f, 1f, 0.5f);
    [SerializeField] private GameObject sparks;           // faísca (ativa enquanto quebrado)

    public bool IsFixed { get; private set; }

    private SpriteRenderer sr;
    private bool playerInside;
    private float progress;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplyVisual();
    }

    private void Update()
    {
        if (IsFixed) return;

        // Pisca enquanto está quebrado
        if (sr != null)
            sr.color = Color.Lerp(brokenColor, Color.white, Mathf.PingPong(Time.time * 3f, 1f) * 0.6f);

        if (!playerInside) return;

        if (Input.GetKey(repairKey))
        {
            progress += Time.deltaTime / repairTime;
            if (progress >= 1f)
            {
                Repair();
                return;
            }
        }
        else
        {
            // Soltou: a barra vai esvaziando
            progress = Mathf.Max(0f, progress - Time.deltaTime * 2f / repairTime);
        }

        if (GridManager.Instance != null)
            GridManager.Instance.SetHold(true, progress);
    }

    private void Repair()
    {
        IsFixed = true;
        progress = 0f;

        foreach (PowerHouse h in houses)
            if (h != null) h.Power(true);

        ApplyVisual();

        if (GridManager.Instance != null)
        {
            GridManager.Instance.SetHold(false);
            GridManager.Instance.OnBreakerChanged(true);
        }
    }

    // Chamado pelo GridManager na sobrecarga
    public void Break()
    {
        if (!IsFixed) return;

        IsFixed = false;
        progress = 0f;

        foreach (PowerHouse h in houses)
            if (h != null) h.Power(false);

        ApplyVisual();

        if (GridManager.Instance != null)
            GridManager.Instance.OnBreakerChanged(false);
    }

    private void ApplyVisual()
    {
        if (sr != null) sr.color = IsFixed ? fixedColor : brokenColor;
        if (sparks != null) sparks.SetActive(!IsFixed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = true;
        if (GridManager.Instance != null)
            GridManager.Instance.SetHold(!IsFixed, progress);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;
        progress = 0f;
        if (GridManager.Instance != null)
            GridManager.Instance.SetHold(false);
    }
}