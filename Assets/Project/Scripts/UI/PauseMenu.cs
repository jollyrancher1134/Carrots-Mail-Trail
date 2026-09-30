using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The panel to show while paused (PAUSED text + Resume button)
    [SerializeField] private GameObject panel;
    // Carrot's movement script, disabled while paused so input can't sneak through
    [SerializeField] private PlayerMovement playerMovement;

    // Whether the game is currently paused
    private bool isPaused;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Makes sure the game starts unpaused with the panel hidden
     */
    private void Start()
    {
        panel.SetActive(false);
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Watches for Escape to toggle pause, using the new Input System directly
     *   since this is a one-off menu toggle rather than a gameplay action worth
     *   adding to the shared Input Actions asset
     */
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    /* Public Pause Method:
     * ~ Freezes gameplay and shows the pause panel
     * ~ Time.timeScale = 0 stops every script that counts time with Time.deltaTime
     *   (shift clock, patience timers) for free; PlayerMovement is disabled on top
     *   since it also reads input directly in Update, which timeScale alone won't stop
     */
    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        playerMovement.enabled = false;
        panel.SetActive(true);
    }

    /* Public Resume Method:
     * ~ Called by the panel's Resume button, or by pressing Escape again
     * ~ Unfreezes gameplay exactly where it left off
     */
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        playerMovement.enabled = true;
        panel.SetActive(false);
    }
}
