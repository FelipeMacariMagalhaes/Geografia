using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DirtCleaner : MonoBehaviour
{
    [Header("Clique")]
    [SerializeField] private Camera cam;              // vazio = Main Camera
    [SerializeField] private float pullRange = 2.5f;  // distância máxima pra arrancar

    [Header("Animação do player (puxão)")]
    [SerializeField] private Transform visual;        // filho com o SpriteRenderer
    [SerializeField] private float pullDuration = 0.18f;
    [SerializeField] private float pullStretch = 0.25f;
    [SerializeField] private float pullLean = 12f;

    [Header("Textos (TextMeshPro)")]
    [SerializeField] private TMP_Text counterText;    // "Sujeira: 0/10"
    [SerializeField] private TMP_Text messageText;    // mensagens que somem

    [Header("Quando limpar tudo")]
    [SerializeField] private UnityEvent onAllCleaned;

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private float pullTimer, pullDirX = 1f;
    private int cleaned, total;
    private Coroutine messageRoutine;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (visual == null) visual = transform;
        sr = visual.GetComponentInChildren<SpriteRenderer>();
        baseScale = visual.localScale;
    }

    private void Start()
    {
        total = FindObjectsByType<Dirt>(FindObjectsSortMode.None).Length;
        UpdateCounter();
        if (messageText != null) SetMessageAlpha(0f);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // Acha a sujeira sob o mouse
        Vector2 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        Dirt dirt = null;
        foreach (Collider2D c in Physics2D.OverlapPointAll(mouse))
        {
            if (c.TryGetComponent(out Dirt d) && !d.IsRemoved) { dirt = d; break; }
        }
        if (dirt == null) return;

        // Muito longe?
        if (Vector2.Distance(transform.position, dirt.transform.position) > pullRange)
        {
            ShowMessage("Chegue mais perto!");
            return;
        }

        // Puxão do player
        pullDirX = Mathf.Sign(dirt.transform.position.x - transform.position.x);
        pullTimer = pullDuration;

        bool removed = dirt.Pull(transform);

        if (removed)
        {
            cleaned++;
            UpdateCounter();

            if (cleaned >= total)
            {
                ShowMessage("Água limpa! Hidrelétrica liberada!");
                onAllCleaned?.Invoke();
            }
            else
            {
                ShowMessage("Sujeira removida!");
            }
        }
        else
        {
            ShowMessage("Arrancando... faltam " + dirt.ClicksLeft);
        }
    }

    // Roda depois do controller, então a animação do puxão tem prioridade
    private void LateUpdate()
    {
        if (pullTimer <= 0f) return;

        pullTimer -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(pullTimer / pullDuration);
        float k = Mathf.Sin(t * Mathf.PI); // vai de 0 a 1 e volta a 0

        visual.localScale = new Vector3(
            baseScale.x * (1f + pullStretch * k),
            baseScale.y * (1f - pullStretch * 0.6f * k),
            baseScale.z);
        visual.localRotation = Quaternion.Euler(0f, 0f, -pullDirX * pullLean * k);

        if (sr != null) sr.flipX = pullDirX < 0f;
    }

    private void UpdateCounter()
    {
        if (counterText != null)
            counterText.text = "Sujeira: " + cleaned + "/" + total;
    }

    private void ShowMessage(string text)
    {
        if (messageText == null) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(text));
    }

    private IEnumerator MessageRoutine(string text)
    {
        messageText.text = text;
        SetMessageAlpha(1f);
        yield return new WaitForSeconds(1f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.4f;
            SetMessageAlpha(1f - t);
            yield return null;
        }
        SetMessageAlpha(0f);
    }

    private void SetMessageAlpha(float a)
    {
        Color c = messageText.color;
        c.a = a;
        messageText.color = c;
    }
}