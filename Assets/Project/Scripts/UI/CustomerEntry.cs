using UnityEngine;
using TMPro;
public class CustomerEntry : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The customer's name in an entry for the manifest
    [SerializeField] private TMP_Text customerNameText;
    // The customer's house name in an entry for the manifest
    [SerializeField] private TMP_Text houseNameText;
    // The customer's package name in an entry for the manifest
    [SerializeField] private TMP_Text packageNameText;
    // The customer's patience in an entry for the manifest
    [SerializeField] private TMP_Text patienceText;

    // The order data for the current customer entry
    private Order order;
    
    /* Void Setup Method:
     * ~ Sets up the customer entry prefab for the scrollview
     * ~ Edits each text object to display proper order data
     */
    public void Setup(Order newOrder)
    {
        order = newOrder;

        customerNameText.text = order.customerName;
        houseNameText.text = "House: " + order.houseName;
        packageNameText.text = "Package: " + order.packageName;

        UpdatePatience();
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Updates the patience text of the customer entry
     */
    private void Update()
    {
        // Update customer entry's patience text if the order data exists
        if (order != null)
        {
            UpdatePatience();
        }

    }

    /* Void UpdatePatience Method:
     * ~ Updates the patience text within the customer entry
     * ~ Shows time left of patience and mood
     */
    private void UpdatePatience()
    {
        // The order's patience time to the nearest second
        int totalSeconds = Mathf.CeilToInt(order.timeRemaining);
        // The order's patience time in minutes using the total seconds
        int minutes = totalSeconds / 60;
        // The order's patience time in seconds excluding any minutes
        int seconds = totalSeconds % 60;

        // Setup the patience text 'm:ss - Mood'
        patienceText.text =
            minutes + ":" +
            seconds.ToString("00") +
            " - " +
            order.mood;
    }
}
