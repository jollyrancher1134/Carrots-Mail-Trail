using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    /*Serialize fields: 
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The name of the house this zone belongs to (typed in the inspector for each zone)
    [SerializeField] private string houseName;
    // The OrderManager in the scene, used to look up this house's order
    [SerializeField] private OrderManager orderManager;
    // NEW: The main camera's CameraController, used to trigger the doorstep camera
    [SerializeField] private CameraController cameraController;
    // The handheld package object. which is used to display orders and package selections on the UI
    [SerializeField] private HandheldPackage handheldPackage;

    /* Void OnTriggerEnter2D:
     * ~ Unity calls this automatically when another collider enters this zone's trigger
     * ~ Checks the Player tag so only Carrot counts as arriving
     * ~ Looks up this house's order and reports the customer, their mood, and their package
     */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Carrot has arrived at " + houseName);

            // NEW: Switches to the doorstep camera, framing Carrot and this house's door
            cameraController.EnterDoorstep(transform);

            // Asks the OrderManager which order belongs to this house
            Order order = orderManager.GetOrderForHouse(houseName);

            // Reports the order and the customer's mood if there is one, otherwise says this house has nothing waiting
            if (order != null)
            {
                Debug.Log(order.customerName + " (" + order.mood + ") is waiting for a " + order.packageName);

                // Opens the handheld package ui upon reaching a door.
                handheldPackage.Open(order);
                // Commented out to implement handheld package ui
                /* NEW: Registers the delivery and awards points
                 * ~ There's no handheld package list yet, so arriving at the house delivers its order automatically
                 * orderManager.CompleteOrder(order);
                 */
            }
            else
            {
                Debug.Log("No order for " + houseName);
                // NEW: Opens the handheld to the Manifest instead of doing nothing -
                // this house's order was already delivered, not genuinely missing
                handheldPackage.OpenEmpty();
            }
        }
    }

    /* Void OnTriggerExit2D:
     * ~ Unity calls this automatically when another collider leaves this zone's trigger
     * ~ Later this will close the package list too, once it exists
     */
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Carrot has left " + houseName);

            // NEW: Switches back to the street follow camera
            cameraController.ExitDoorstep();
        }
    }
}