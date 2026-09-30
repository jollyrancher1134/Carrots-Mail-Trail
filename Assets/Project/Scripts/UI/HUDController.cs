using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The shift's clock and score source
    [SerializeField] private ShiftManager shiftManager;
    // The order/score source
    [SerializeField] private OrderManager orderManager;
    // Carrot's stamina source
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Display")]
    // Shows the shift's remaining time, formatted mm:ss
    [SerializeField] private TMP_Text clockText;
    // Shows the running score
    [SerializeField] private TMP_Text scoreText;
    // Shows the strike count, driven live by ShiftManager.Strikes
    [SerializeField] private TMP_Text strikesText;
    // Fills to match Carrot's current stamina percentage
    [SerializeField] private Image staminaBar;

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Keeps the clock, score, strikes, and stamina bar in sync with their source scripts
     */
    private void Update()
    {
        clockText.text = FormatTime(shiftManager.TimeRemaining);
        scoreText.text = "Score: " + orderManager.Score;
        strikesText.text = "Strikes: " + shiftManager.Strikes;
        staminaBar.fillAmount = playerMovement.StaminaPercent;
    }

    /* Private FormatTime Method:
     * ~ Turns a seconds value into an mm:ss display string
     */
    private string FormatTime(float seconds)
    {
        int wholeSeconds = Mathf.CeilToInt(Mathf.Max(seconds, 0f));
        int minutes = wholeSeconds / 60;
        int remainingSeconds = wholeSeconds % 60;
        return minutes.ToString("00") + ":" + remainingSeconds.ToString("00");
    }
}
