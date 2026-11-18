
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TreeAnimation2D : MonoBehaviour
{
    [Header("Respiração")]
    [SerializeField] private float breathAmount = 0.025f;
    [SerializeField] private float breathSpeed = 1.5f;

    [Header("Mola")]
    [SerializeField] private float springStrength = 180f;
    [SerializeField] private float springDamping = 8f;

    [Header("Boing")]
    [SerializeField] private float squashAmount = 0.15f;

    [Header("Movimento")]
    [SerializeField] private float bobAmount = 0.01f;
    [SerializeField] private float bobSpeed = 1.5f;

    private Vector3 baseScale;
    private Vector3 basePosition;

    private Vector2 squash;
    private Vector2 squashVelocity;

    private float randomOffset;

    private void Awake()
    {
        baseScale = transform.localScale;
        basePosition = transform.localPosition;
        randomOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        float time = Time.time + randomOffset;

        float breath = Mathf.Sin(time * breathSpeed) * breathAmount;

        Vector2 target = new Vector2(
            -breath * 0.5f,
            breath
        );

        Vector2 acceleration =
            (target - squash) * springStrength
            - squashVelocity * springDamping;

        squashVelocity += acceleration * dt;
        squash += squashVelocity * dt;

        transform.localScale = new Vector3(
            baseScale.x * (1f + squash.x),
            baseScale.y * (1f + squash.y),
            baseScale.z
        );

        float bob = Mathf.Sin(time * bobSpeed) * bobAmount;

        transform.localPosition = basePosition +
            Vector3.up * bob;
    }

    public void Boing()
    {
        squashVelocity += new Vector2(
            squashAmount * 0.6f,
            -squashAmount
        );
    }

    public void BoingStrong()
    {
        squashVelocity += new Vector2(
            squashAmount * 1.5f,
            -squashAmount * 1.5f
        );
    }
}
