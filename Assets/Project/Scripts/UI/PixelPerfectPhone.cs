using UnityEngine;

// Makes sure the phone always has a RectTransform, since this script resizes it
[RequireComponent(typeof(RectTransform))]
public class PixelPerfectPhone : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    [Header("Phone Size (in art pixels)")]
    // How tall the phone art is, in art pixels (the tall card is 50 x 74)
    [SerializeField] private int phoneHeightPixels = 74;
    // The gap between the phone and the screen's bottom right corner, in art pixels
    [SerializeField] private int marginPixels = 3;

    [Header("Screen Fitting")]
    // The scale the phone was laid out at in the scene (10 screen pixels per art pixel = 500 x 740)
    [SerializeField] private int designScale = 10;
    // Space kept free at the top of the screen, so the Strikes and clock HUD stay visible
    [SerializeField] private float reservedTopSpace = 50f;
    // The smallest scale the phone can shrink to
    [SerializeField] private int minScale = 1;
    // The biggest scale the phone can grow to (10 keeps it at 500 x 740)
    [SerializeField] private int maxScale = 10;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // The phone's RectTransform, which is scaled and moved
    private RectTransform rect;
    // The Canvas the phone sits on, used to read its scale
    private Canvas canvas;
    // The screen size the phone was last fitted to
    private int lastScreenWidth;
    private int lastScreenHeight;

    /* Void Awake Method:
     * ~ Runs once when the phone is loaded, before anything else
     * ~ Pins the phone to the bottom right corner, so it grows and shrinks from that corner
     */
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
    }

    /* Void OnEnable Method:
     * ~ Runs whenever the phone is switched on (the handheld opening)
     * ~ Fits the phone to the screen straight away
     */
    private void OnEnable()
    {
        ApplyScale();
    }

    /* Void LateUpdate Method:
     * ~ Runs every frame, after Update
     * ~ Only re-fits the phone when the window size changes
     */
    private void LateUpdate()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            ApplyScale();
        }
    }

    /* Void ApplyScale Method:
     * ~ Picks the biggest whole number scale that still fits the screen, so every art pixel
     *   is always an exact square of screen pixels (never blurry or uneven)
     * ~ Stays 500 x 740 whenever it fits, and only shrinks on short screens (720p -> 9x)
     */
    private void ApplyScale()
    {
        if (rect == null)
        {
            return;
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        // How much room there is for the phone, in UI units
        float canvasScale = canvas != null ? canvas.scaleFactor : 1f;
        float availableHeight = Screen.height / canvasScale - reservedTopSpace;

        // The biggest whole number scale where the phone and its margin still fit
        int pixelScale = Mathf.FloorToInt(availableHeight / (phoneHeightPixels + marginPixels));
        pixelScale = Mathf.Clamp(pixelScale, minScale, maxScale);

        // The phone is laid out at designScale, so this scales it to the chosen size
        float scale = (float)pixelScale / designScale;
        rect.localScale = new Vector3(scale, scale, 1f);

        // Keeps the same gap from the corner, in art pixels
        int margin = marginPixels * pixelScale;
        rect.anchoredPosition = new Vector2(-margin, margin);
    }
}
