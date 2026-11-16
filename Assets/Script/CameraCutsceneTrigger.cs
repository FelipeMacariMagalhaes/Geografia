using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraCutsceneTrigger : MonoBehaviour
{
    [Header("Câmera da cutscene")]
    [SerializeField] private GameObject cutsceneCamera;
    [SerializeField] private Transform cameraTarget;

    [Header("Pontos (ordem que a câmera passa)")]
    [SerializeField] private Transform[] waypoints;

    [Header("Tempos")]
    [SerializeField] private float blendInTime = 1.5f;
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

        if (waypoints == null || waypoints.Length == 0 ||
            cutsceneCamera == null || cameraTarget == null || player == null)
        {
            Debug.LogError("CameraCutsceneTrigger: falta preencher algum campo no Inspector.");
            return;
        }

        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        playing = true;
        played = true;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        player.enabled = false;
        rb.linearVelocity = Vector2.zero;

        cameraTarget.position = waypoints[0].position;
        cutsceneCamera.SetActive(true);
        yield return new WaitForSeconds(blendInTime);

        for (int i = 0; i < waypoints.Length; i++)
        {
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

        cutsceneCamera.SetActive(false);
        yield return new WaitForSeconds(blendOutTime);

        player.enabled = true;
        playing = false;
    }
}