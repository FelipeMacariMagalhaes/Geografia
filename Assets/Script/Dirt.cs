using System.Collections;
using UnityEngine;

public class Dirt : MonoBehaviour
{
    [SerializeField] private int clicksToRemove = 5;
    [SerializeField] private float shakeAmount = 0.06f;
    [SerializeField] private float stretchPerClick = 0.08f;  
    [SerializeField] private float popDuration = 0.35f;

    public bool IsRemoved { get; private set; }
    public int ClicksLeft => Mathf.Max(0, clicksToRemove - clicks);

    private int clicks;
    private float shakeTimer;
    private Vector3 basePos, baseScale;

    private void Awake()
    {
        basePos = transform.position;
        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (IsRemoved) return;

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            transform.position = basePos + (Vector3)(Random.insideUnitCircle * shakeAmount);
        }
        else
        {
            transform.position = basePos;
        }
    }

    // Retorna true quando a sujeira foi arrancada
    public bool Pull(Transform puller)
    {
        if (IsRemoved) return false;

        clicks++;
        shakeTimer = 0.15f;
        transform.localScale = baseScale * (1f + stretchPerClick * clicks);

        if (clicks >= clicksToRemove)
        {
            IsRemoved = true;
            StartCoroutine(Pop(puller));
            return true;
        }
        return false;
    }

    private IEnumerator Pop(Transform puller)
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Vector3 start = basePos;
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / popDuration;
            float e = t * t; // acelera no fim, como se fosse sugada

            Vector3 target = puller != null ? puller.position : basePos + Vector3.up;
            transform.position = Vector3.Lerp(start, target, e);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, e);
            yield return null;
        }

        Destroy(gameObject);
    }
}