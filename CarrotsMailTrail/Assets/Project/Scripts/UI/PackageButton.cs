using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackageButton : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The package's name text object
    [SerializeField] private TMP_Text packageNameText;
    // The button the package name is attached to
    [SerializeField] private Button button;

    // The order data for the package in the bag
    private Order order;
    // Handheld package UI 
    private HandheldPackage handheldUI;

    /* Void Setup Method
     * ~ Sets up a package button within the package list
     */
    public void Setup(Order newOrder, HandheldPackage newHandheldUI)
    {
        order = newOrder;
        handheldUI = newHandheldUI;

        // Set the package name text to the order's package name
        packageNameText.text = order.packageName;

        // Set an on click listener for the package button for selecting the package
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(SelectPackage);
    }

    /* Void SelectPackage Method:
     * ~ Runs HandheldPackage's SelectPackage Method to set the selected package within the bag
     */
    private void SelectPackage()
    {
        handheldUI.SelectPackage(order);
    }
}