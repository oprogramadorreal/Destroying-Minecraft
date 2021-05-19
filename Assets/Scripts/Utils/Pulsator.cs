using UnityEngine;

public sealed class Pulsator : MonoBehaviour
{
    [SerializeField]
    private float bpm = 60.0f;

    [SerializeField]
    private float strength = 1.0f;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        var step = 1.0f + Mathf.Cos(Time.unscaledTime * (bpm / 60.0f) * Mathf.PI * 2.0f);
        step *= strength;

        transform.localScale = originalScale + new Vector3(step, step, 0.0f);
    }
}
