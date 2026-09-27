using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    /*Serialize fields: 
     * ~ this allows for the value to be viewed inside the inspector
     */
    // Walk speed of carrot (since serialized it can change inside the inspector)
    [SerializeField] private float walkSpeed = 4f;
    // Reads the Move action from the Input Actions asset (is not device specific, application automatically identifies controller input type)
    [SerializeField] private InputActionReference moveAction; 

    /* Declare private fields: 
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // Handles the physics of Carrot by setting velocity while handling physical collisions with walls
    private Rigidbody2D rb; 
    // Carrot's directional inputs per frame
    private Vector2 moveInput;
    // Carrot's last direction moved
    private Vector2 facing = Vector2.down;


    /* Void Awake Method:
     * ~ Awake runs once when Carrot is loaded into the scene, before anything else
     * ~ Uses GetComponent to find Carrot's Rigidbody2D and save it in rb
     */
    void Awake()
    {
        // Finds the Rigidbody2D attached to Carrot and saves it in rb
        rb = GetComponent<Rigidbody2D>();
    }

    /* Void OnEnable Method:
     * ~ Runs whenever the script is switched on
     * ~ Turns on the input for the moveAction
     */
    void OnEnable()
    {
        // Move action (field of reference), action (calling the reference to the Move action), Enable (switch on method)
        moveAction.action.Enable();
    }

    /* Void Update Method: 
     * ~ Method is called once per frame
     * ~ Reads Carrot's input each frame and remembers the last direction moved
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
    }

    /* Void FixedUpdate Method: 
     * ~ Method runs on the physics clock (50 times per second) 
     * ~ Sets Carrot's Velocity to the input direction x speed
     */
    private void FixedUpdate()
    {
        // Direction x speed = velocity; Rigidbody2D handles collisions so Carrot slides along walls
        rb.linearVelocity = moveInput * walkSpeed;
    }
}
