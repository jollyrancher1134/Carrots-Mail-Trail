using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractPrompt : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    [Header("Prompt Text")]
    // What the prompt says while Carrot is being played with the keyboard
    [SerializeField] private string keyboardPrompt = "Tab  Phone";
    // What the prompt says while Carrot is being played with a gamepad
    [SerializeField] private string gamepadPrompt = "Y  Phone";

    [Header("When It Shows")]
    // On for the post office counter: the prompt goes away once the parcels are picked up
    [SerializeField] private bool onlyBeforeParcels = false;
    // On for the doors: the prompt hides while the handheld is open, since it's already on screen
    [SerializeField] private bool hideWhileHandheldOpen = true;

    [Header("Look")]
    // Where the prompt sits, measured from Carrot (just above his head)
    [SerializeField] private Vector2 promptOffset = new Vector2(0f, 1.4f);
    // How big the text is, in world units
    [SerializeField] private float fontSize = 3f;
    // The text colour (cream, to match the handheld)
    [SerializeField] private Color textColor = new Color(1f, 0.95f, 0.85f, 1f);
    // The outline colour (dark brown, so it reads on grass and roofs alike)
    [SerializeField] private Color outlineColor = new Color(0.23f, 0.13f, 0.15f, 1f);
    // Draws the prompt above the houses, Carrot and the speech bubbles
    [SerializeField] private int sortingOrder = 110;
    // How far the prompt bobs up and down, in world units
    [SerializeField] private float bobAmount = 0.06f;
    // How fast the prompt bobs
    [SerializeField] private float bobSpeed = 4f;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // True after the last button press came from a gamepad, shared by every prompt so they all agree
    private static bool usingGamepad;
    // The OrderManager in the scene, used to check if the parcels have been picked up
    private OrderManager orderManager;
    // The ShiftManager in the scene, used to hide prompts once the shift is over
    private ShiftManager shiftManager;
    // The handheld in the scene, used to hide door prompts while it's open
    private HandheldPackage handheldPackage;
    // Carrot, while he is standing inside this trigger (null when he isn't)
    private Transform player;
    // The prompt text built in Start
    private TextMeshPro promptText;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Finds the managers and builds the prompt text (hidden until Carrot walks in)
     */
    private void Start()
    {
        orderManager = FindFirstObjectByType<OrderManager>();
        shiftManager = FindFirstObjectByType<ShiftManager>();
        handheldPackage = FindFirstObjectByType<HandheldPackage>(FindObjectsInactive.Include);

        BuildPrompt();
    }

    /* Void BuildPrompt Method:
     * ~ Makes the prompt text in code, instead of as a child of the trigger
     * ~ Delivery zones are scaled up to make their triggers bigger, so a child text would come out stretched
     */
    private void BuildPrompt()
    {
        GameObject promptObject = new GameObject(name + "_Prompt");

        promptText = promptObject.AddComponent<TextMeshPro>();
        promptText.fontSize = fontSize;
        promptText.color = textColor;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.textWrappingMode = TextWrappingModes.NoWrap;
        promptText.fontStyle = FontStyles.Bold;
        promptText.outlineWidth = 0.25f;
        promptText.outlineColor = outlineColor;
        promptText.sortingOrder = sortingOrder;
        promptText.rectTransform.sizeDelta = new Vector2(8f, 1f);

        promptObject.SetActive(false);
    }

    /* Void OnTriggerEnter2D Method:
     * ~ Unity calls this automatically when another collider enters this trigger
     * ~ Remembers Carrot, so the prompt can float above him
     */
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
        }
    }

    /* Void OnTriggerExit2D Method:
     * ~ Unity calls this automatically when another collider leaves this trigger
     * ~ Forgets Carrot, which hides the prompt
     */
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
        }
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Notices whether the keyboard or a gamepad was used last, so the prompt shows the right button
     */
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            usingGamepad = false;
        }
        else if (Gamepad.current != null && GamepadWasUsed(Gamepad.current))
        {
            usingGamepad = true;
        }
    }

    /* Void LateUpdate Method:
     * ~ Runs every frame, after Carrot has moved
     * ~ Shows the prompt above Carrot while he is inside the trigger and it's useful, otherwise hides it
     */
    private void LateUpdate()
    {
        if (promptText == null)
        {
            return;
        }

        bool show = ShouldShow();
        if (promptText.gameObject.activeSelf != show)
        {
            promptText.gameObject.SetActive(show);
        }

        if (!show)
        {
            return;
        }

        promptText.text = usingGamepad ? gamepadPrompt : keyboardPrompt;

        // Floats above Carrot with a gentle bob, snapped to the 16 pixels per unit grid so it stays crisp
        float bob = Mathf.Sin(Time.unscaledTime * bobSpeed) * bobAmount;
        Vector3 position = player.position + (Vector3)promptOffset + new Vector3(0f, bob, 0f);
        position.x = Mathf.Round(position.x * 16f) / 16f;
        position.y = Mathf.Round(position.y * 16f) / 16f;
        promptText.transform.position = position;
    }

    /* Bool ShouldShow Method:
     * ~ Returns true if the prompt should be on screen this frame
     * ~ Each check stops the method early if the prompt isn't needed
     */
    private bool ShouldShow()
    {
        // Carrot isn't here
        if (player == null)
        {
            return false;
        }

        // Paused
        if (Time.timeScale == 0f)
        {
            return false;
        }

        // The shift has ended, so there is nothing left to do
        if (shiftManager != null && shiftManager.IsShiftOver)
        {
            return false;
        }

        // The parcels have already been picked up at the counter
        if (onlyBeforeParcels && orderManager != null && orderManager.ParcelsCollected)
        {
            return false;
        }

        // The handheld is already open
        if (hideWhileHandheldOpen && handheldPackage != null && handheldPackage.IsOpen)
        {
            return false;
        }

        return true;
    }

    /* Bool GamepadWasUsed Method:
     * ~ Returns true if any face button, shoulder or stick on the gamepad was used this frame
     */
    private static bool GamepadWasUsed(Gamepad pad)
    {
        return pad.buttonSouth.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame ||
               pad.buttonEast.wasPressedThisFrame || pad.buttonWest.wasPressedThisFrame ||
               pad.startButton.wasPressedThisFrame || pad.leftShoulder.wasPressedThisFrame ||
               pad.rightShoulder.wasPressedThisFrame || pad.dpad.ReadValue().sqrMagnitude > 0.25f ||
               pad.leftStick.ReadValue().sqrMagnitude > 0.25f;
    }

    /* Void OnDestroy Method:
     * ~ Runs when the scene unloads
     * ~ Cleans up the prompt text, since it isn't a child of this object
     */
    private void OnDestroy()
    {
        if (promptText != null)
        {
            Destroy(promptText.gameObject);
        }
    }
}
