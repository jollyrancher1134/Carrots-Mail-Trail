using UnityEngine;

public class CustomerBubble : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    [Header("Bubble Animations")]
    // The Happy bubble frames (green), played while the customer is Happy and after a delivery
    [SerializeField] private Sprite[] happyFrames;
    // The Neutral bubble frames (yellow), played while the customer is Impatient
    [SerializeField] private Sprite[] impatientFrames;
    // The Angry bubble frames (red), played while the customer is Angry, on a wrong pick, and when they leave
    [SerializeField] private Sprite[] angryFrames;
    // How many frames play per second while Happy or Impatient
    [SerializeField] private float framesPerSecond = 8f;
    // How many frames play per second while Angry (faster, so it reads as urgent)
    [SerializeField] private float angryFramesPerSecond = 12f;
    // Unlit sprite material, so the bubble stays readable no matter how the village is lit
    [SerializeField] private Material bubbleMaterial;

    [Header("Placement")]
    // Where the tip of the bubble's tail sits, measured from this delivery zone (just above the door)
    [SerializeField] private Vector2 bubbleOffset = new Vector2(0f, 1.5f);
    // Draws the bubble above the houses, Carrot and props
    [SerializeField] private int sortingOrder = 100;

    [Header("Mood Feedback")]
    // Shows the bubble while the customer is still Happy (on, since there is a Happy animation)
    [SerializeField] private bool showWhenHappy = true;
    // How fast the bubble pulses while the customer is Impatient
    [SerializeField] private float impatientPulseSpeed = 3f;
    // How fast the bubble pulses while the customer is Angry (faster, so it reads as urgent)
    [SerializeField] private float angryPulseSpeed = 8f;
    // How much the bubble grows at the top of each pulse (0.1 = 10% bigger)
    [SerializeField] private float pulseAmount = 0.1f;
    // How long a reaction (wrong pick, delivered, gone) stays up, in seconds
    [SerializeField] private float reactionTime = 1.5f;
    // How far the bubble shakes side to side on a wrong pick, in world units
    [SerializeField] private float shakeAmount = 0.08f;
    // How fast the bubble shakes on a wrong pick
    [SerializeField] private float shakeSpeed = 40f;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // The OrderManager in the scene, which owns every customer's order and mood
    private OrderManager orderManager;
    // The ShiftManager in the scene, used to keep bubbles hidden until the shift starts
    private ShiftManager shiftManager;
    // The order belonging to this zone's house
    private Order order;
    // The bubble object built above the door in Start
    private GameObject bubbleObject;
    // Draws the current bubble frame
    private SpriteRenderer bubbleRenderer;
    // Where the bubble rests when it isn't shaking
    private Vector3 restPosition;
    // The customer's mood last frame, so a change to Gone can be noticed once
    private CustomerMood lastMood;
    // Seconds left on the current reaction (0 means no reaction is showing)
    private float reactionTimer;
    // The animation being played as a reaction (wrong pick, delivered or gone)
    private Sprite[] reactionFrames;
    // True while the reaction is a wrong pick, which shakes instead of pulsing
    private bool reactionShakes;
    // True once the order is delivered or the customer is Gone, so the bubble hides for good after its last reaction
    private bool isFinished;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Finds this house's order and builds the bubble above the door
     * ~ Starts listening to the OrderManager for wrong picks and deliveries
     */
    private void Start()
    {
        orderManager = FindFirstObjectByType<OrderManager>();
        shiftManager = FindFirstObjectByType<ShiftManager>();

        // Finds the order for this zone's house (DeliveryZone sits on the same object)
        DeliveryZone zone = GetComponent<DeliveryZone>();
        if (orderManager != null && zone != null)
        {
            foreach (Order shiftOrder in orderManager.GetOrders())
            {
                if (shiftOrder.houseName == zone.HouseName)
                {
                    order = shiftOrder;
                    break;
                }
            }
        }

        BuildBubble();

        // No order for this house means there's nobody to show a mood for
        if (order == null)
        {
            enabled = false;
            return;
        }

        lastMood = order.mood;

        orderManager.WrongPick += OnWrongPick;
        orderManager.OrderDelivered += OnOrderDelivered;
    }

    /* Void OnDestroy Method:
     * ~ Runs when the scene unloads (restart or quit to menu)
     * ~ Stops listening to the OrderManager, so nothing calls into a bubble that no longer exists
     */
    private void OnDestroy()
    {
        if (orderManager != null)
        {
            orderManager.WrongPick -= OnWrongPick;
            orderManager.OrderDelivered -= OnOrderDelivered;
        }
    }

    /* Void BuildBubble Method:
     * ~ Makes the bubble sprite in code, instead of as a child of the zone
     * ~ The zone is scaled up to make its trigger bigger, so a child bubble would come out stretched
     * ~ The bubble isn't parented, so it keeps its true pixel size (zones never move, so it never needs to follow)
     */
    private void BuildBubble()
    {
        bubbleObject = new GameObject(name + "_MoodBubble");
        restPosition = transform.position + (Vector3)bubbleOffset;
        bubbleObject.transform.position = restPosition;

        bubbleRenderer = bubbleObject.AddComponent<SpriteRenderer>();
        bubbleRenderer.sortingOrder = sortingOrder;

        if (bubbleMaterial != null)
        {
            bubbleRenderer.sharedMaterial = bubbleMaterial;
        }

        bubbleObject.SetActive(false);
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Stays hidden until the shift starts (Carrot leaves the post office)
     * ~ A reaction (wrong pick, delivered, gone) takes priority while its timer runs
     * ~ Otherwise plays the customer's mood: Happy, Impatient with a slow pulse, Angry with a fast pulse
     * ~ Uses scaled time, so pausing (timeScale 0) freezes the bubble along with everything else
     */
    private void Update()
    {
        // Customers aren't waiting yet, so nobody has a mood to show
        if (shiftManager != null && !shiftManager.IsShiftStarted)
        {
            bubbleObject.SetActive(false);
            return;
        }

        // A customer who just ran out of patience flashes Angry once, then the bubble is done
        if (lastMood != CustomerMood.Gone && order.mood == CustomerMood.Gone)
        {
            StartReaction(angryFrames, false);
            isFinished = true;
        }
        lastMood = order.mood;

        // A reaction is showing: hold it until its timer runs out
        if (reactionTimer > 0f)
        {
            reactionTimer -= Time.deltaTime;
            PlayFrames(reactionFrames, framesPerSecond);

            if (reactionShakes)
            {
                // Shakes side to side, settling down as the reaction ends
                float strength = reactionTimer / reactionTime;
                float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount * strength;
                bubbleObject.transform.position = restPosition + new Vector3(offsetX, 0f, 0f);
                bubbleObject.transform.localScale = Vector3.one;
            }
            else
            {
                Pulse(impatientPulseSpeed);
            }
            return;
        }

        // Back at rest once a shake is over
        bubbleObject.transform.position = restPosition;

        // Delivered or Gone: the last reaction has finished, so the bubble goes away
        if (isFinished)
        {
            bubbleObject.SetActive(false);
            return;
        }

        // Plays the animation for the customer's current mood
        switch (order.mood)
        {
            case CustomerMood.Happy:
                if (showWhenHappy)
                {
                    PlayFrames(happyFrames, framesPerSecond);
                    bubbleObject.transform.localScale = Vector3.one;
                }
                else
                {
                    bubbleObject.SetActive(false);
                }
                break;

            case CustomerMood.Impatient:
                PlayFrames(impatientFrames, framesPerSecond);
                Pulse(impatientPulseSpeed);
                break;

            case CustomerMood.Angry:
                PlayFrames(angryFrames, angryFramesPerSecond);
                Pulse(angryPulseSpeed);
                break;

            default:
                bubbleObject.SetActive(false);
                break;
        }
    }

    /* Void PlayFrames Method:
     * ~ Turns the bubble on and shows the right frame of the given animation for this moment in time
     * ~ The frame is worked out from the game clock, so the animation loops and freezes when paused
     */
    private void PlayFrames(Sprite[] frames, float fps)
    {
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        if (!bubbleObject.activeSelf)
        {
            bubbleObject.SetActive(true);
        }

        // Counts whole frames since the game started, then wraps around to loop the animation
        int frame = Mathf.FloorToInt(Time.time * fps) % frames.Length;
        bubbleRenderer.sprite = frames[frame];
    }

    /* Void Pulse Method:
     * ~ Gently grows and shrinks the bubble from the tip of its tail, so it stays pinned to the door
     * ~ A higher speed pulses faster (Angry pulses faster than Impatient)
     */
    private void Pulse(float speed)
    {
        // Sin goes from -1 to 1, so this goes from 0 (normal size) to 1 (biggest) and back
        float wave = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        float scale = 1f + wave * pulseAmount;
        bubbleObject.transform.localScale = new Vector3(scale, scale, 1f);
    }

    /* Void StartReaction Method:
     * ~ Plays a reaction animation for reactionTime seconds
     * ~ shakes is true for a wrong pick (shakes the bubble), false for delivered and gone (pulses it)
     */
    private void StartReaction(Sprite[] frames, bool shakes)
    {
        reactionFrames = frames;
        reactionShakes = shakes;
        reactionTimer = reactionTime;
    }

    /* Void OnWrongPick Method:
     * ~ Called by the OrderManager when the wrong package is confirmed at a door
     * ~ Only reacts if it was this customer's door
     */
    private void OnWrongPick(Order wrongOrder)
    {
        if (wrongOrder == order && !isFinished)
        {
            StartReaction(angryFrames, true);
        }
    }

    /* Void OnOrderDelivered Method:
     * ~ Called by the OrderManager when an order is delivered
     * ~ Only reacts if it was this customer's order, then the bubble is done for the shift
     */
    private void OnOrderDelivered(Order deliveredOrder)
    {
        if (deliveredOrder == order)
        {
            StartReaction(happyFrames, false);
            isFinished = true;
        }
    }
}
