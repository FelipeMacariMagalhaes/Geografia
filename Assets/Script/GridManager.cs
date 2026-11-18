using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Tempo")]
    [SerializeField] private float timeLimit = 90f;

    [Header("Disjuntores (vazio = acha todos sozinho)")]
    [SerializeField] private Breaker[] breakers;

    [Header("Sobrecarga (derruba um disjuntor já consertado)")]
    [SerializeField] private float firstOverloadDelay = 15f;
    [SerializeField] private float overloadInterval = 20f;   // 0 = desliga

    [Header("Escuridão")]
    [SerializeField] private Image darkness;                 // Image preta em tela cheia
    [SerializeField, Range(0f, 1f)] private float maxDarkness = 0.75f;
    [SerializeField] private float fadeSpeed = 1.5f;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text breakersText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Slider holdBar;                 // barra de segurar E
    [SerializeField] private TMP_Text promptText;            // "Segure E para consertar"

    [Header("Fim")]
    [SerializeField] private bool reloadOnFail = true;
    [SerializeField] private float reloadDelay = 2.5f;
    [SerializeField] private UnityEvent onGridRestored;      // vitória
    [SerializeField] private UnityEvent onBlackout;          // derrota

    private enum State { Playing, Won, Lost }
    private State state = State.Playing;

    private float timeLeft, nextOverload, messageTimer;
    private Color timerColor = Color.white;

    private void Awake()
    {
        Instance = this;
        timeLeft = timeLimit;
        nextOverload = firstOverloadDelay;

        if (breakers == null || breakers.Length == 0)
            breakers = FindObjectsByType<Breaker>(FindObjectsSortMode.None);

        if (holdBar != null)
        {
            holdBar.minValue = 0f;
            holdBar.maxValue = 1f;
        }
        if (timerText != null) timerColor = timerText.color;
        if (messageText != null) SetAlpha(messageText, 0f);

        SetHold(false);
    }

    private void Start()
    {
        UpdateBreakersText();
        ShowMessage("Apagão! Conserte os disjuntores!");
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (state == State.Playing)
        {
            timeLeft -= dt;
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                Lose();
            }

            if (overloadInterval > 0f)
            {
                nextOverload -= dt;
                if (nextOverload <= 0f)
                {
                    Overload();
                    nextOverload = overloadInterval;
                }
            }
        }

        UpdateUI(dt);
    }

    // Os disjuntores avisam aqui quando consertam ou caem
    public void OnBreakerChanged(bool repaired)
    {
        int fixedCount = CountFixed();
        UpdateBreakersText();

        if (state != State.Playing) return;

        if (breakers.Length > 0 && fixedCount >= breakers.Length)
        {
            Win();
            return;
        }

        if (repaired)
            ShowMessage("Disjuntor consertado! " + fixedCount + "/" + breakers.Length);
    }

    private void Overload()
    {
        List<Breaker> fixedOnes = new List<Breaker>();
        foreach (Breaker b in breakers)
            if (b != null && b.IsFixed) fixedOnes.Add(b);

        if (fixedOnes.Count == 0) return;

        fixedOnes[Random.Range(0, fixedOnes.Count)].Break();
        ShowMessage("Sobrecarga! Um disjuntor caiu!");
    }

    private void Win()
    {
        state = State.Won;
        SetHold(false);
        ShowMessage("Rede restabelecida!", 3f);
        onGridRestored?.Invoke();
    }

    private void Lose()
    {
        state = State.Lost;
        SetHold(false);
        ShowMessage("APAGÃO TOTAL!", reloadDelay);
        onBlackout?.Invoke();

        if (reloadOnFail) Invoke(nameof(Reload), reloadDelay);
    }

    private void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private int CountFixed()
    {
        int n = 0;
        foreach (Breaker b in breakers)
            if (b != null && b.IsFixed) n++;
        return n;
    }

    // ---------- UI ----------

    public void SetHold(bool show, float progress = 0f)
    {
        if (holdBar != null)
        {
            holdBar.gameObject.SetActive(show);
            holdBar.value = progress;
        }
        if (promptText != null) promptText.gameObject.SetActive(show);
    }

    public void ShowMessage(string text, float duration = 1.6f)
    {
        if (messageText == null) return;
        messageText.text = text;
        SetAlpha(messageText, 1f);
        messageTimer = duration;
    }

    private void UpdateBreakersText()
    {
        if (breakersText != null)
            breakersText.text = "Disjuntores: " + CountFixed() + "/" + breakers.Length;
    }

    private void UpdateUI(float dt)
    {
        if (timerText != null)
        {
            int t = Mathf.CeilToInt(timeLeft);
            timerText.text = (t / 60).ToString("00") + ":" + (t % 60).ToString("00");
            timerText.color = (timeLeft <= 10f && state == State.Playing) ? Color.red : timerColor;
        }

        // Quanto mais disjuntores consertados, mais claro fica
        if (darkness != null)
        {
            float ratio = breakers.Length > 0 ? CountFixed() / (float)breakers.Length : 1f;
            float target = state == State.Lost ? 1f : maxDarkness * (1f - ratio);

            Color c = darkness.color;
            c.a = Mathf.MoveTowards(c.a, target, fadeSpeed * dt);
            darkness.color = c;
        }

        if (messageText != null && messageTimer > 0f)
        {
            messageTimer -= dt;
            SetAlpha(messageText, Mathf.Clamp01(messageTimer / 0.4f));
        }
    }

    private static void SetAlpha(TMP_Text t, float a)
    {
        Color c = t.color;
        c.a = a;
        t.color = c;
    }
}