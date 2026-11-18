using UnityEngine;

public class PowerHouse : MonoBehaviour
{
    [SerializeField] private Color offColor = new Color(0.25f, 0.25f, 0.35f, 1f);
    [SerializeField] private Color onColor = Color.white;
    [SerializeField] private GameObject glow;       
    [SerializeField] private float colorSpeed = 4f;

    public bool IsPowered => powerCount > 0;

    private int powerCount;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = offColor;
        if (glow != null) glow.SetActive(false);
    }

    // Chamado pelo disjuntor: true = ligou, false = caiu
    public void Power(bool on)
    {
        powerCount = Mathf.Max(0, powerCount + (on ? 1 : -1));
        if (glow != null) glow.SetActive(IsPowered);
    }

    private void Update()
    {
        if (sr == null) return;
        Color target = IsPowered ? onColor : offColor;
        sr.color = Color.Lerp(sr.color, target, Time.deltaTime * colorSpeed);
    }
}