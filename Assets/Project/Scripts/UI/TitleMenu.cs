using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenu : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The gameplay scene to load when Play is pressed (must be added to Build Settings)
    [SerializeField] private string gameplaySceneName = "Testbed";
    // The Play button, wired to PlayGame
    [SerializeField] private Button playButton;
    // The Quit button, wired to QuitGame
    [SerializeField] private Button quitButton;

    /* Void Awake Method:
     * ~ Runs once when the title scene loads
     * ~ Wires the buttons here instead of through the inspector's persistent calls,
     *   so re-wiring a button later is just dragging a different reference, nothing to re-click
     */
    private void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGame);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    /* Public PlayGame Method:
     * ~ Loads the gameplay scene, starting the shift
     */
    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    /* Public QuitGame Method:
     * ~ Closes the app in a built game, or stops play mode when testing inside the Unity editor
     */
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
