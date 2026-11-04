using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameAudio : MonoBehaviour
{
    /*Serialize fields:
     * ~ this allows for the value to be viewed inside the inspector
     * ~ Every clip is optional: an empty slot just stays silent, so clips can be dropped in one at a time
     */
    [Header("Scene References")]
    // The shift's clock, used for the halfway tone, low time warning and end of shift sounds (auto-found if left empty)
    [SerializeField] private ShiftManager shiftManager;
    // Raises the delivered and wrong pick events (auto-found if left empty)
    [SerializeField] private OrderManager orderManager;

    [Header("Music")]
    // The background music, looped for the whole shift
    [SerializeField] private AudioClip musicClip;
    // How loud the music is
    [SerializeField] private float musicVolume = 0.4f;
    // How fast the music plays once time is running low (1 = normal)
    [SerializeField] private float lowTimeMusicPitch = 1.2f;
    // How quickly the music speeds up once time is running low (higher is faster)
    [SerializeField] private float musicSpeedUpRate = 0.5f;
    // How quickly the music fades out when the shift ends (higher is faster)
    [SerializeField] private float musicFadeRate = 0.5f;

    [Header("UI")]
    // Played whenever any button is clicked or pressed with the keyboard or gamepad
    [SerializeField] private AudioClip uiClickClip;

    [Header("Deliveries")]
    // Played when the right package is delivered to a Happy customer
    [SerializeField] private AudioClip goodDeliveryClip;
    // Played when the right package is delivered, but the customer was already Impatient or Angry
    [SerializeField] private AudioClip poorDeliveryClip;
    // Played when the wrong package is confirmed at a door
    [SerializeField] private AudioClip wrongPickClip;

    [Header("Shift Clock")]
    // Played once when half the shift's time is gone
    [SerializeField] private AudioClip halfwayClip;
    // Played once when time is running low, as the music speeds up
    [SerializeField] private AudioClip lowTimeClip;
    // How many seconds left counts as running low
    [SerializeField] private float lowTimeSeconds = 30f;
    // Played when the shift ends with every order delivered
    [SerializeField] private AudioClip shiftWinClip;
    // Played when the shift ends any other way (time's up, three strikes, or no customers left)
    [SerializeField] private AudioClip shiftEndClip;

    [Header("Volume")]
    // How loud the sound effects are
    [SerializeField] private float sfxVolume = 0.8f;

    /* Declare private fields:
     * Fields can only be accessed within the script, cannot be seen outside
    */
    // Plays the looping music
    private AudioSource musicSource;
    // Plays every one shot sound effect
    private AudioSource sfxSource;
    // Buttons that already play the click, so none get it twice
    private readonly HashSet<Button> hookedButtons = new HashSet<Button>();
    // Counts down to the next check for new buttons (bag packages are made after the scene starts)
    private float buttonScanTimer;
    // True once each one time sound has played this shift
    private bool playedHalfway;
    private bool playedLowTime;
    private bool playedShiftEnd;

    /* Void Awake Method:
     * ~ Runs once when the scene loads, before anything else
     * ~ Makes one AudioSource for music and one for sound effects, both 2D so they're the same volume everywhere
     */
    private void Awake()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
    }

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Finds any empty scene references, listens for deliveries and wrong picks, and starts the music
     */
    private void Start()
    {
        // If a field was left empty in the inspector, find it in the scene instead
        if (shiftManager == null)
        {
            shiftManager = FindFirstObjectByType<ShiftManager>();
        }
        if (orderManager == null)
        {
            orderManager = FindFirstObjectByType<OrderManager>();
        }

        if (orderManager != null)
        {
            orderManager.OrderDelivered += OnOrderDelivered;
            orderManager.WrongPick += OnWrongPick;
        }

        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }

        HookNewButtons();
    }

    /* Void OnDestroy Method:
     * ~ Stops listening to the OrderManager when the scene unloads (restart or quit to menu)
     */
    private void OnDestroy()
    {
        if (orderManager != null)
        {
            orderManager.OrderDelivered -= OnOrderDelivered;
            orderManager.WrongPick -= OnWrongPick;
        }
    }

    /* Void Update Method:
     * ~ Runs every frame
     * ~ Hooks the click sound onto any new buttons, and plays the shift clock sounds at the right moments
     */
    private void Update()
    {
        // Looks for new buttons twice a second (unscaled, so it still works while paused)
        buttonScanTimer -= Time.unscaledDeltaTime;
        if (buttonScanTimer <= 0f)
        {
            buttonScanTimer = 0.5f;
            HookNewButtons();
        }

        if (shiftManager == null)
        {
            return;
        }

        // Shift over: plays the win fanfare or the end sound once, then fades the music out
        if (shiftManager.IsShiftOver)
        {
            if (!playedShiftEnd)
            {
                playedShiftEnd = true;
                PlaySfx(shiftManager.IsWon ? shiftWinClip : shiftEndClip);
            }
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, 0f, musicFadeRate * Time.unscaledDeltaTime);
            return;
        }

        // The clock hasn't started yet (Carrot is still at the post office)
        if (!shiftManager.IsShiftStarted)
        {
            return;
        }

        float timeLeft = shiftManager.TimeRemaining;

        // Halfway tone, once
        if (!playedHalfway && timeLeft <= shiftManager.ShiftDuration * 0.5f)
        {
            playedHalfway = true;
            PlaySfx(halfwayClip);
        }

        // Low time warning, once, and the music speeds up from then on
        if (timeLeft <= lowTimeSeconds)
        {
            if (!playedLowTime)
            {
                playedLowTime = true;
                PlaySfx(lowTimeClip);
            }
            musicSource.pitch = Mathf.MoveTowards(musicSource.pitch, lowTimeMusicPitch, musicSpeedUpRate * Time.deltaTime);
        }
    }

    /* Void HookNewButtons Method:
     * ~ Finds every button in the scene (including hidden ones) and gives each new one the click sound
     */
    private void HookNewButtons()
    {
        if (uiClickClip == null)
        {
            return;
        }

        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (hookedButtons.Add(button))
            {
                button.onClick.AddListener(PlayClick);
            }
        }
    }

    /* Void PlayClick Method:
     * ~ Plays the UI click sound
     */
    private void PlayClick()
    {
        PlaySfx(uiClickClip);
    }

    /* Void OnOrderDelivered Method:
     * ~ Runs when the right package is delivered
     * ~ A Happy customer gets the good cue, an Impatient or Angry one gets the poor cue
     */
    private void OnOrderDelivered(Order order)
    {
        bool happy = order != null && order.mood == CustomerMood.Happy;
        PlaySfx(happy ? goodDeliveryClip : poorDeliveryClip);
    }

    /* Void OnWrongPick Method:
     * ~ Runs when the wrong package is confirmed at a door
     */
    private void OnWrongPick(Order order)
    {
        PlaySfx(wrongPickClip);
    }

    /* Void PlaySfx Method:
     * ~ Plays a one shot sound effect if the clip has been assigned
     * ~ Safe to call with an empty slot, so missing clips just stay silent
     */
    private void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
}
