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

    // How many seconds are left in the current shift
    public float TimeRemaining { get; private set; }
    // Whether the shift is currently running (false once it ends)
    public bool ShiftEnded { get; private set; }

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
