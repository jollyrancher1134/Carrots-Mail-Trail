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
    // The main camera's CameraController, used to trigger the doorstep camera
    [SerializeField] private CameraController cameraController;
    // The handheld package object, which displays orders and package selections on the UI
    [SerializeField] private HandheldPackage handheldPackage;

    /* Void OnTriggerEnter2D:
     * ~ Unity calls this automatically when another collider enters this zone's trigger
     * ~ Checks the Player tag so only Carrot counts as arriving
     * ~ Switches to the doorstep camera and opens the handheld for this house's order
     */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Carrot has arrived at " + houseName);

            // Switches to the doorstep camera, framing Carrot and this house's door
            cameraController.EnterDoorstep(transform);

            // Asks the OrderManager which order belongs to this house
            Order order = orderManager.GetOrderForHouse(houseName);

            if (order != null)
            {
                Debug.Log(order.customerName + " (" + order.mood + ") is waiting for a " + order.packageName);

                // Opens the handheld to the Bag so the player can pick a package for this customer
                handheldPackage.Open(order);
            }
            else
            {
                Debug.Log("No order for " + houseName);

                // Opens the handheld to the Manifest instead, since this house has nothing waiting
                handheldPackage.OpenEmpty();
            }
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

            if (cameraController.IsFraming(transform))
            {
                // Switches back to the street follow camera
                cameraController.ExitDoorstep();
                // Walking away from the door puts the handheld away
                handheldPackage.Close();
            }
        }
    }
}