// MainMenu.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject creditsPanel;

    [Header("Botões")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;  

    [Header("Cena do jogo")]
    [SerializeField] private string gameSceneName = "MapaMundo";

    private void Awake()
    {
        playButton.onClick.AddListener(Play);
        creditsButton.onClick.AddListener(OpenCredits);
        quitButton.onClick.AddListener(Quit);
        backButton.onClick.AddListener(CloseCredits);
    }

    private void Start() => creditsPanel.SetActive(false);

    private void Play() => SceneManager.LoadScene(gameSceneName);

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