using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "Menu";    
    [SerializeField] private float delay = 2.5f;              

    [Header("Opcional: concluir ao encostar neste objeto")]
    [SerializeField] private bool completeOnTrigger = false;
    [SerializeField] private string playerTag = "Player";

    private bool done;

    // Ligue nos eventos de vitória de cada fase
    public void Complete()
    {
        if (done) return;
        done = true;

        LevelProgress.MarkCompleted(SceneManager.GetActiveScene().name);
        StartCoroutine(GoToMenu());
    }

    private IEnumerator GoToMenu()
    {
        yield return new WaitForSecondsRealtime(delay);
        SceneManager.LoadScene(menuSceneName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completeOnTrigger && other.CompareTag(playerTag))
            Complete();
    }
}