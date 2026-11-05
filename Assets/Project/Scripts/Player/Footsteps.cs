using UnityEngine;

// Makes sure Carrot always has a Rigidbody2D, since steps are counted from how fast he's moving
[RequireComponent(typeof(Rigidbody2D))]
public class Footsteps : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    [Header("Steps")]
    // On: steps come from Animation Events on the landing frames of Carrot's Run clips (frame accurate)
    // Off: steps are counted by distance instead (stepDistance), as a backup if the events are ever removed
    [SerializeField] private bool useAnimationEvents = true;
    // The shortest time between two steps, so blended clips (running diagonally) don't puff twice on one landing
    [SerializeField] private float minStepInterval = 0.12f;
    // How far Carrot walks between footsteps, in world units (only used when useAnimationEvents is off)
    [SerializeField] private float stepDistance = 0.6f;
    // Where Carrot's feet are, measured from his position
    [SerializeField] private Vector2 feetOffset = new Vector2(0f, -0.4f);

    [Header("Dust Puff")]
    // Turns the dust puffs on or off
    [SerializeField] private bool showDust = true;
    // Square particle material (Sprite-Unlit-Default draws each particle as a crisp square)
    [SerializeField] private Material dustMaterial;
    // The dust colour (dusty path beige)
    [SerializeField] private Color dustColor = new Color(0.85f, 0.75f, 0.6f, 0.9f);
    // How many specks puff out per step
    [SerializeField] private int specksPerStep = 3;
    // Draws dust above the paths but under Carrot and his shadow (Carrot is 5, his shadow is 4)
    [SerializeField] private int dustSortingOrder = 3;

    /* Public Events:
     * ~ Other scripts listen to this to do something on every step (audio can play a footstep sound here later)
     * ~ The position of Carrot's feet is passed along
     */
    // Raised every time Carrot takes a step
    public event System.Action<Vector2> Stepped;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // Carrot's Rigidbody2D, read to see how far he moved
    private Rigidbody2D rb;
    // How far Carrot has walked since his last step
    private float distanceSinceStep;
    // The dust particle system built in Start
    private ParticleSystem dust;
    // When the last step happened, used by minStepInterval
    private float lastStepTime = -1f;

    /* Void Awake Method:
     * ~ Runs once when Carrot is loaded, before anything else
     * ~ Finds Carrot's Rigidbody2D
     */
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Builds the dust puff in code, so no extra prefab is needed
     */
    private void Start()
    {
        if (showDust)
        {
            BuildDust();
        }
    }

    /* Void BuildDust Method:
     * ~ Makes a particle system that only spits out a few square specks when told to (one puff per step)
     */
    private void BuildDust()
    {
        GameObject dustObject = new GameObject("Carrot_FootstepDust");

        dust = dustObject.AddComponent<ParticleSystem>();
        dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = dust.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.0625f, 0.125f);
        main.startColor = dustColor;
        main.gravityModifier = -0.05f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Only puffs when told to, never on its own
        ParticleSystem.EmissionModule emission = dust.emission;
        emission.rateOverTime = 0f;

        // Puffs out sideways and a little upwards, like kicked up dirt
        ParticleSystem.ShapeModule shape = dust.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;
        shape.arc = 180f;

        // Shrinks each speck to nothing as it fades
        ParticleSystem.SizeOverLifetimeModule shrink = dust.sizeOverLifetime;
        shrink.enabled = true;
        shrink.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ParticleSystemRenderer dustRenderer = dustObject.GetComponent<ParticleSystemRenderer>();
        dustRenderer.sortingOrder = dustSortingOrder;
        if (dustMaterial != null)
        {
            dustRenderer.sharedMaterial = dustMaterial;
        }
    }

    /* Void FixedUpdate Method:
     * ~ Runs every physics step, the same rate Carrot moves at
     * ~ Adds up how far Carrot walked, and takes a step each time it passes stepDistance
     * ~ Sprinting covers ground faster, so the steps come quicker on their own
     */
    private void FixedUpdate()
    {
        // The Run clips' Animation Events are taking the steps instead
        if (useAnimationEvents)
        {
            return;
        }

        float moved = rb.linearVelocity.magnitude * Time.fixedDeltaTime;

        // Standing still: the next step starts fresh when he moves again
        if (moved <= 0.0001f)
        {
            distanceSinceStep = 0f;
            return;
        }

        distanceSinceStep += moved;
        if (distanceSinceStep >= stepDistance)
        {
            distanceSinceStep -= stepDistance;
            TakeStep();
        }
    }

    /* Public OnFootstep Method:
     * ~ Called by Animation Events on the landing frames of Carrot's Run clips (frames 1 and 5, where his body squashes lowest)
     * ~ Walking reuses the Run clips at 0.6 speed, so walking steps land on the same frames, just slower
     * ~ Ignores events while he's standing still or when two blended clips land on the same frame
     */
    public void OnFootstep()
    {
        if (!useAnimationEvents || rb.linearVelocity.magnitude < 0.1f)
        {
            return;
        }

        if (Time.time - lastStepTime < minStepInterval)
        {
            return;
        }

        TakeStep();
    }

    /* Void TakeStep Method:
     * ~ Puffs dust at Carrot's feet and tells any listeners (like a future footstep sound) that he stepped
     */
    private void TakeStep()
    {
        lastStepTime = Time.time;
        Vector2 feet = (Vector2)transform.position + feetOffset;

        if (dust != null)
        {
            dust.transform.position = feet;
            dust.Emit(specksPerStep);
        }

        if (Stepped != null)
        {
            Stepped(feet);
        }
    }

    /* Void OnDestroy Method:
     * ~ Runs when the scene unloads
     * ~ Cleans up the dust, since it isn't a child of Carrot
     */
    private void OnDestroy()
    {
        if (dust != null)
        {
            Destroy(dust.gameObject);
        }
    }
}
