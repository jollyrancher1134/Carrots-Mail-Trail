using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    /*Serialize fields: 
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The hard coded list of orders for this shift (filled in through the inspector)
    [SerializeField] private List<Order> orders;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Starts every customer's patience timer at the beginning of the shift
     */
    private void Start()
    {
        foreach (Order order in orders)
        {
            order.StartTimer();
        }
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Counts down every customer's patience at the same time
     */
    private void Update()
    {
        foreach (Order order in orders)
        {
            order.Tick(Time.deltaTime);
        }
    }

    /* Public GetOrderForHouse Method:
     * ~ Other scripts (like DeliveryZone) call this to ask which order belongs to a house
     * ~ Returns the matching order, or null if that house has no order
     */
    public Order GetOrderForHouse(string houseName)
    {
        // Checks each order in the list one at a time
        foreach (Order order in orders)
        {
            // If this order's house matches the one we're asking about, hand it back
            if (order.houseName == houseName)
            {
                return order;
            }
        }

        // No order matched this house
        return null;
    }
}