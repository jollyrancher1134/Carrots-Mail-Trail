using UnityEngine;
using UnityEngine.InputSystem;

public class PostOffice : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */

    // The OrderManager in the scene, told when Carrot collects the parcels
    [SerializeField] private OrderManager orderManager;
    // The ShiftManager in the scene, told to start the shift once Carrot leaves with the parcels
    [SerializeField] private ShiftManager shiftManager;
    // The handheld package object, whose bag is filled when the parcels are collected
    [SerializeField] private HandheldPackage handheldPackage;
    // the type of interaction action that's needed??
    [SerializeField] private InputActionReference interactAction;

    /*Private fields:
    * ~ these fields are only accessible within the script
    */

    // a boolean determining whether or not the player is inside the post office.
    private bool playerInside;

    /* Void OnEnable Method:
     * ~ enables the interact action needed to be performed
     */
    void OnEnable()
    {
        // interact Action, calls action and calls Enable() method
        interactAction.action.Enable();
    }

    /* Void OnTriggerEnter2D Method:
     * ~ Unity calls this automatically when another collider enters the post office trigger
     * ~ Checks the Player tag so only Carrot counts as arriving at the counter
     */
    void OnTriggerEnter2D(Collider2D other)
    {
        // if the player is detected inside the trigger
        if (other.CompareTag("Player"))
        {
            // the bool playerInside is set to true
            playerInside = true;

            Debug.Log("Carrot is at the post office counter");
        }
    }

    /* Void OnTriggerExit2D Method:
     * ~ Unity calls this automatically when another collider leaves the post office trigger
     * ~ If Carrot leaves carrying the parcels, the shift starts (the clock starts on leaving the post office, per the GDD)
     * ~ Leaving empty handed starts nothing, so Carrot can come back in
     */
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            Debug.Log("Carrot left the post office");

            // Only starts the shift the first time Carrot leaves with his parcels
            if (orderManager.ParcelsCollected && !shiftManager.IsShiftStarted)
            {
                shiftManager.StartShift();
            }
        }
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Picks up the parcels when Interact is pressed at the counter
     * ~ Each check stops the method early if pickup isn't allowed this frame
     */
    void Update()
    {
        // Paused: Update still runs while timeScale is 0, so input has to be blocked here
        if (Time.timeScale == 0f)
        {
            return;
        }

        // The shift has ended, so there is nothing left to pick up
        if (shiftManager.IsShiftOver)
        {
            return;
        }

        // Carrot isn't at the counter
        if (!playerInside)
        {
            return;
        }

        // The parcels have already been collected (one visit per shift)
        if (orderManager.ParcelsCollected)
        {
            return;
        }

        // WasPressedThisFrame is only true on the frame the button goes down, so one press is one pickup
        if (interactAction.action.WasPressedThisFrame())
        {
            PickUp();
        }
    }

    /* Void PickUp Method:
     * ~ Collects every parcel for the shift in one visit and fills the bag
     */
    private void PickUp()
    {
        orderManager.CollectParcels();
        handheldPackage.FillBag();

        Debug.Log("Carrot picked up the parcels. Head out to start the shift!");
    }
}