using System.Collections;
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

    private Color normalColor;
    private static readonly Color chosenColor =
        new Color(1f, 0.78f, 0.25f, 1f);

    public Order Order => order;

    // Pushes the button down into its shadow while chosen (optional, uses the colour change if left empty)
    private PixelPressButton pressButton;

    [Header("Wrong Pick Juice")]
    // The red the button flashes when it's confirmed at the wrong door
    [SerializeField] private Color wrongColor = new Color(1f, 0.45f, 0.45f, 1f);
    // How long the shake and red flash last, in seconds
    [SerializeField] private float wrongTime = 0.35f;
    // How far the button shakes side to side, in UI units (10 = one art pixel, so it stays crisp)
    [SerializeField] private float shakeDistance = 10f;
    // How many times a second the button jumps from side to side
    [SerializeField] private float shakeRate = 30f;

    private void Awake()
    {
        normalColor = button.image.color;
        pressButton = GetComponent<PixelPressButton>();
    }

    /* Public PlayWrongPick Method:
     * ~ Called by HandheldPackage when this package is confirmed at the wrong door
     * ~ Shakes the button side to side and flashes it red, so it's clear which pick was wrong
     */
    public void PlayWrongPick()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(WrongPick());
    }

    /* WrongPick Coroutine:
     * ~ Jumps the button one art pixel left and right while tinted red, then puts it back
     * ~ Uses real time, so it still plays if the game is paused while the handheld is open
     */
    private IEnumerator WrongPick()
    {
        RectTransform rect = (RectTransform)transform;
        Vector2 restPosition = rect.anchoredPosition;
        Color startColor = button.image.color;

        float timer = 0f;
        while (timer < wrongTime)
        {
            timer += Time.unscaledDeltaTime;

            // Flips between left and right in whole steps, so the pixel art never lands between pixels
            int side = Mathf.FloorToInt(timer * shakeRate) % 2 == 0 ? 1 : -1;
            rect.anchoredPosition = restPosition + new Vector2(side * shakeDistance, 0f);
            button.image.color = wrongColor;

            yield return null;
        }

        rect.anchoredPosition = restPosition;
        button.image.color = startColor;
    }

    /* Void SetChosen Method:
     * ~ Shows whether this package is the one currently selected in the bag
     * ~ With pixel art buttons the chosen package stays pressed down, like the open tab
     * ~ Without them it falls back to changing the button's colour
     */
    public void SetChosen(bool chosen)
    {
        if (pressButton != null)
        {
            pressButton.SetStuckDown(chosen);
            return;
        }

        button.image.color = chosen ? chosenColor : normalColor;
    }

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