using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LanternLight : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     * ~ Select a lantern in the Scene view to see a circle showing how far its glow reaches
     */
    [Header("Light")]
    // Where the glow sits, measured from the bottom of the post (the glass)
    [SerializeField] private Vector2 lightOffset = new Vector2(0f, 1.45f);
    // The glow colour (soft blue to match the glass)
    [SerializeField] private Color lightColor = new Color(0.65f, 0.85f, 1f, 1f);
    // How bright the lantern is
    [SerializeField] private float intensity = 1f;
    // The fully bright middle of the glow, in world units
    [SerializeField] private float innerRadius = 0.3f;
    // How far the glow reaches, in world units
    [SerializeField] private float outerRadius = 3f;
    // How much the lantern flickers like a flame (0 = steady)
    [SerializeField] private float flickerAmount = 0.06f;
    // How fast the flicker moves (higher is jumpier)
    [SerializeField] private float flickerSpeed = 2.5f;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // The Light2D built in Start
    private Light2D lamp;
    // A random start for the flicker, so neighbouring lanterns don't flicker in step
    private float flickerSeed;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Builds the glow as a child of the lantern, so moving the lantern moves its light too
     */
    private void Start()
    {
        flickerSeed = Random.Range(0f, 100f);

        GameObject lightObject = new GameObject("Glow");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = lightOffset;

        lamp = lightObject.AddComponent<Light2D>();
        lamp.lightType = Light2D.LightType.Point;
        lamp.color = lightColor;
        lamp.pointLightInnerRadius = innerRadius;
        lamp.pointLightOuterRadius = outerRadius;
        lamp.falloffIntensity = 0.6f;
        lamp.shadowsEnabled = false;
        lamp.intensity = intensity;
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Gently flickers the glow (Perlin noise gives a soft, uneven flicker instead of a regular pulse)
     */
    private void Update()
    {
        if (lamp == null)
        {
            return;
        }

        float flicker = 1f + (Mathf.PerlinNoise(flickerSeed, Time.time * flickerSpeed) - 0.5f) * 2f * flickerAmount;
        lamp.intensity = intensity * flicker;
    }

    /* Void OnDrawGizmosSelected Method:
     * ~ Draws the glow's reach in the Scene view while the lantern is selected (not in the game or the build)
     * ~ The light itself is only made in Play mode, so this helps with spacing lanterns out while placing them
     */
    private void OnDrawGizmosSelected()
    {
        Vector3 centre = transform.position + (Vector3)lightOffset;
        Gizmos.color = lightColor;
        Gizmos.DrawWireSphere(centre, outerRadius);
        Gizmos.DrawWireSphere(centre, innerRadius);
    }
}
