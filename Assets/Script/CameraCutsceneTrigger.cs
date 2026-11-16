using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraCutsceneTrigger : MonoBehaviour
{
    [Header("Câmera da cutscene")]
    [SerializeField] private GameObject cutsceneCamera;  // vcam extra (começa desligada)
    [SerializeField] private Transform cameraTarget;     // objeto vazio que a vcam segue

    [Header("Pontos (de baixo pra cima)")]
    [SerializeField] private Transform[] waypoints;      // ex.: Gerador1, Gerador2, Gerador3

    [Header("Tempos")]
    [SerializeField] private float blendInTime = 1.5f;   // igual ao Default Blend do Brain
    [SerializeField] private float moveTimePerPoint = 2f;
    [SerializeField] private float holdTimePerPoint = 1f;
    [SerializeField] private float blendOutTime = 1.5f;

    [Header("Player")]
    [SerializeField] private PlayerController2D player;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool playOnce = true;

    private bool playing, played;

    private void Reset() => GetComponent<Collider2D>().isTrigger = true;

    private void Start()
    {
        if (cutsceneCamera != null) cutsceneCamera.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (playing || (playOnce && played)) return;
        if (waypoints == null || waypoints.Length == 0) return;

        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        playing = true;
        played = true;

        // Trava o player durante a cutscene
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        player.enabled = false;
        rb.linearVelocity = Vector2.zero;

        // Posiciona o alvo no primeiro ponto ANTES de ligar a câmera (evita pulo)
        cameraTarget.position = waypoints[0].position;
        cutsceneCamera.SetActive(true);
        yield return new WaitForSeconds(blendInTime);

        for (int i = 0; i < waypoints.Length; i++)
        {
            // Sobe do ponto anterior até este
            if (i > 0)
            {
                Vector3 from = waypoints[i - 1].position;
                Vector3 to = waypoints[i].position;
                float t = 0f;

                while (t < 1f)
                {
                    t += Time.deltaTime / moveTimePerPoint;
                    cameraTarget.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                    yield return null;
                }
            }

            yield return new WaitForSeconds(holdTimePerPoint);
        }

        // Volta pra câmera do player
        cutsceneCamera.SetActive(false);
        yield return new WaitForSeconds(blendOutTime);

        player.enabled = true;
        playing = false;
    }
}