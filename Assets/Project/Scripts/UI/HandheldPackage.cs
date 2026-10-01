using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HandheldPackage : MonoBehaviour
{
    /* Headers and Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     * ~ headers to better separate what data is for what
     */
    [Header("Order Data")]
    // Order manager for grabbing order data
    [SerializeField] private OrderManager orderManager;

    [Header("Pages")]
    // The manifest page object where we display order data
    [SerializeField] private GameObject manifestPage;
    // The bag page object where we display the HELD packages
    [SerializeField] private GameObject bagPage;

    [Header("Manifest")]
    // The customer list object inside of the scrollview where we populate with customer entries
    [SerializeField] private Transform customerList;
    // The customer entry prefab containing order data
    [SerializeField] private CustomerEntry customerEntryPrefab;

    [Header("Bag")]
    // The package list object which holds each held package in the bag
    [SerializeField] private Transform packageList;
    // The package button prefab object
    [SerializeField] private PackageButton packageButtonPrefab;
    // The selection text which displays the currently selected package
    [SerializeField] private TMP_Text selectionText;

    [Header("Feedback")]
    // The camera controller, shaken when the wrong package is confirmed
    [SerializeField] private CameraController cameraController;
    // Tracks strikes and ends the shift once Carrot runs out of them
    [SerializeField] private ShiftManager shiftManager;
    // Plays the wrong pick and delivered sounds
    [SerializeField] private AudioSource sfxSource;
    // Played when the wrong package is confirmed
    [SerializeField] private AudioClip wrongPickClip;
    // Played when the correct package is confirmed
    [SerializeField] private AudioClip deliveredClip;

    // The chosen package in the BAG tab
    private Order selectedOrder;
    // The order for the customer whose door Carrot is currently at
    private Order currentOrder;
    // True while the handheld is open at a door (so Start doesn't hide it if a door opened it first)
    private bool isOpen;
    // Maps each order to its manifest row, so a delivered order's row can be removed
    private readonly Dictionary<Order, CustomerEntry> manifestEntries = new Dictionary<Order, CustomerEntry>();

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Fills the manifest and bag, then hides the handheld until Carrot reaches a door
     */
    private void Start()
    {
        PopulateManifest();
        PopulateBag();

        // Only reset and hide if a door didn't already open it
        if (!isOpen)
        {
            selectedOrder = null;
            selectionText.text = "Selected: None";
            ShowManifest();
            gameObject.SetActive(false);
        }
    }

    /* Void PopulateManifest Method:
     * ~ Creates one customer entry for every order in the shift
     */
    private void PopulateManifest()
    {
        foreach (Order order in orderManager.GetOrders())
        {
            // Instantiate a new customer entry prefab into the customer list
            CustomerEntry entry = Instantiate(customerEntryPrefab, customerList);
            // Setup the text display for the new customer entry order data
            entry.Setup(order);
            // Remembers this row so it can be removed once the order is delivered
            manifestEntries[order] = entry;
        }
    }

    /* Void RemoveManifestEntry Method:
     * ~ Removes a delivered order's row from the Manifest, so the list visibly shrinks
     */
    private void RemoveManifestEntry(Order order)
    {
        if (manifestEntries.TryGetValue(order, out CustomerEntry entry) && entry != null)
        {
            Destroy(entry.gameObject);
            manifestEntries.Remove(order);
        }
    }

    /* Void PopulateBag Method:
     * ~ Populates the bag with all orders (temporary until the packing feature is implemented)
     */
    private void PopulateBag()
    {
        foreach (Order order in orderManager.GetOrders())
        {
            // Instantiate a new package prefab into the package list
            PackageButton button = Instantiate(packageButtonPrefab, packageList);
            // Setup the text display for the new package order data
            button.Setup(order, this);
        }
    }

    /* Void SelectPackage Method:
     * ~ Sets the selected package to then be confirmed
     */
    public void SelectPackage(Order order)
    {
        selectedOrder = order;
        selectionText.text = "Selected: " + order.packageName;
    }

    /* Void ConfirmPackage Method:
     * ~ Confirms the currently selected package
     * ~ If the selected package belongs to the current customer, the order is completed,
     *   its manifest row is removed, the handheld closes, and a win is checked for
     * ~ Otherwise it counts as a strike and gives feedback
     */
    public void ConfirmPackage()
    {
        // Nothing has been selected
        if (selectedOrder == null || currentOrder == null)
        {
            return;
        }

        // Correct package selected
        if (selectedOrder == currentOrder)
        {
            orderManager.CompleteOrder(currentOrder);
            // Plays the delivered cue
            PlaySfx(deliveredClip);
            // Removes the delivered customer from the Manifest list
            RemoveManifestEntry(currentOrder);
            // Wins the shift once every order has been delivered
            if (orderManager.AllDelivered && shiftManager != null)
            {
                shiftManager.Win();
            }
            Close();
        }
        // Wrong package selected: shakes the camera, plays a sound, and counts as a strike
        else
        {
            if (cameraController != null)
            {
                cameraController.Shake();
            }
            PlaySfx(wrongPickClip);
            if (shiftManager != null)
            {
                shiftManager.AddStrike();
            }
        }
    }

    /* Void PlaySfx Method:
     * ~ Plays a one shot sound effect if both the source and the clip are assigned
     * ~ Safe to call with no clip set yet, so feedback can be wired before sound assets exist
     */
    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /* Void Open Method:
     * ~ Opens the handheld UI for the current customer's order
     * ~ Opens directly to the Bag page when at a customer's door
     */
    public void Open(Order order)
    {
        isOpen = true;
        currentOrder = order;

        selectedOrder = null;
        selectionText.text = "Selected: None";

        gameObject.SetActive(true);
        ShowBag();
    }

    /* Public OpenEmpty Method:
     * ~ Called by DeliveryZone when a house has no order left (already delivered or the customer is Gone)
     * ~ Opens straight to the Manifest instead of the Bag, since there's nothing to confirm here
     */
    public void OpenEmpty()
    {
        isOpen = true;
        currentOrder = null;

        selectedOrder = null;
        selectionText.text = "Selected: None";

        gameObject.SetActive(true);
        ShowManifest();
    }

    /* Void Close Method:
     * ~ Closes the handheld UI
     * ~ Does not change the camera because Carrot may still be at the door
     */
    public void Close()
    {
        isOpen = false;
        selectedOrder = null;
        selectionText.text = "Selected: None";

        gameObject.SetActive(false);
    }

    /* Void ShowManifest Method:
     * ~ Sets the active handheld page to the Manifest page
     */
    public void ShowManifest()
    {
        manifestPage.SetActive(true);
        bagPage.SetActive(false);
    }

    /* Void ShowBag Method:
     * ~ Sets the active handheld page to the Bag page
     */
    public void ShowBag()
    {
        manifestPage.SetActive(false);
        bagPage.SetActive(true);
    }
}