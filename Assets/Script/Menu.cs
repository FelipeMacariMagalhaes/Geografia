// Menu.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelEntry
{
    public string sceneName;          // nome EXATO da cena
    public Button button;             // botão da fase
    public GameObject completedMark;  // "✔" (opcional)
}

public class Menu : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject levelsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Botões do menu")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Botões de voltar")]
    [SerializeField] private Button backButton;        // dentro dos créditos
    [SerializeField] private Button levelsBackButton;  // dentro do painel de fases

    [Header("Fases")]
    [SerializeField] private LevelEntry[] levels;      // as 4 fases
    [SerializeField] private bool lockCompleted = true; // fase concluída não abre mais
    [SerializeField] private TMP_Text progressText;    // "Concluídas: 2/4" (opcional)
    [SerializeField] private Button resetButton;       // zera o progresso (opcional, pra testes)

    private void Awake()
    {
        playButton.onClick.AddListener(OpenLevels);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(Quit);
        backButton.onClick.AddListener(CloseCredits);
        levelsBackButton.onClick.AddListener(CloseLevels);
        if (resetButton != null) resetButton.onClick.AddListener(ResetProgress);

        foreach (LevelEntry level in levels)
        {
            string scene = level.sceneName;
            level.button.onClick.AddListener(() => LoadLevel(scene));
        }
    }

    private void Start()
    {
        levelsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        RefreshLevels();

        // Zerou as 4 fases: abre os créditos uma vez
        if (AllCompleted() && !LevelProgress.FinalShown)
        {
            LevelProgress.FinalShown = true;
            OpenCredits();
        }
    }

    // ---------- Fases ----------

    private void OpenLevels()
    {
        RefreshLevels();
        levelsPanel.SetActive(true);
    }

    private void CloseLevels() => levelsPanel.SetActive(false);

    private void LoadLevel(string sceneName)
    {
        if (lockCompleted && LevelProgress.IsCompleted(sceneName)) return;
        SceneManager.LoadScene(sceneName);
    }

    private void RefreshLevels()
    {
        int done = 0;

        foreach (LevelEntry level in levels)
        {
            bool completed = LevelProgress.IsCompleted(level.sceneName);
            if (completed) done++;

            // Concluída = botão travado (não dá pra voltar)
            level.button.interactable = !(completed && lockCompleted);

            if (level.completedMark != null)
                level.completedMark.SetActive(completed);
        }

        if (progressText != null)
            progressText.text = "Concluídas: " + done + "/" + levels.Length;
    }

    private bool AllCompleted()
    {
        if (levels.Length == 0) return false;

        foreach (LevelEntry level in levels)
            if (!LevelProgress.IsCompleted(level.sceneName)) return false;

        return true;
    }

    // Clique com o botão direito no componente > "Resetar progresso"
    [ContextMenu("Resetar progresso")]
    public void ResetProgress()
    {
        string[] scenes = new string[levels.Length];
        for (int i = 0; i < levels.Length; i++)
            scenes[i] = levels[i].sceneName;

        LevelProgress.ResetAll(scenes);

        if (Application.isPlaying) RefreshLevels();
    }

    // ---------- Créditos e sair ----------

    private void OpenCredits() => creditsPanel.SetActive(true);

    private void CloseCredits() => creditsPanel.SetActive(false);

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}