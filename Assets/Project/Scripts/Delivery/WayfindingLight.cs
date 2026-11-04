using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WayfindingLight : MonoBehaviour
{
    // The two kinds of wayfinding light: the post office glow, or a lamp at a customer's door
    public enum LightPurpose
    {
        // Glows until Carrot picks up the parcels, so the first stop is obvious
        PostOffice,
        // Glows while this house's parcel is in the bag and still waiting to be delivered
        Door
    }

    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    [Header("Purpose")]
    // Whether this light guides Carrot to the post office or to a customer's door
    [SerializeField] private LightPurpose purpose = LightPurpose.Door;

    [Header("Light")]
    // Where the light sits, measured from this object (just above the door)
    [SerializeField] private Vector2 lightOffset = new Vector2(0f, 1f);
    // The light colour (warm lamp orange)
    [SerializeField] private Color lightColor = new Color(1f, 0.75f, 0.4f, 1f);
    // How bright the light is when fully on
    [SerializeField] private float onIntensity = 1.2f;
    // The fully bright middle of the light, in world units
    [SerializeField] private float innerRadius = 0.5f;
    // How far the glow reaches, in world units
    [SerializeField] private float outerRadius = 3.5f;
    // How quickly the light fades on and off (higher is faster)
    [SerializeField] private float fadeSpeed = 3f;
    // How much the lamp flickers like a candle (0 = steady)
    [SerializeField] private float flickerAmount = 0.08f;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // The OrderManager in the scene, used to check the parcels and this house's order
    private OrderManager orderManager;
    // The ShiftManager in the scene, used to turn every light off once the shift is over
    private ShiftManager shiftManager;
    // This door's delivery zone (only used for Door lights)
    private DeliveryZone zone;
    // The Light2D built in Start
    private Light2D lamp;
    // How far the light is faded in, from 0 (off) to 1 (fully on)
    private float brightness;
    // A random start for the flicker, so neighbouring lamps don't flicker in step
    private float flickerSeed;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Finds the managers and builds the light (off until it's needed)
     */
    private void Start()
    {
        orderManager = FindFirstObjectByType<OrderManager>();
        shiftManager = FindFirstObjectByType<ShiftManager>();
        zone = GetComponent<DeliveryZone>();
        flickerSeed = Random.Range(0f, 100f);

        BuildLight();
    }

    /* Void BuildLight Method:
     * ~ Makes the light in code, instead of as a child of this object
     * ~ Delivery zones are scaled up to make their triggers bigger, so a child light would come out stretched
     */
    private void BuildLight()
    {
        GameObject lightObject = new GameObject(name + "_WayfindingLight");
        lightObject.transform.position = transform.position + (Vector3)lightOffset;

        lamp = lightObject.AddComponent<Light2D>();
        lamp.lightType = Light2D.LightType.Point;
        lamp.color = lightColor;
        lamp.pointLightInnerRadius = innerRadius;
        lamp.pointLightOuterRadius = outerRadius;
        lamp.falloffIntensity = 0.6f;
        lamp.shadowsEnabled = false;
        lamp.intensity = 0f;
        lamp.enabled = false;
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Fades the light towards on or off depending on whether Carrot needs to come here
     * ~ Switches the Light2D off completely while dark, so unused lamps cost nothing
     */
    private void Update()
    {
        if (lamp == null)
        {
            return;
        }

        float target = ShouldBeOn() ? 1f : 0f;
        brightness = Mathf.MoveTowards(brightness, target, fadeSpeed * Time.deltaTime);

        lamp.enabled = brightness > 0f;
        if (!lamp.enabled)
        {
            return;
        }

        // Perlin noise gives a soft, uneven flicker instead of a regular pulse
        float flicker = 1f + (Mathf.PerlinNoise(flickerSeed, Time.time * 3f) - 0.5f) * 2f * flickerAmount;
        lamp.intensity = onIntensity * brightness * flicker;
    }

    /* Bool ShouldBeOn Method:
     * ~ Returns true if this light should be glowing right now
     * ~ Post office: until the parcels are picked up
     * ~ Door: once the parcels are picked up, until this house's order is delivered or the customer is Gone
     */
    private bool ShouldBeOn()
    {
        if (orderManager == null)
        {
            return false;
        }

        // The shift has ended, so there is nowhere left to go
        if (shiftManager != null && shiftManager.IsShiftOver)
        {
            return false;
        }

        if (purpose == LightPurpose.PostOffice)
        {
            return !orderManager.ParcelsCollected;
        }

        // A door with no zone can't know which house it is
        if (zone == null)
        {
            return false;
        }

        // GetOrderForHouse hands back null once the order is delivered or the customer is Gone
        return orderManager.ParcelsCollected && orderManager.GetOrderForHouse(zone.HouseName) != null;
    }

    /* Void OnDestroy Method:
     * ~ Runs when the scene unloads
     * ~ Cleans up the light, since it isn't a child of this object
     */
    private void OnDestroy()
    {
        if (lamp != null)
        {
            Destroy(lamp.gameObject);
        }
    }
}
