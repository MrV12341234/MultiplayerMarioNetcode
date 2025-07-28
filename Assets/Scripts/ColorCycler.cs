using UnityEngine;
using UnityEngine.UI;   // ← needed for Image

[RequireComponent(typeof(Image))]
public class ColorCycler : MonoBehaviour
{
    [Tooltip("Seconds between colour hops")]
    [SerializeField] private float interval = 0.25f;

    [Tooltip("How far to advance the hue each hop (0-1 range). "
             + "0.05 ≈ full cycle in 5 sec with 0.25 s interval")]
    [SerializeField] [Range(0f, 1f)] private float hueStep = 0.05f;

    [Tooltip("Use unscaled time so it still flashes if Time.timeScale = 0")]
    [SerializeField] private bool useUnscaledTime = true;

    private Image  img;
    private float  hue;     // current hue value (0-1)
    private float  timer;

    private void Awake()
    {
        img = GetComponent<Image>();
        hue = 0f;
    }

    private void OnEnable()  => timer = 0f;

    private void Update()
    {
        timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (timer < interval) return;

        timer -= interval;          // wrap the timer
        hue    = (hue + hueStep) % 1f;

        // full saturation & brightness for bold colours
        img.color = Color.HSVToRGB(hue, 1f, 1f);
    }
}