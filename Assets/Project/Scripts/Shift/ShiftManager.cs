using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ShiftManager : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // How long one shift lasts, in seconds
    [SerializeField] private float shiftDurationSeconds = 180f;
    // The OrderManager in the scene, used to read the final score
    [SerializeField] private OrderManager orderManager;
    // How many strikes end the run (three strikes, per the GDD)
    [SerializeField] private int maxStrikes = 3;
    // The panel shown when the shift ends (final score and restart)
    [SerializeField] private GameOverPanel gameOverPanel;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
     */
    // Seconds left in the current shift
    private float timeRemaining;
    // True once the shift has ended for any reason (clock ran out, won, or game over)
    private bool shiftOver;
    // How many strikes Carrot has this shift
    private int strikes;
    // True if every order was delivered
    private bool won;

    /* Public read only properties:
     * ~ Let the HUD and UI read the shift state without being able to change it
     */
    public float TimeRemaining
    {
        get { return timeRemaining; }
    }

    public bool IsShiftOver
    {
        get { return shiftOver; }
    }

    public int Strikes
    {
        get { return strikes; }
    }

    public int MaxStrikes
    {
        get { return maxStrikes; }
    }

    public bool IsWon
    {
        get { return won; }
    }

    public bool IsGameOver
    {
        get { return strikes >= maxStrikes; }
    }

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Fills the shift clock, clears strikes, and makes sure the OrderManager and panel are connected
     */
    private void Start()
    {
        timeRemaining = shiftDurationSeconds;
        shiftOver = false;
        strikes = 0;
        won = false;

        // If a field was left empty in the inspector, find it in the scene instead
        if (orderManager == null)
        {
            orderManager = FindFirstObjectByType<OrderManager>();
        }
        if (gameOverPanel == null)
        {
            gameOverPanel = FindFirstObjectByType<GameOverPanel>();
        }
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Checks for restart, then counts down the shift clock and ends the shift at zero
     */
    private void Update()
    {
        // Restart: R on keyboard or View/Back on gamepad (new Input System)
        bool restartKeyboard = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
        bool restartGamepad = Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame;

        if (restartKeyboard || restartGamepad)
        {
            RestartShift();
            return;
        }

        // Once the shift is over, the clock stops
        if (shiftOver)
        {
            return;
        }

        // Counts down the shift clock in real seconds
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndShift("Time's up!");
        }
    }

    /* Public AddStrike Method:
     * ~ Called when Carrot picks the wrong package, or when a customer runs out of patience
     * ~ Three strikes ends the run with a game over
     */
    public void AddStrike()
    {
        if (shiftOver)
        {
            return;
        }

        strikes++;
        Debug.Log("Strike " + strikes + " of " + maxStrikes);

        if (strikes >= maxStrikes)
        {
            EndShift("Game over! Three strikes.");
        }
    }

    /* Public Win Method:
     * ~ Called once every order has been delivered
     * ~ Ends the shift early as a win
     */
    public void Win()
    {
        if (shiftOver)
        {
            return;
        }

        won = true;
        EndShift("Every order delivered!");
    }

    /* Void EndShift Method:
     * ~ Stops the shift, reports why it ended, and shows the game over panel with the final score
     */
    private void EndShift(string reason)
    {
        shiftOver = true;

        int finalScore = 0;
        if (orderManager != null)
        {
            finalScore = orderManager.Score;
        }

        Debug.Log(reason + " Final score: " + finalScore);

        // Shows the end of shift panel
        if (gameOverPanel != null)
        {
            gameOverPanel.Show(finalScore, won);
        }
    }

    /* Public RestartShift Method:
     * ~ Reloads the current scene, so timers, score, strikes and Carrot all start fresh
     * ~ Resets timeScale first, otherwise restarting from the pause menu would load a frozen game
     */
    public void RestartShift()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}