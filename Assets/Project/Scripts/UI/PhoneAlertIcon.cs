using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Makes sure the icon always has an Image, since this script swaps that Image's sprite
[RequireComponent(typeof(Image))]
public class PhoneAlertIcon : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    [Header("References")]
    // Reads which door Carrot is standing at
    [SerializeField] private HandheldInput handheldInput;
    // The handheld, so the icon hides while the phone is open
    [SerializeField] private HandheldPackage handheldPackage;

    [Header("Art")]
    // The phone icon while nothing needs doing (screen dark)
    [SerializeField] private Sprite idleSprite;
    // The same idle icon pushed down into its shadow, shown while it's being clicked
    [SerializeField] private Sprite idlePressedSprite;
    // The phone lighting up / ringing, played on a loop while Carrot is at a door with an order
    [SerializeField] private Sprite[] alertFrames;
    // The same ringing frames pushed down into their shadow, in the same order (shown while clicked)
    [SerializeField] private Sprite[] alertPressedFrames;
    // How many alert frames play per second
    [SerializeField] private float framesPerSecond = 3f;

    [Header("Jingle")]
    // How far the icon jumps side to side while ringing, in UI units (one art pixel, so it stays crisp: the icon is drawn at 5x, so 5)
    [SerializeField] private float jingleDistance = 5f;
    // How many times a second the icon jumps from side to side
    [SerializeField] private float jingleRate = 16f;
    // How long each jingle lasts, in seconds
    [SerializeField] private float jingleTime = 0.4f;
    // How long the icon rests between jingles, in seconds
    [SerializeField] private float jinglePause = 0.8f;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // The icon's Image, which shows the idle or alert art
    private Image image;
    // The icon's RectTransform, which is moved for the jingle
    private RectTransform rect;
    // Where the icon rests when it isn't jingling
    private Vector2 restPosition;
    // Seconds since the alert started, used to time the frames and jingles
    private float alertTimer;
    // True while the mouse or touch is holding the icon down
    private bool pointerHeld;

    /* Void Awake Method:
     * ~ Runs once when the icon is loaded, before anything else
     * ~ Finds the Image, remembers where the icon rests, and finds any references left empty
     */
    private void Awake()
    {
        image = GetComponent<Image>();
        rect = (RectTransform)transform;
        restPosition = rect.anchoredPosition;

        if (handheldInput == null)
        {
            handheldInput = FindFirstObjectByType<HandheldInput>();
        }
        if (handheldPackage == null)
        {
            handheldPackage = FindFirstObjectByType<HandheldPackage>(FindObjectsInactive.Include);
        }
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Hides the icon while the phone is open (the phone sits in the same corner)
     * ~ Rings (lights up and jingles) while Carrot is at a door whose customer is still waiting
     * ~ Otherwise shows the plain idle phone
     * ~ Uses real time, so it keeps animating if the game is paused
     */
    private void Update()
    {
        bool phoneOpen = handheldPackage != null && handheldPackage.IsOpen;
        image.enabled = !phoneOpen;

        if (phoneOpen)
        {
            pointerHeld = false;
        }

        if (phoneOpen || !NeedsOpening())
        {
            alertTimer = 0f;
            rect.anchoredPosition = restPosition;
            Sprite idle = pointerHeld && idlePressedSprite != null ? idlePressedSprite : idleSprite;
            if (idle != null)
            {
                image.sprite = idle;
            }
            return;
        }

        alertTimer += Time.unscaledDeltaTime;

        // Plays the light up animation on a loop (the pressed version while it's held down)
        if (alertFrames != null && alertFrames.Length > 0)
        {
            int frame = Mathf.FloorToInt(alertTimer * framesPerSecond) % alertFrames.Length;
            bool usePressed = pointerHeld && alertPressedFrames != null && frame < alertPressedFrames.Length;
            image.sprite = usePressed ? alertPressedFrames[frame] : alertFrames[frame];
        }

        // Jingles in short bursts, then rests, like a phone buzzing
        float cycle = alertTimer % (jingleTime + jinglePause);
        if (cycle < jingleTime)
        {
            // Flips between left and right in whole steps, so the pixel art never lands between pixels
            int side = Mathf.FloorToInt(cycle * jingleRate) % 2 == 0 ? 1 : -1;
            rect.anchoredPosition = restPosition + new Vector2(side * jingleDistance, 0f);
        }
        else
        {
            rect.anchoredPosition = restPosition;
        }
    }

    /* Bool NeedsOpening Method:
     * ~ Returns true if Carrot is at a door and that customer is still waiting for a parcel
     */
    private bool NeedsOpening()
    {
        if (handheldInput == null || handheldInput.CurrentZone == null)
        {
            return false;
        }

        return handheldInput.CurrentZone.GetCurrentOrder() != null;
    }

    /* Void OnPointerDown Method:
     * ~ Unity calls this when the mouse or a touch presses on the icon
     * ~ Pushes the icon down into its shadow
     */
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            pointerHeld = true;
        }
    }

    /* Void OnPointerUp Method:
     * ~ Unity calls this when the mouse or touch lets go
     * ~ Pops the icon back up
     */
    public void OnPointerUp(PointerEventData eventData)
    {
        pointerHeld = false;
    }

    /* Void OnPointerExit Method:
     * ~ Unity calls this when the mouse leaves the icon
     * ~ Dragging off the icon lets it pop back up
     */
    public void OnPointerExit(PointerEventData eventData)
    {
        pointerHeld = false;
    }

    /* Public OpenPhone Method:
     * ~ Hooked to the icon's Button, so clicking the icon opens the phone (the same as pressing Tab or Y)
     */
    public void OpenPhone()
    {
        if (handheldInput != null)
        {
            handheldInput.TogglePhone();
        }
    }
}
