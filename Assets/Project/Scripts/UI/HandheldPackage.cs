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
    // NEW: The street-follow camera, shaken when the wrong package is confirmed
    [SerializeField] private CameraController cameraController;
    // NEW: Tracks strikes and ends the shift once Carrot runs out of them
    [SerializeField] private ShiftManager shiftManager;
    // NEW: Plays the wrong-pick and delivered sounds
    [SerializeField] private AudioSource sfxSource;
    // NEW: Played when the wrong package is confirmed
    [SerializeField] private AudioClip wrongPickClip;
    // NEW: Played when the correct package is confirmed
    [SerializeField] private AudioClip deliveredClip;

    // The chosen package in the BAG tab
    private Order selectedOrder;
    // The order for the customer whose door Carrot is currently at
    private Order currentOrder;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Populates the manifest and sets the manifest to the first active tab when the handheld ui opens
     */
    private void Start()
    {
        PopulateManifest();
        PopulateBag();

        // Sets currently selected order to nothing
        selectedOrder = null;
        selectionText.text = "Selected: None";

        ShowManifest();
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
        }
    }

    /* Void PopulateBag Method:
     * ~ Populates the bag with all orders. (Temporary until packing feature implemented)
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
     * ~ If the selected package belongs to the current customer,
     *   the order is completed and the handheld closes
     * ~ Wrong package selections currently do nothing
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
            // NEW: Plays the delivered cue
            PlaySfx(deliveredClip);
            Close();
        }
        // NEW: Wrong package selected - shakes the camera, plays a sound, and counts as a strike
        // instead of silently doing nothing
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

    /* NEW: Void PlaySfx Method:
     * ~ Plays a one-shot sound effect if both the source and the clip are assigned
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
        currentOrder = order;

        selectedOrder = null;
        selectionText.text = "Selected: None";

        gameObject.SetActive(true);
        ShowBag();
    }

    /* Void Close Method:
     * ~ Closes the handheld UI
     * ~ Does not change the camera because Carrot may still be at the door
     */
    public void Close()
    {
        selectedOrder = null;
        selectionText.text = "Selected: None";

        gameObject.SetActive(false);
    }

    /* Void ShowManifest Method:
     * Sets the active handheld page to the Manifest page.
     */
    public void ShowManifest()
    {
        manifestPage.SetActive(true);
        bagPage.SetActive(false);
    }

    /* Void ShowBag Method:
     * Sets the active handheld page to the Bag page.
     */
    public void ShowBag()
    {
        manifestPage.SetActive(false);
        bagPage.SetActive(true);
    }
}