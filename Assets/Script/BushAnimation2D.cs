
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BushAnimation2D : MonoBehaviour
{
    [Header("Respiração")]
    [SerializeField] private float breathAmount = 0.015f;
    [SerializeField] private float breathSpeed = 2.5f;

    [Header("Balanço")]
    [SerializeField] private float swayAngle = 3f;
    [SerializeField] private float swaySpeed = 2f;

    [Header("Mola")]
    [SerializeField] private float springStrength = 220f;
    [SerializeField] private float springDamping = 9f;
    [SerializeField] private float squashAmount = 0.12f;

    [Header("Tremidinha")]
    [SerializeField] private float shakeAmount = 0.3f;
    [SerializeField] private float shakeSpeed = 8f;

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

        float sway = Mathf.Sin(time * swaySpeed) * swayAngle;

        float shake = Mathf.Sin(time * shakeSpeed) * shakeAmount * 0.01f;

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

        transform.localRotation = Quaternion.Euler(
            0f,
            0f,
            sway + shake
        );

        transform.localPosition = basePosition +
            new Vector3(shake, 0f, 0f);
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
