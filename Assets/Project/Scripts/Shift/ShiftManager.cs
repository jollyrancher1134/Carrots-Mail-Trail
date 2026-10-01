using UnityEngine;
using UnityEngine.SceneManagement;

public class ShiftManager : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // How long a shift lasts, in seconds (set in the inspector)
    [SerializeField] private float shiftDurationSeconds = 180f;
    // The OrderManager in the scene, used to report the final score when the shift ends
    [SerializeField] private OrderManager orderManager;
    // NEW: The game over panel, shown with the final score once the shift ends
    [SerializeField] private GameOverPanel gameOverPanel;
    // NEW: How many wrong picks Carrot can make before the shift ends early
    [SerializeField] private int maxStrikes = 3;

    // How many seconds are left in the current shift
    public float TimeRemaining { get; private set; }
    // Whether the shift is currently running (false once it ends)
    public bool ShiftEnded { get; private set; }
    // NEW: How many wrong picks Carrot has made this shift
    public int Strikes { get; private set; }

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Begins the shift clock
     */
    private void Start()
    {
        TimeRemaining = shiftDurationSeconds;
        ShiftEnded = false;
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Counts down the shift clock and ends the shift at zero
     * ~ Also listens for the restart key so testing doesn't require re-entering play mode
     */
    private void Update()
    {
        if (!ShiftEnded)
        {
            TimeRemaining -= Time.deltaTime;

            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                EndShift();
            }
        }

        // NEW: Press R at any time to restart the shift (stands in for a restart button until UI exists)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartShift();
        }
    }

    /* Public EndShift Method:
     * ~ Stops the clock and reports the final score
     * ~ Safe to call directly too (e.g. later from a "wrong pick" strikes system)
     */
    public void EndShift()
    {
        if (ShiftEnded)
        {
            return;
        }

        ShiftEnded = true;
        int finalScore = orderManager != null ? orderManager.Score : 0;
        Debug.Log("Shift over! Final score: " + finalScore);

        // NEW: Shows the game over panel with the final score, if one is wired up
        if (gameOverPanel != null)
        {
            gameOverPanel.Show(finalScore, won: false);
        }
    }

    /* NEW: Public Win Method:
     * ~ Called by HandheldPackage once every order in the shift has been delivered
     * ~ Ends the shift early with a win, instead of a loss, framing
     */
    public void Win()
    {
        if (ShiftEnded)
        {
            return;
        }

        ShiftEnded = true;
        int finalScore = orderManager != null ? orderManager.Score : 0;
        Debug.Log("All orders delivered! Final score: " + finalScore);

        if (gameOverPanel != null)
        {
            gameOverPanel.Show(finalScore, won: true);
        }
    }

    /* NEW: Public AddStrike Method:
     * ~ Called by HandheldPackage when the wrong package is confirmed
     * ~ Ends the shift early once Carrot runs out of strikes
     */
    public void AddStrike()
    {
        if (ShiftEnded)
        {
            return;
        }

        Strikes++;
        Debug.Log("Wrong pick! Strikes: " + Strikes + "/" + maxStrikes);

        if (Strikes >= maxStrikes)
        {
            EndShift();
        }
    }

    /* Public RestartShift Method:
     * ~ Reloads the current scene, which resets every order, timer, and the score
     */
    public void RestartShift()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
