using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SolarCycle : MonoBehaviour
{
    public static SolarCycle Instance { get; private set; }
    public static bool IsDay { get; private set; } = true;
    public static bool Dangerous { get; private set; } // noite E já passou o tempo de aviso

    [Header("Ciclo dia/noite")]
    [SerializeField] private float dayDuration = 25f;
    [SerializeField] private float nightDuration = 15f;
    [SerializeField] private float nightGrace = 1.5f;       // segundos de aviso antes do raio queimar

    [Header("Energia (a barra enche várias vezes)")]
    [SerializeField] private float energyGoal = 100f;       // tamanho da barra
    [SerializeField] private int roundsToWin = 3;           // quantas vezes encher
    [SerializeField] private float nightDrainPerSecond = 2f; // perda passiva à noite (0 = desliga)
    [SerializeField] private float roundPauseTime = 0.8f;   // barra fica cheia um instante antes de zerar
    [SerializeField] private float barSpeed = 2f;           // velocidade da animação da barra
    [SerializeField] private UnityEvent onRoundCompleted;   // toda vez que a barra enche
    [SerializeField] private UnityEvent onGoalReached;      // quando completa as 3

    [Header("Escuridão (opcional)")]
    [SerializeField] private Image darkness;
    [SerializeField, Range(0f, 1f)] private float nightDarkness = 0.55f;
    [SerializeField] private float fadeSpeed = 2f;

    [Header("UI")]
    [SerializeField] private TMP_Text phaseText;   // "DIA 18s"
    [SerializeField] private TMP_Text roundText;   // "CARGA 1/3"
    [SerializeField] private Slider energyBar;
    [SerializeField] private TMP_Text messageText;

    private float timer, sinceNight, energy, messageTimer, roundPause;
    private int round;               // quantas vezes já encheu
    private bool goalReached;

    private void Awake()
    {
        Instance = this;
        IsDay = true;
        Dangerous = false;
        timer = dayDuration;

        if (energyBar != null)
        {
            energyBar.minValue = 0f;
            energyBar.maxValue = 1f;
            energyBar.value = 0f;
        }
        if (messageText != null) SetAlpha(messageText, 0f);
        UpdateRoundText();
    }

    private void Start() => ShowMessage("Doutor Nerfario segue a Luz");

    private void Update()
    {
        float dt = Time.deltaTime;

        if (!goalReached)
        {
            timer -= dt;

            if (!IsDay)
            {
                sinceNight += dt;
                Dangerous = sinceNight >= nightGrace;

                // Perda passiva da noite
                if (nightDrainPerSecond > 0f) AddEnergy(-nightDrainPerSecond * dt);
            }

            if (timer <= 0f) SwitchPhase();

            // Depois de encher, segura a barra cheia um instante e zera
            if (roundPause > 0f)
            {
                roundPause -= dt;
                if (roundPause <= 0f) energy = 0f;
            }
        }

        // Barra com animação suave
        if (energyBar != null)
        {
            float target = energy / energyGoal;
            energyBar.value = Mathf.MoveTowards(energyBar.value, target, barSpeed * dt);
        }

        if (phaseText != null)
            phaseText.text = goalReached
                ? "COMPLETO"
                : (IsDay ? "DIA " : "NOITE ") + Mathf.CeilToInt(Mathf.Max(0f, timer)) + "s";

        if (darkness != null)
        {
            float target = (!IsDay && !goalReached) ? nightDarkness : 0f;
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

    private void SwitchPhase()
    {
        IsDay = !IsDay;
        timer = IsDay ? dayDuration : nightDuration;
        sinceNight = 0f;
        Dangerous = false;

        ShowMessage(IsDay ? "Doutor Nerfario, Vamos pegar esse raio de Luz" : "Doutor Nerfario vamos correr e roubar a lua");
    }

    // A placa chama isso (positivo = ganha, negativo = perde)
    public void AddEnergy(float amount)
    {
        if (goalReached || roundPause > 0f) return;

        energy = Mathf.Clamp(energy + amount, 0f, energyGoal);
        if (energy < energyGoal) return;

        // Barra encheu
        round++;

        if (round >= roundsToWin)
        {
            Win();
            return;
        }

        roundPause = roundPauseTime;
        UpdateRoundText();
        ShowMessage("Carga " + round + "/" + roundsToWin + " completa!");
        onRoundCompleted?.Invoke();
    }

    private void Win()
    {
        goalReached = true;
        IsDay = true;
        Dangerous = false;
        UpdateRoundText();
        ShowMessage("Energia completa!");
        onGoalReached?.Invoke();
    }

    public void ShowMessage(string text)
    {
        if (messageText == null) return;
        messageText.text = text;
        SetAlpha(messageText, 1f);
        messageTimer = 1.6f;
    }

    private void UpdateRoundText()
    {
        if (roundText == null) return;
        roundText.text = goalReached ? "COMPLETO" : "CARGA " + (round + 1) + "/" + roundsToWin;
    }

    private static void SetAlpha(TMP_Text t, float a)
    {
        Color c = t.color;
        c.a = a;
        t.color = c;
    }
}