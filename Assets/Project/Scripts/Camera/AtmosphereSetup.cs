using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Makes sure this sits on the camera, since it turns on the camera's post processing
[RequireComponent(typeof(Camera))]
public class AtmosphereSetup : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    [Header("Bloom")]
    // How strongly bright spots (lamps, sparkles) glow
    [SerializeField] private float bloomIntensity = 0.5f;
    // How bright something has to be before it starts to glow (above 1 keeps plain white art from glowing)
    [SerializeField] private float bloomThreshold = 1f;

    [Header("Vignette")]
    // How dark the screen's edges get (0 = none, 0.3 is a soft frame)
    [SerializeField] private float vignetteIntensity = 0.28f;
    // The colour the edges fade into (warm dark brown instead of black)
    [SerializeField] private Color vignetteColor = new Color(0.2f, 0.1f, 0.08f, 1f);

    [Header("Colour")]
    // A warm tint over the whole village, like late afternoon
    [SerializeField] private Color warmFilter = new Color(1f, 0.96f, 0.9f, 1f);
    // Pushes colours a little richer (0 = unchanged)
    [SerializeField] private float saturation = 10f;

    [Header("Pollen")]
    // Turns the drifting pollen on or off
    [SerializeField] private bool showPollen = true;
    // Square particle material (Sprite-Unlit-Default draws each particle as a crisp square)
    [SerializeField] private Material pollenMaterial;
    // The pollen colour (soft yellow)
    [SerializeField] private Color pollenColor = new Color(1f, 0.93f, 0.6f, 0.8f);
    // How many pollen specks float on screen at once
    [SerializeField] private int pollenCount = 40;
    // The size of each speck, in world units (0.0625 = one art pixel at 16 pixels per unit)
    [SerializeField] private float pollenSize = 0.0625f;
    // The area around the camera pollen floats in, in world units
    [SerializeField] private Vector2 pollenArea = new Vector2(24f, 14f);
    // Draws pollen above the village but under the speech bubbles and prompts
    [SerializeField] private int pollenSortingOrder = 90;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Builds the post processing look and the pollen in code, so no extra assets are needed
     */
    private void Start()
    {
        BuildPostProcessing();

        if (showPollen)
        {
            BuildPollen();
        }
    }

    /* Void BuildPostProcessing Method:
     * ~ Turns on the camera's post processing, then makes a global Volume with bloom, a vignette and a warm colour grade
     * ~ The profile is made in code, so every value can be tuned from this script's inspector
     */
    private void BuildPostProcessing()
    {
        // Post processing is off on cameras by default
        UniversalAdditionalCameraData cameraData = GetComponent<Camera>().GetUniversalAdditionalCameraData();
        if (cameraData != null)
        {
            cameraData.renderPostProcessing = true;
        }

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();

        Bloom bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(bloomIntensity);
        bloom.threshold.Override(bloomThreshold);

        Vignette vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(vignetteIntensity);
        vignette.color.Override(vignetteColor);
        vignette.smoothness.Override(0.4f);

        ColorAdjustments colour = profile.Add<ColorAdjustments>(true);
        colour.colorFilter.Override(warmFilter);
        colour.saturation.Override(saturation);

        GameObject volumeObject = new GameObject("Atmosphere_Volume");
        Volume volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = profile;
    }

    /* Void BuildPollen Method:
     * ~ Makes a particle system of tiny square specks that drift slowly around the camera
     * ~ It's a child of the camera so it always fills the screen, but the specks float in the world,
     *   so walking past them feels like moving through the air
     */
    private void BuildPollen()
    {
        GameObject pollenObject = new GameObject("Atmosphere_Pollen");
        pollenObject.transform.SetParent(transform, false);
        pollenObject.transform.localPosition = new Vector3(0f, 0f, 10f);

        ParticleSystem pollen = pollenObject.AddComponent<ParticleSystem>();
        pollen.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = pollen.main;
        main.loop = true;
        main.duration = 5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        main.startSize = pollenSize;
        main.startColor = pollenColor;
        main.maxParticles = pollenCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.prewarm = true;

        ParticleSystem.EmissionModule emission = pollen.emission;
        emission.rateOverTime = pollenCount / 8f;

        // Spawns specks anywhere inside a flat box the size of the screen
        ParticleSystem.ShapeModule shape = pollen.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(pollenArea.x, pollenArea.y, 0f);

        // A gentle wander, so specks drift instead of flying in straight lines
        ParticleSystem.NoiseModule noise = pollen.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.3f;
        noise.scrollSpeed = 0.2f;

        // Fades each speck in and out, so they never pop
        ParticleSystem.ColorOverLifetimeModule fade = pollen.colorOverLifetime;
        fade.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
        fade.color = gradient;

        ParticleSystemRenderer pollenRenderer = pollenObject.GetComponent<ParticleSystemRenderer>();
        pollenRenderer.sortingOrder = pollenSortingOrder;
        if (pollenMaterial != null)
        {
            pollenRenderer.sharedMaterial = pollenMaterial;
        }

        pollen.Play();
    }
}
