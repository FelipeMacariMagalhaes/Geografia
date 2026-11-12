// WhirlwindQTE.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class WindMill : MonoBehaviour
{
    [Header("Empurrão")]
    [SerializeField] private Vector2 pushDirection = Vector2.left; // pra onde ele joga o player
    [SerializeField] private float pushForce = 40f;
    [SerializeField] private string playerTag = "Player";

    [Header("Quick time (spam)")]
    [SerializeField] private KeyCode spamKey = KeyCode.E;
    [SerializeField] private float progressPerPress = 0.08f; // 0 a 1 (quanto cada clique enche)
    [SerializeField] private float decayPerSecond = 0.35f;   // quanto esvazia por segundo se parar

    [Header("Ao desligar")]
    [SerializeField] private float offDuration = 4f;
    [SerializeField] private GameObject visual;       // sprite/animação do redemoinho
    [SerializeField] private Slider progressBar;      // opcional
    [SerializeField] private GameObject qtePrompt;    // opcional ("SPAM E!")

    private Rigidbody2D playerRb;
    private float progress;
    private bool isOff;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    private void Start() => SetUI(false);

    private void Update()
    {
        if (isOff || playerRb == null) return;

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
        if (isOff || playerRb == null) return;

        // Quanto mais cheia a barra, mais fraco o empurrão
        float strength = 1f - progress;
        playerRb.AddForce(pushDirection.normalized * pushForce * strength);
    }

    private void TurnOff()
    {
        isOff = true;
        progress = 0f;
        if (visual != null) visual.SetActive(false);
        SetUI(false);
        Invoke(nameof(TurnOn), offDuration);
    }

    private void TurnOn()
    {
        isOff = false;
        if (visual != null) visual.SetActive(true);
        if (playerRb != null) SetUI(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerRb = other.attachedRigidbody;
        progress = 0f;
        if (!isOff) SetUI(true);
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