using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The hard coded list of orders for this shift (filled in through the inspector)
    [SerializeField] private List<Order> orders;
    // The ShiftManager, told to add a strike when a customer runs out of patience
    [SerializeField] private ShiftManager shiftManager;

    // Orders that have already been delivered this shift (so they can't be delivered twice)
    private readonly List<Order> deliveredOrders = new List<Order>();

    // Running total of points earned from completed deliveries
    public int Score { get; private set; }
    // Maximum possible points for this shift, because every customer can give up to 20 points.
    public int MaxPossibleScore => orders.Count * 20;
    // Points performance normalized from 0.00 to 1.00
    public float PointsPerformance => MaxPossibleScore > 0 ? Mathf.Clamp01((float)Score / MaxPossibleScore): 0f;
    // True once every order in the shift has been delivered
    public bool AllDelivered => deliveredOrders.Count == orders.Count;
    // True once every order has either been delivered or gone (nothing left for Carrot to do this shift)
    public bool AllResolved => orders.TrueForAll(order => deliveredOrders.Contains(order) || order.mood == CustomerMood.Gone);
    // How many orders have been delivered this shift, for the day-grade screen
    public int DeliveredCount => deliveredOrders.Count;
    // True if this order has been delivered (used by the report card to pick each customer's face)
    public bool IsDelivered(Order order) => deliveredOrders.Contains(order);

    // True once Carrot has picked up the parcels at the post office
    private bool parcelsCollected;
    // True once the shift has started, so customers' patience begins counting down
    private bool patienceRunning;

    /* Public Events:
     * ~ Other scripts (like CustomerBubble) listen to these to react when something happens to an order
     * ~ The order it happened to is passed along, so each bubble can check if it was their customer
     */
    // Raised when the wrong package is confirmed at a customer's door
    public event System.Action<Order> WrongPick;
    // Raised when the right package is delivered
    public event System.Action<Order> OrderDelivered;

    /* Public ParcelsCollected Property:
     * ~ Lets the PostOffice read whether the parcels were picked up, without being able to change it
     */
    public bool ParcelsCollected
    {
        get { return parcelsCollected; }
    }

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Fills every customer's patience timer, which starts counting down once the shift starts
     */
    private void Start()
    {
        // If the ShiftManager field was left empty in the inspector, find it in the scene instead
        if (shiftManager == null)
        {
            shiftManager = FindFirstObjectByType<ShiftManager>();
        }

        foreach (Order order in orders)
        {
            order.StartTimer();
        }
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Counts down every waiting customer's patience, and gives a strike when one gives up
     */
    private void Update()
    {
        // Shift's over: stop ticking patience entirely, so moods don't keep changing behind the game over screen
        if (shiftManager != null && shiftManager.IsShiftOver)
        {
            return;
        }

        // Shift hasn't started yet: Carrot is still at the post office, so nobody's patience counts down
        if (!patienceRunning)
        {
            return;
        }

        foreach (Order order in orders)
        {
            // Delivered customers are done, so their timer stops
            if (deliveredOrders.Contains(order))
            {
                continue;
            }

            CustomerMood moodBefore = order.mood;
            order.Tick(Time.deltaTime);

            // A customer who just ran out of patience counts as a strike (a late order, per the GDD)
            if (moodBefore != CustomerMood.Gone && order.mood == CustomerMood.Gone)
            {
                if (shiftManager != null)
                {
                    shiftManager.AddStrike();
                }
            }
        }

        // Nothing left to deliver and the shift didn't already end in strikes: wrap it up instead of
        // ticking an empty shift down to zero (AllDelivered is handled separately, as an instant win)
        if (!AllDelivered && AllResolved && shiftManager != null && !shiftManager.IsShiftOver)
        {
            shiftManager.EndShiftNoOrdersLeft();
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
     * ~ Returns the matching order, or null if the parcels haven't been picked up yet, that house has no order,
     *   it's already delivered, or the customer is Gone
     */
    public Order GetOrderForHouse(string houseName)
    {
        // Carrot hasn't picked up the parcels yet, so there is nothing to deliver
        if (!parcelsCollected)
        {
            return null;
        }

        // Checks each order in the list one at a time
        foreach (Order order in orders)
        {
            // If this order's house matches, it isn't delivered yet, and the customer is still waiting, hand it back
            if (order.houseName == houseName && !deliveredOrders.Contains(order) && order.mood != CustomerMood.Gone)
            {
                return order;
            }
        }

        // No order matched this house
        return null;
    }

    /* Public CollectParcels Method:
     * ~ Called by the PostOffice when Carrot presses Interact at the counter
     * ~ Marks the parcels as picked up, so houses can now accept deliveries
     */
    public void CollectParcels()
    {
        parcelsCollected = true;
    }

    /* Public StartPatience Method:
     * ~ Called by the ShiftManager when the shift starts (Carrot leaves the post office)
     * ~ Starts every customer's patience counting down
     */
    public void StartPatience()
    {
        patienceRunning = true;
    }

    /* Private GetDeliveryPoints Method:
    * ~ Calculates delivery points based on customer mood
    */
    private int GetDeliveryPoints(CustomerMood mood)
    {
        switch (mood)
        {
            case CustomerMood.Happy:
                return 20;

            case CustomerMood.Impatient:
                return 15;

            case CustomerMood.Angry:
                return 5;

            default:
                return 0;
        }
    }

    /* Public CompleteOrder Method:
     * ~ Called by HandheldPackage once the correct package has been confirmed
     * ~ Marks the order delivered so it can't be redelivered, and adds its points to the score
     */
    public void CompleteOrder(Order order)
    {
        // Updated so that customers that are Gone cannot receive deliveries.
        if (order == null || deliveredOrders.Contains(order) || order.mood == CustomerMood.Gone)
        {
            return;
        }

        // Calculate points from the customer's mood at delivery
        int earnedPoints = GetDeliveryPoints(order.mood);

        // Mark the order as delivered
        deliveredOrders.Add(order);

        // Add the earned points to the shift score
        Score += earnedPoints;

        Debug.Log("Delivered to " + order.customerName + " (" + order.mood + ", +" + earnedPoints + " pts, score: " + Score + ")");

        // Lets the customer's bubble show the delivered face
        if (OrderDelivered != null)
        {
            OrderDelivered(order);
        }
    }

    /* Public ReportWrongPick Method:
     * ~ Called by HandheldPackage when the wrong package is confirmed at a door
     * ~ Lets the customer's bubble react, so the wrong pick reads in the world, not just on the handheld
     */
    public void ReportWrongPick(Order order)
    {
        if (order == null)
        {
            return;
        }

        Debug.Log(order.customerName + " got the wrong package");

        if (WrongPick != null)
        {
            WrongPick(order);
        }
    }
}