using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The panel to show while paused (PAUSED text + Resume button)
    [SerializeField] private GameObject panel;
    // Carrot's movement script, disabled while paused so input can't sneak through
    [SerializeField] private PlayerMovement playerMovement;
    // Tells us whether the shift has already ended, so pause can't revive it
    [SerializeField] private ShiftManager shiftManager;

    // Whether the game is currently paused
    private bool isPaused;
    // The handheld's CanvasGroup, disabled while paused so its buttons can't be clicked through
    private CanvasGroup handheldCanvasGroup;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Makes sure the game starts unpaused with the panel hidden
     * ~ Wires the Resume and Quit buttons found under the panel, and grabs (or adds)
     *   a CanvasGroup on the handheld so pausing can block its clicks
     */
    private void Start()
    {
        panel.SetActive(false);

        if (shiftManager == null)
        {
            shiftManager = FindFirstObjectByType<ShiftManager>();
        }

        Transform resumeButton = panel.transform.Find("ResumeButton");
        if (resumeButton != null)
        {
            resumeButton.GetComponent<Button>().onClick.AddListener(Resume);
        }

        Transform quitButton = panel.transform.Find("QuitGameButton");
        if (quitButton != null && shiftManager != null)
        {
            quitButton.GetComponent<Button>().onClick.AddListener(shiftManager.QuitGame);
        }

        Transform restartButton = panel.transform.Find("RestartButton");
        if (restartButton != null && shiftManager != null)
        {
            restartButton.GetComponent<Button>().onClick.AddListener(shiftManager.RestartShift);
        }

        HandheldPackage handheld = FindFirstObjectByType<HandheldPackage>(FindObjectsInactive.Include);
        if (handheld != null)
        {
            handheldCanvasGroup = handheld.GetComponent<CanvasGroup>();
            if (handheldCanvasGroup == null)
            {
                handheldCanvasGroup = handheld.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Watches for Escape (keyboard) or Start (gamepad) to toggle pause
     * ~ Once the shift has ended, pause can't be opened or closed anymore -
     *   the game over/win screen already owns Carrot's movement at that point
     */
    private void Update()
    {
        if (shiftManager != null && shiftManager.IsShiftOver)
        {
            return;
        }

        bool togglePressed =
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

        if (togglePressed)
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

        if (handheldCanvasGroup != null)
        {
            handheldCanvasGroup.interactable = false;
        }
    }

    /* Public Resume Method:
     * ~ Called by the panel's Resume button, or by pressing Escape/gamepad Start again
     * ~ Unfreezes gameplay exactly where it left off
     */
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        playerMovement.enabled = true;
        panel.SetActive(false);

        if (handheldCanvasGroup != null)
        {
            handheldCanvasGroup.interactable = true;
        }
    }
}
