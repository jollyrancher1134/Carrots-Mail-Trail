using UnityEngine;
using UnityEngine.InputSystem;

// Makes sure Carrot always has a Rigidbody2D, since this script can't work without one
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    /*Serialize fields: 
     * ~ this allows for the value to be viewed inside the inspector
     */
    // Walk speed of carrot (since serialized it can change inside the inspector)
    [SerializeField] private float walkSpeed = 4f;
    // Sprint speed of carrot in tiles per second
    [SerializeField] private float sprintSpeed = 7f;
    // Reads the Move action from the Input Actions asset (is not device specific, application automatically identifies controller input type)
    [SerializeField] private InputActionReference moveAction;
    // Reads the Sprint action from the Input Actions asset (Left Shift, left stick press, or any other sprint binding)
    [SerializeField] private InputActionReference sprintAction;
    // How much stamina Carrot has when full
    [SerializeField] private float maxStamina = 100f;
    // How much stamina sprinting uses per second (100 / 30 = about 3 seconds of sprint)
    [SerializeField] private float staminaDrain = 30f;
    // How much stamina refills per second while not sprinting
    [SerializeField] private float staminaRegen = 20f;
    // How much stamina Carrot needs before sprinting again after running out
    [SerializeField] private float exhaustedRecover = 30f;

    /* Declare private fields: 
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // Handles the physics of Carrot by setting velocity while handling physical collisions with walls
    private Rigidbody2D rb;
    // Carrot's directional inputs per frame
    private Vector2 moveInput;
    // Carrot's last direction moved
    private Vector2 facing = Vector2.down;
    // Carrot's stamina right now
    private float currentStamina;
    // The speed Carrot moves at this frame (walk or sprint)
    private float currentSpeed;
    // True after stamina runs out, until it recovers to exhaustedRecover
    private bool isExhausted;
    // TEMPORARY: Counts time between stamina log messages (remove once sprint is tuned)
    private float logTimer;

    /* Public StaminaPercent Property:
     * ~ Gives stamina as a fraction from 0 (empty) to 1 (full)
     * ~ Read only, so the HUD can show a stamina bar without being able to change stamina
     */
    public float StaminaPercent
    {
        get { return currentStamina / maxStamina; }
    }

    /* Void Awake Method:
     * ~ Awake runs once when Carrot is loaded into the scene, before anything else
     * ~ Uses GetComponent to find Carrot's Rigidbody2D and save it in rb
     * ~ Starts Carrot with full stamina and at walking speed
     */
    void Awake()
    {
        // Finds the Rigidbody2D attached to Carrot and saves it in rb
        rb = GetComponent<Rigidbody2D>();

        // Carrot starts the shift fully rested
        currentStamina = maxStamina;
        currentSpeed = walkSpeed;
    }

    /* Void OnEnable Method:
     * ~ Runs whenever the script is switched on
     * ~ Turns on the input for the moveAction and sprintAction
     */
    void OnEnable()
    {
        // Move action (field of reference), action (calling the reference to the Move action), Enable (switch on method)
        moveAction.action.Enable();
        // Same idea for the Sprint action
        sprintAction.action.Enable();
    }

    /* Void Update Method: 
     * ~ Method is called once per frame
     * ~ Reads Carrot's input each frame and remembers the last direction moved
     * ~ Decides walk or sprint speed and drains or refills stamina
     */
    void Update()
    {
        // Stores what the player is pressing this frame as a direction (x, y)
        moveInput = moveAction.action.ReadValue<Vector2>();

        // Only updates facing while Carrot is moving, so Carrot remembers their direction when standing still
        if (moveInput.sqrMagnitude > 0.01f)
        {
            facing = moveInput.normalized;
        }

        // Whether Carrot is actually moving, and whether the sprint button is held this frame
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool sprintHeld = sprintAction.action.IsPressed();

        // Sprinting only happens while moving, holding sprint, and not exhausted
        if (sprintHeld && isMoving && !isExhausted)
        {
            currentSpeed = sprintSpeed;

            // Time.deltaTime turns "per frame" into "per second", so drain is the same on any computer
            currentStamina -= staminaDrain * Time.deltaTime;

            // Ran out: stop at zero and become exhausted
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isExhausted = true;
                Debug.Log("Carrot is exhausted! Stamina: " + currentStamina);
            }
        }
        else
        {
            currentSpeed = walkSpeed;

            // Refills stamina, but never past the maximum
            currentStamina += staminaRegen * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);

            // Recovered enough to sprint again
            if (isExhausted && currentStamina >= exhaustedRecover)
            {
                isExhausted = false;
                Debug.Log("Carrot recovered and can sprint again. Stamina: " + currentStamina.ToString("F0"));
            }
        }

        // TEMPORARY: Prints stamina every half second for testing (remove once sprint is tuned)
        logTimer += Time.deltaTime;
        if (logTimer >= 0.5f)
        {
            Debug.Log("Stamina: " + currentStamina.ToString("F0") + " | Exhausted: " + isExhausted);
            logTimer = 0f;
        }
    }

    /* Void FixedUpdate Method: 
     * ~ Method runs on the physics clock (50 times per second) 
     * ~ Sets Carrot's Velocity to the input direction x current speed (walk or sprint)
     */
    private void FixedUpdate()
    {
        // Direction x speed = velocity; Rigidbody2D handles collisions so Carrot slides along walls
        rb.linearVelocity = moveInput * currentSpeed;
    }
}