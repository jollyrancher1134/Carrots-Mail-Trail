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

            // Asks the OrderManager which order belongs to this house
            Order order = orderManager.GetOrderForHouse(houseName);

            // Reports the order and the customer's mood if there is one, otherwise says this house has nothing waiting
            if (order != null)
            {
                Debug.Log(order.customerName + " (" + order.mood + ") is waiting for a " + order.packageName);

                // NEW: Registers the delivery and awards points
                // ~ There's no handheld package list yet, so arriving at the house delivers its order automatically
                orderManager.CompleteOrder(order);
            }
            else
            {
                Debug.Log("No order for " + houseName);
            }
        }
    }

    /* Void OnTriggerExit2D:
     * ~ Unity calls this automatically when another collider leaves this zone's trigger
     * ~ Later this will close the package list and return to the street camera
     */
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Carrot has left " + houseName);
        }
    }
}