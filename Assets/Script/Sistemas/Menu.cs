
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using PixeLadder.EasyTransition;
using PixeLadder.EasyTransition.Demo;

[System.Serializable]
public class LevelEntry
{
    public string sceneName;
    public Button button;
    public GameObject completedMark;
}

public class Menu : MonoBehaviour
{
    [Header("Transição")]
    [SerializeField] private TransitionEffect levelTransition;
    [SerializeField] private float transitionDuration = 1f;

    private bool isTransitioning;

    [Header("Painéis")]
    [SerializeField] private GameObject levelsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Botões do menu")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Botões de voltar")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button levelsBackButton;

    [Header("Fases")]
    [SerializeField] private LevelEntry[] levels;
    [SerializeField] private bool lockCompleted = true;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Button resetButton;

    private void Awake()
    {
        playButton.onClick.AddListener(OpenLevels);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(Quit);
        backButton.onClick.AddListener(CloseCredits);
        levelsBackButton.onClick.AddListener(CloseLevels);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetProgress);

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

        if (AllCompleted() && !LevelProgress.FinalShown)
        {
            LevelProgress.FinalShown = true;
            OpenCredits();
        }
    }

    private void OpenLevels()
    {
        RefreshLevels();
        levelsPanel.SetActive(true);
    }

    private void CloseLevels()
    {
        levelsPanel.SetActive(false);
    }

    private void LoadLevel(string sceneName)
    {
        if (isTransitioning)
            return;

        if (lockCompleted && LevelProgress.IsCompleted(sceneName))
            return;

        StartCoroutine(LoadLevelRoutine(sceneName));
    }


    private IEnumerator LoadLevelRoutine(string sceneName)
    {
        isTransitioning = true;

        PlayTransition(sceneName);

        yield return null;
    }


    private void PlayTransition(string sceneName)
    {
        if (levelTransition == null)
        {
            Debug.LogWarning("[Easy Transition] Nenhum efeito foi atribuído.");
            return;
        }

        if (SceneTransitioner.Instance == null)
        {
            Debug.LogWarning("[Easy Transition] SceneTransitioner não encontrado.");
            return;
        }

        if (SceneTransitioner.Instance.IsTransitioning)
            return;

        TransitionEffect clonedEffect = Instantiate(levelTransition);
        clonedEffect.name = levelTransition.name + "_Clone";

        clonedEffect.Duration = transitionDuration;

        SceneTransitioner.Instance.LoadScene(sceneName, clonedEffect);
    }

    private void RefreshLevels()
    {
        int done = 0;

        foreach (LevelEntry level in levels)
        {
            bool completed = LevelProgress.IsCompleted(level.sceneName);

            if (completed)
                done++;

            level.button.interactable = !(completed && lockCompleted);

            if (level.completedMark != null)
                level.completedMark.SetActive(completed);
        }

        if (progressText != null)
            progressText.text = "Concluídas: " + done + "/" + levels.Length;
    }

    private bool AllCompleted()
    {
        if (levels.Length == 0)
            return false;

        foreach (LevelEntry level in levels)
        {
            if (!LevelProgress.IsCompleted(level.sceneName))
                return false;
        }

        return true;
    }

    [ContextMenu("Resetar progresso")]
    public void ResetProgress()
    {
        string[] scenes = new string[levels.Length];

        for (int i = 0; i < levels.Length; i++)
            scenes[i] = levels[i].sceneName;

        LevelProgress.ResetAll(scenes);

        if (Application.isPlaying)
            RefreshLevels();
    }

    private void OpenCredits()
    {
        creditsPanel.SetActive(true);
    }

    private void CloseCredits()
    {
        creditsPanel.SetActive(false);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
