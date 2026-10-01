using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    /*Serialize fields: 
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The hard coded list of orders for this shift (filled in through the inspector)
    [SerializeField] private List<Order> orders;

    // Orders that have already been delivered this shift (so they can't be delivered twice)
    private readonly List<Order> deliveredOrders = new List<Order>();

    // Running total of points earned from completed deliveries
    public int Score { get; private set; }
    // NEW: True once every order in the shift has been delivered
    public bool AllDelivered => deliveredOrders.Count == orders.Count;

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

    /* Public GetOrders Method:
     * ~ Returns our list of orders for the shift
     */
    public List<Order> GetOrders()
    {
        return orders;
    }

    /* Public GetOrderForHouse Method:
     * ~ Other scripts (like DeliveryZone) call this to ask which order belongs to a house
     * ~ Returns the matching order, or null if that house has no order or it's already delivered
     */
    public Order GetOrderForHouse(string houseName)
    {
        // Checks each order in the list one at a time
        foreach (Order order in orders)
        {
            // If this order's house matches the one we're asking about, hand it back
            if (order.houseName == houseName && !deliveredOrders.Contains(order))
            {
                return order;
            }
        }

        // No order matched this house
        return null;
    }

    /* Public CompleteOrder Method:
     * ~ Called by DeliveryZone once the correct package has been dropped off
     * ~ Marks the order delivered so it can't be redelivered, and adds its points to the score
     */
    public void CompleteOrder(Order order)
    {
        if (order == null || deliveredOrders.Contains(order))
        {
            return;
        }

        deliveredOrders.Add(order);
        Score += order.pointValue;

        Debug.Log("Delivered to " + order.customerName + " (+" + order.pointValue + " pts, score: " + Score + ")");
    }
}