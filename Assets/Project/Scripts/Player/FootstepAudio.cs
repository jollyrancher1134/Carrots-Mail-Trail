using UnityEngine;
using UnityEngine.Tilemaps;

// Makes sure Carrot always has a Rigidbody2D, since the sound is played from how fast he's moving
[RequireComponent(typeof(Rigidbody2D))]
public class FootstepAudio : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     */
    [Header("Clips")]
    // Running on the dirt paths (a short loop of steps)
    [SerializeField] private AudioClip dirtClip;
    // Running on the grass (a short loop of steps)
    [SerializeField] private AudioClip grassClip;

    [Header("Ground")]
    // The Paths tilemap: if there is a tile under Carrot's feet he's on dirt, otherwise he's on grass
    [SerializeField] private Tilemap dirtTilemap;
    // The name of the dirt path tilemap object, used to find it automatically when the game starts
    [SerializeField] private string dirtTilemapName = "Paths";
    // Where Carrot's feet are, measured from his position
    [SerializeField] private Vector2 feetOffset = new Vector2(0f, -0.4f);

    [Header("Sound")]
    // How loud the footsteps are
    [SerializeField] private float volume = 0.5f;
    // Carrot's walking speed (playing at normal pitch)
    [SerializeField] private float walkSpeed = 4f;
    // Carrot's sprinting speed (playing at sprintPitch)
    [SerializeField] private float sprintSpeed = 7f;
    // How fast the steps sound while sprinting (1 = normal)
    [SerializeField] private float sprintPitch = 1.25f;
    // How quickly the sound fades out when Carrot stops (higher is faster)
    [SerializeField] private float fadeSpeed = 8f;

    [Header("Clip Tuning")]
    // Speeds up the dirt clip, since it's recorded at a walking pace (1 = as recorded)
    [SerializeField] private float dirtPitch = 1.7f;
    // Speeds up the grass clip (1 = as recorded, it's already a running pace)
    [SerializeField] private float grassPitch = 1f;
    // Where the grass loop starts, in seconds (skips the quiet start of the file)
    [SerializeField] private float grassLoopStart = 0.3f;
    // Where the grass loop jumps back to the start, in seconds (cuts off the last crunch, which starts at about 1.87)
    [SerializeField] private float grassLoopEnd = 1.84f;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // Carrot's Rigidbody2D, read to see how fast he's moving
    private Rigidbody2D rb;
    // Plays the looping footstep clip
    private AudioSource source;

    /* Void Awake Method:
     * ~ Runs once when Carrot is loaded, before anything else
     * ~ Finds the Rigidbody2D and makes a looping AudioSource for the steps
     */
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Finds the Paths tilemap by name, so a broken or empty link in the inspector can't stop the footsteps
        GameObject paths = GameObject.Find(dirtTilemapName);
        if (paths != null && paths.TryGetComponent(out Tilemap foundTilemap))
        {
            dirtTilemap = foundTilemap;
        }

        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        // 2D sound, so it's the same volume wherever the camera is
        source.spatialBlend = 0f;
        source.volume = 0f;
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Plays the dirt or grass loop while Carrot is moving, a little faster while sprinting
     * ~ Fades out when he stops, and stops completely once silent
     */
    private void Update()
    {
        float speed = rb.linearVelocity.magnitude;
        bool moving = speed > 0.1f && Time.timeScale > 0f;

        if (moving)
        {
            // Swaps the loop when Carrot steps between dirt and grass
            AudioClip clip = IsOnDirt() ? dirtClip : grassClip;
            if (clip != null && source.clip != clip)
            {
                source.clip = clip;
                source.Play();
                // The grass loop starts after its quiet opening
                if (clip == grassClip)
                {
                    source.time = grassLoopStart;
                }
            }
            if (!source.isPlaying && source.clip != null)
            {
                source.Play();
            }

            // Jumps the grass loop back before it reaches the out of place last crunch
            if (source.clip == grassClip && source.time >= grassLoopEnd)
            {
                source.time = grassLoopStart;
            }

            // Walking plays at the clip's own pitch, sprinting plays faster
            float clipPitch = source.clip == dirtClip ? dirtPitch : grassPitch;
            float sprintAmount = Mathf.InverseLerp(walkSpeed, sprintSpeed, speed);
            source.pitch = clipPitch * Mathf.Lerp(1f, sprintPitch, sprintAmount);
            source.volume = Mathf.MoveTowards(source.volume, volume, fadeSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            source.volume = Mathf.MoveTowards(source.volume, 0f, fadeSpeed * Time.unscaledDeltaTime);
            if (source.volume <= 0f && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    /* Bool IsOnDirt Method:
     * ~ Returns true if there's a path tile under Carrot's feet
     */
    private bool IsOnDirt()
    {
        // No path tilemap found, so everything counts as grass
        if (dirtTilemap == null)
        {
            return false;
        }

        Vector3 feet = transform.position + (Vector3)feetOffset;
        return dirtTilemap.HasTile(dirtTilemap.WorldToCell(feet));
    }
}
