using UnityEngine;
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

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Makes sure the panel starts hidden until the shift actually ends
     */
    private void Start()
    {
        panel.SetActive(false);
    }

    /* Public Show Method:
     * ~ Called by ShiftManager.EndShift()/Win() once the shift is over
     * ~ Displays the final score (with a win/lose framing) and shows the panel
     */
    public void Show(int finalScore, bool won)
    {
        string prefix = won ? "You Win! Final Score: " : "Shift Over — Final Score: ";
        finalScoreText.text = prefix + finalScore;
        panel.SetActive(true);
    }
}
