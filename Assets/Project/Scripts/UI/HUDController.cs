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

    [Header("Stamina Bar")]
    // How many art pixels wide the fill is, so it fills one whole pixel at a time (StaminaBar_Fill.png is 38 wide)
    [SerializeField] private int staminaPixels = 38;
    // The fill's color while Carrot can sprint
    [SerializeField] private Color staminaColor = new Color(0.44f, 0.74f, 0.51f);
    // The fill's color while Carrot is on his sprint cooldown (ran out and is recovering)
    [SerializeField] private Color exhaustedColor = new Color(0.9f, 0.35f, 0.25f);
    // The second color the bar blinks to during the cooldown
    [SerializeField] private Color exhaustedBlinkColor = new Color(1f, 0.6f, 0.35f);
    // How many times a second the bar blinks during the cooldown (0 = no blink)
    [SerializeField] private float exhaustedBlinkRate = 4f;

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Keeps the clock, score, strikes, and stamina bar in sync with their source scripts
     */
    private void Update()
    {
        clockText.text = FormatTime(shiftManager.TimeRemaining);
        //Changed "Score: " to "Points: " to better match the grading system
        scoreText.text = "Points: " + orderManager.Score;
        strikesText.text = "Strikes: " + shiftManager.Strikes;
        UpdateStaminaBar();
    }

    /* Void UpdateStaminaBar Method:
     * ~ Fills the bar in whole art pixels so it stays pixelated instead of sliding smoothly
     * ~ Turns the bar red and blinks it while Carrot is on his sprint cooldown, then back to green once he recovers
     */
    private void UpdateStaminaBar()
    {
        // Rounds the fill to the nearest whole pixel of the bar
        float pixels = Mathf.Max(staminaPixels, 1);
        staminaBar.fillAmount = Mathf.Round(playerMovement.StaminaPercent * pixels) / pixels;

        if (playerMovement.IsExhausted)
        {
            // Swaps between the two cooldown colors (unscaled so it still blinks if the game slows)
            bool blinkOn = exhaustedBlinkRate > 0f && Mathf.FloorToInt(Time.unscaledTime * exhaustedBlinkRate * 2f) % 2 == 1;
            staminaBar.color = blinkOn ? exhaustedBlinkColor : exhaustedColor;
        }
        else
        {
            staminaBar.color = staminaColor;
        }
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
