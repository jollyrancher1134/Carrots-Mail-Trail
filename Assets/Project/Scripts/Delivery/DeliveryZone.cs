using UnityEngine;
using System.Collections;

public class DeliveryZone : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The name of the house this zone belongs to (typed in the inspector for each zone)
    [SerializeField] private string houseName;
    // The OrderManager in the scene, used to look up this house's order
    [SerializeField] private OrderManager orderManager;
    // The main camera's CameraController, used to trigger the doorstep camera
    [SerializeField] private CameraController cameraController;
    // The handheld package object, which displays orders and package selections on the UI
    [SerializeField] private HandheldPackage handheldPackage;

    [Header("Handheld Delay")]
    [SerializeField] private float handheldOpenDelay = 0.5f;

    private Coroutine openRoutine;
    private bool playerInside;

    /* GetCurrentOrder Method:
     * ~ Returns the order belonging to this doorstep
     */
    public Order GetCurrentOrder()
    {
        return orderManager.GetOrderForHouse(houseName);
    }

    /* Void OnTriggerEnter2D:
     * ~ Unity calls this automatically when another collider enters this zone's trigger
     * ~ Checks the Player tag so only Carrot counts as arriving
     * ~ Switches to the doorstep camera and opens the handheld for this house's order
     */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            Debug.Log("Carrot has arrived at " + houseName);

            // Switches to the doorstep camera immediately
            cameraController.EnterDoorstep(transform);

            // Waits before opening the handheld so Carrot can
            // fully enter the delivery zone
            openRoutine = StartCoroutine(OpenHandheldAfterDelay());

            HandheldInput input = other.GetComponent<HandheldInput>();

            if (input != null)
                input.SetCurrentZone(this);
        }
    }

    /* OpenHandheldAfterDelay Coroutine:
     * ~ Waits 0.5 seconds before opening the handheld
     * ~ Allows Carrot to move into the delivery zone first
     */
    private IEnumerator OpenHandheldAfterDelay()
    {
        yield return new WaitForSeconds(handheldOpenDelay);

        openRoutine = null;

        // Prevents opening if Carrot has already left the zone
        if (!playerInside)
            yield break;

        // Prevents replacing a handheld already opened manually
        if (handheldPackage.IsOpen)
            yield break;

        // Asks the OrderManager which order belongs to this house
        Order order = orderManager.GetOrderForHouse(houseName);

        if (order != null)
        {
            Debug.Log(order.customerName + " (" + order.mood +
                      ") is waiting for a " + order.packageName);

            // Opens the handheld to the Bag
            handheldPackage.Open(order);
        }
        else
        {
            Debug.Log("No order for " + houseName);

            // Opens the handheld to the Manifest
            handheldPackage.OpenEmpty();
        }
    }

    /* Void OnTriggerExit2D:
     * ~ Unity calls this automatically when another collider leaves this zone's trigger
     * ~ Only exits the doorstep camera and closes the handheld if this is the door currently being framed,
     *   so leaving Blue while already standing at Pink doesn't kick the camera back out (close pair fix)
     */
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Carrot has left " + houseName);

            playerInside = false;
            HandheldInput input = other.GetComponent<HandheldInput>();

            if (cameraController.IsFraming(transform))
            {
                // Switches back to the street follow camera
                cameraController.ExitDoorstep();
                // Walking away from the door puts the handheld away
                handheldPackage.Close();
            }

            // Cancels the delayed opening if Carrot leaves early
            if (openRoutine != null)
            {
                StopCoroutine(openRoutine);
                openRoutine = null;
            }

            if (input != null)
                input.ClearCurrentZone(this);
        }
    }
}