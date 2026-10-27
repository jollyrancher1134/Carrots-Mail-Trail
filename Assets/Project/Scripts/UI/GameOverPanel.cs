using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverPanel : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The panel to show once the shift ends (final score + Restart button)
    [SerializeField] private GameObject panel;
    // Displays the final score
    [SerializeField] private TMP_Text finalScoreText;
    // Carrot's movement script, disabled while game end so input can't sneak through
    [SerializeField] private PlayerMovement playerMovement;
    // The order/delivery count source, for the deliveries made/missed breakdown
    [SerializeField] private OrderManager orderManager;
    // The ShiftManager, for the Restart button
    [SerializeField] private ShiftManager shiftManager;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Makes sure the panel starts hidden until the shift actually ends
     * ~ Wires the Restart and Quit buttons found under the panel
     */
    private void Start()
    {
        panel.SetActive(false);

        if (orderManager == null)
        {
            orderManager = FindFirstObjectByType<OrderManager>();
        }
        if (shiftManager == null)
        {
            shiftManager = FindFirstObjectByType<ShiftManager>();
        }

        if (shiftManager != null)
        {
            Transform restartButton = panel.transform.Find("RestartButton");
            if (restartButton != null)
            {
                restartButton.GetComponent<Button>().onClick.AddListener(shiftManager.RestartShift);
            }

            Transform quitButton = panel.transform.Find("QuitGameButton");
            if (quitButton != null)
            {
                quitButton.GetComponent<Button>().onClick.AddListener(shiftManager.QuitGame);
            }
        }
    }

    /* Public Show Method:
     * ~ Called by ShiftManager.EndShift()/Win() once the shift is over
     * ~ Displays the final score plus deliveries made/missed (with a win/lose framing),
     *   and shows the panel
     */
    public void Show(int finalScore, bool won)
    {
        string prefix = won ? "You Win! Final Score: " : "Shift Over — Final Score: ";
        string deliveries = "";

        if (orderManager != null)
        {
            int total = orderManager.GetOrders().Count;
            int made = orderManager.DeliveredCount;
            deliveries = "\nDeliveries: " + made + "/" + total + " (missed " + (total - made) + ")";
        }

        finalScoreText.text = prefix + finalScore + deliveries;
        panel.SetActive(true);
        playerMovement.enabled = false;

    }
}
