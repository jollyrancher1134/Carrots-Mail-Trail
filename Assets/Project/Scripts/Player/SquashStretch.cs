using System.Collections;
using UnityEngine;

// A quick squash-then-spring-back pulse on Carrot's own scale, for PickUp/Deliver juice.
// Captures the authored scale once and always animates relative to it, so it never drifts.
public class SquashStretch : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    // How long one pulse takes, start to finish
    [SerializeField] private float duration = 0.15f;
    // How much to squash on X / stretch on Y at the peak of the pulse
    [SerializeField] private float squashAmount = 0.2f;

    // Carrot's authored scale, captured once so the pulse always returns to the right place
    private Vector3 restScale;
    // So a new pulse can cleanly interrupt one already playing
    private Coroutine activeRoutine;

    private void Awake()
    {
        restScale = transform.localScale;
    }

    /* Public Pulse Method:
     * ~ Called by HandheldPackage on picking up a package (select) or delivering one (confirm)
     * ~ Squashes wide and short, then springs back to the authored scale
     */
    public void Pulse()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }
        activeRoutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        Vector3 squashedScale = new Vector3(
            restScale.x * (1f + squashAmount),
            restScale.y * (1f - squashAmount),
            restScale.z);

        float elapsed = 0f;
        float half = duration / 2f;

        // Squash out
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(restScale, squashedScale, elapsed / half);
            yield return null;
        }

        // Spring back to rest
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squashedScale, restScale, elapsed / half);
            yield return null;
        }

        transform.localScale = restScale;
        activeRoutine = null;
    }
}
