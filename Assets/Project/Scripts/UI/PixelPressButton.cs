using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Makes sure the button always has an Image, since the press swaps that Image's sprite
[RequireComponent(typeof(Image))]
public class PixelPressButton : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ISubmitHandler
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    [Header("Art")]
    // The raised button art (the face with its shadow row underneath)
    [SerializeField] private Sprite upSprite;
    // The pressed button art (the same face drawn 1 art pixel lower, covering the shadow)
    [SerializeField] private Sprite downSprite;

    [Header("Label")]
    // The text that rides down with the button face
    [SerializeField] private RectTransform label;
    // One art pixel in UI units (the phone is laid out at 10x, so 10)
    [SerializeField] private float pressDepth = 10f;
    // How long a keyboard or gamepad press stays down, in seconds
    [SerializeField] private float tapTime = 0.08f;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // The button's Image, which shows the up or down art
    private Image image;
    // The button itself, used to check it can be pressed
    private Button button;
    // Where the label sits while the button is up
    private Vector2 labelUpPosition;
    // True while the button is held down on purpose (the tab Carrot is currently on)
    private bool stuckDown;
    // True while the mouse or touch is holding the button down
    private bool pointerHeld;

    /* Void Awake Method:
     * ~ Runs once when the button is loaded, before anything else
     * ~ Finds the Image and Button, and remembers where the label sits while up
     */
    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        if (label != null)
        {
            labelUpPosition = label.anchoredPosition;
        }
    }

    /* Void OnEnable Method:
     * ~ Runs whenever the button is switched on (the handheld opening)
     * ~ Forgets any old press, so the button never opens stuck halfway
     */
    private void OnEnable()
    {
        pointerHeld = false;
        Show(stuckDown);
    }

    /* Public SetStuckDown Method:
     * ~ Called by HandheldPackage to hold a tab down (true) or let it pop back up (false)
     * ~ The tab Carrot is on stays pushed in, so it's clear which page is open
     */
    public void SetStuckDown(bool down)
    {
        stuckDown = down;

        // Can be called before Awake if the handheld switches tabs while it's still hidden
        if (image == null)
        {
            Awake();
        }

        Show(stuckDown || pointerHeld);
    }

    /* Void OnPointerDown Method:
     * ~ Unity calls this when the mouse or a touch presses on the button
     * ~ Pushes the button down into its shadow
     */
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsUsable() || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        pointerHeld = true;
        Show(true);
    }

    /* Void OnPointerUp Method:
     * ~ Unity calls this when the mouse or touch lets go
     * ~ Pops the button back up, unless it's the tab being held down
     */
    public void OnPointerUp(PointerEventData eventData)
    {
        pointerHeld = false;
        Show(stuckDown);
    }

    /* Void OnPointerExit Method:
     * ~ Unity calls this when the mouse leaves the button
     * ~ Dragging off the button lets it pop back up (the click won't fire either)
     */
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!pointerHeld)
        {
            return;
        }

        pointerHeld = false;
        Show(stuckDown);
    }

    /* Void OnSubmit Method:
     * ~ Unity calls this when the button is pressed with the keyboard or gamepad
     * ~ Plays a quick tap down and back up, since there's no "let go" for a submit press
     */
    public void OnSubmit(BaseEventData eventData)
    {
        if (!IsUsable() || !isActiveAndEnabled)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(Tap());
    }

    /* Tap Coroutine:
     * ~ Holds the button down for tapTime seconds, then lets it back up
     * ~ Uses real time, so it still works while the game is paused
     */
    private IEnumerator Tap()
    {
        Show(true);
        yield return new WaitForSecondsRealtime(tapTime);
        Show(stuckDown);
    }

    /* Bool IsUsable Method:
     * ~ Returns true if the button can be pressed right now (not switched off)
     */
    private bool IsUsable()
    {
        return button == null || button.IsInteractable();
    }

    /* Void Show Method:
     * ~ Swaps to the down art and lowers the label by one art pixel (down), or puts both back (up)
     */
    private void Show(bool down)
    {
        if (image != null && upSprite != null && downSprite != null)
        {
            image.sprite = down ? downSprite : upSprite;
        }

        if (label != null)
        {
            label.anchoredPosition = labelUpPosition + (down ? Vector2.down * pressDepth : Vector2.zero);
        }
    }
}
