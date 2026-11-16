// WindMill.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class WindMill : MonoBehaviour
{
    [Header("Empurrão")]
    [SerializeField] private Vector2 pushDirection = Vector2.left;
    [SerializeField] private float pushForce = 40f;
    [SerializeField] private string playerTag = "Player";

    [Header("Quick time (spam)")]
    [SerializeField] private KeyCode spamKey = KeyCode.E;
    [SerializeField] private float progressPerPress = 0.08f;
    [SerializeField] private float decayPerSecond = 0.35f;

    [Header("Ao desligar")]
    [SerializeField] private float offDuration = 4f;
    [SerializeField] private GameObject visual;
    [SerializeField] private bool hideVisualWhenOff = false; // deixe desmarcado: o visual não some
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject qtePrompt;

    // ---------- Bloqueio global do vento (a cutscene usa isso) ----------
    public static float WindBlockedUntil;
    public static bool IsWindBlocked => Time.time < WindBlockedUntil;
    public static void BlockWind(float seconds) => WindBlockedUntil = Time.time + seconds;
    public static void BlockWindIndefinitely() => WindBlockedUntil = float.PositiveInfinity;

    [RuntimeInitializeOnLoadMethod]
    private static void ResetBlock() => WindBlockedUntil = 0f;

    private Rigidbody2D playerRb;
    private float progress;
    private bool isOff;
    private bool wasBlocked;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    private void Start() => SetUI(false);

    private void Update()
    {
        bool blocked = IsWindBlocked;
        if (blocked != wasBlocked)
        {
            wasBlocked = blocked;
            progress = 0f;
            if (playerRb != null) SetUI(!blocked && !isOff);
        }

        if (blocked || isOff || playerRb == null) return;

        if (Input.GetKeyDown(spamKey))
            progress += progressPerPress;

        progress -= decayPerSecond * Time.deltaTime;
        progress = Mathf.Clamp01(progress);

        if (progressBar != null) progressBar.value = progress;

        if (progress >= 1f)
            TurnOff();
    }

    private void FixedUpdate()
    {
        if (IsWindBlocked || isOff || playerRb == null) return;

        float strength = 1f - progress;
        playerRb.AddForce(pushDirection.normalized * pushForce * strength);
    }

    private void TurnOff()
    {
        isOff = true;
        progress = 0f;
        if (hideVisualWhenOff && visual != null) visual.SetActive(false);
        SetUI(false);
        Invoke(nameof(TurnOn), offDuration);
    }

    private void TurnOn()
    {
        isOff = false;
        if (visual != null) visual.SetActive(true);
        if (playerRb != null && !IsWindBlocked) SetUI(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerRb = other.attachedRigidbody;
        progress = 0f;
        if (!isOff && !IsWindBlocked) SetUI(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerRb = null;
        progress = 0f;
        SetUI(false);
    }

    private void SetUI(bool show)
    {
        if (progressBar != null) progressBar.gameObject.SetActive(show);
        if (qtePrompt != null) qtePrompt.SetActive(show);
    }
}