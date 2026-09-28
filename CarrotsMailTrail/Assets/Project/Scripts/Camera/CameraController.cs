using UnityEngine;

public class CameraController : MonoBehaviour
{
    /*Serialize fields: 
     * ~ this allows for the value to be viewed inside the inspector
     */
    // What the camera follows (Carrot is dragged in through the inspector)
    [SerializeField] private Transform target;
    // About how many seconds the camera takes to catch up (larger = looser)
    [SerializeField] private float smoothTime = 0.2f;
    // How far Carrot can move from the centre (in tiles) before the camera follows
    [SerializeField] private Vector2 deadZone = new Vector2(1f, 0.75f);
    // How far the camera looks ahead per unit of Carrot's speed (walking at 4 gives 1.6 tiles)
    [SerializeField] private float lookAheadAmount = 0.4f;
    // The furthest the look ahead can ever reach, in tiles
    [SerializeField] private float maxLookAhead = 3f;
    // How gently the look ahead eases in and out (larger = gentler)
    [SerializeField] private float lookAheadSmoothTime = 0.3f;
    // NEW: The playable area of the map (MapBounds is dragged in through the inspector)
    [SerializeField] private BoxCollider2D mapBounds;
    // NEW: How zoomed in the doorstep camera gets (smaller = closer)
    [SerializeField] private float doorstepOrthoSize = 3f;
    // NEW: How gently the doorstep camera eases in, holds, and eases back out
    [SerializeField] private float doorstepSmoothTime = 0.5f;

    /* Declare private fields: 
     * Fields can only be accessed within the script, cannot be seen outside
     */
    // SmoothDamp's memory of the camera's speed between frames
    private Vector3 velocity;
    // The centre of the dead zone box, which the camera eases toward
    private Vector2 focusPoint;
    // Carrot's physics body, used to read how fast and which way he is moving
    private Rigidbody2D targetBody;
    // How far ahead the camera is currently shifted
    private Vector2 currentLookAhead;
    // SmoothDamp's memory for easing the look ahead shift
    private Vector2 lookAheadVelocity;
    // NEW: This camera, used to read its size and aspect ratio for the clamp
    private Camera cam;
    // NEW: The two states the camera can be in
    private enum CameraState { StreetFollow, Doorstep }
    // NEW: Which state the camera is currently in
    private CameraState state = CameraState.StreetFollow;
    // NEW: The house door being framed while in the Doorstep state (null while on the street)
    private Transform doorstepDoor;
    // NEW: The zoom level to ease back to once the Doorstep state ends
    private float streetOrthoSize;
    // NEW: SmoothDamp's memory of the zoom's speed between frames
    private float orthoVelocity;

    /* Void Start Method:
     * ~ Runs once before the first frame
     * ~ Starts the focus point on Carrot so the camera doesn't drift in from (0, 0)
     * ~ Finds Carrot's Rigidbody2D so the camera can read his velocity
     * ~ Finds this object's Camera so the clamp knows how much of the map is on screen
     */
    private void Start()
    {
        focusPoint = target.position;
        targetBody = target.GetComponent<Rigidbody2D>();
        cam = GetComponent<Camera>();
        streetOrthoSize = cam.orthographicSize;
    }

    /* NEW: Public EnterDoorstep Method:
     * ~ Called by a DeliveryZone when Carrot arrives at its door
     * ~ Switches to the Doorstep state, which eases in and frames Carrot and the door
     */
    public void EnterDoorstep(Transform door)
    {
        state = CameraState.Doorstep;
        doorstepDoor = door;
    }

    /* NEW: Public ExitDoorstep Method:
     * ~ Called by a DeliveryZone when Carrot leaves its door
     * ~ Switches back to the StreetFollow state, which eases the camera back out
     */
    public void ExitDoorstep()
    {
        state = CameraState.StreetFollow;
        doorstepDoor = null;
    }

    /* Void LateUpdate Method:
     * ~ Runs after every Update, so Carrot has already moved this frame (prevents jitter)
     * ~ Hands off to whichever state the camera is currently in
     */
    private void LateUpdate()
    {
        if (state == CameraState.Doorstep && doorstepDoor != null)
        {
            UpdateDoorstep();
        }
        else
        {
            UpdateStreetFollow();
        }
    }

    /* NEW: Void UpdateDoorstep Method:
     * ~ Eases the camera to the midpoint between Carrot and the door, and zooms in to frame both
     * ~ Runs every frame while in the Doorstep state, so it holds steady once it arrives
     */
    private void UpdateDoorstep()
    {
        Vector3 midpoint = (target.position + doorstepDoor.position) / 2f;
        Vector3 desiredPosition = new Vector3(midpoint.x, midpoint.y, transform.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, doorstepSmoothTime);
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, doorstepOrthoSize, ref orthoVelocity, doorstepSmoothTime);
    }

    /* Void UpdateStreetFollow Method:
     * ~ The original follow camera: applies the dead zone, adds look ahead, clamps to the map, then eases toward the result
     * ~ Also eases the zoom back to its normal street level, in case the Doorstep state just ended
     */
    private void UpdateStreetFollow()
    {
        // Dead zone (X): how far Carrot is from the centre of the box sideways
        float differenceX = target.position.x - focusPoint.x;

        // Only moves the focus point when Carrot pushes past the left or right edge
        if (differenceX > deadZone.x)
        {
            focusPoint.x = target.position.x - deadZone.x;
        }
        else if (differenceX < -deadZone.x)
        {
            focusPoint.x = target.position.x + deadZone.x;
        }

        // Dead zone (Y): same check for up and down
        float differenceY = target.position.y - focusPoint.y;

        if (differenceY > deadZone.y)
        {
            focusPoint.y = target.position.y - deadZone.y;
        }
        else if (differenceY < -deadZone.y)
        {
            focusPoint.y = target.position.y + deadZone.y;
        }

        // Look ahead: shift toward where Carrot is moving (faster movement = bigger shift)
        Vector2 targetLookAhead = targetBody.linearVelocity * lookAheadAmount;

        // Caps the shift so it never goes further than maxLookAhead
        targetLookAhead = Vector2.ClampMagnitude(targetLookAhead, maxLookAhead);

        // Eases the shift in and out so turning around doesn't snap the view
        currentLookAhead = Vector2.SmoothDamp(currentLookAhead, targetLookAhead, ref lookAheadVelocity, lookAheadSmoothTime);

        // Where the camera wants to be: the focus point plus the look ahead, keeping the camera's own Z
        Vector3 desiredPosition = new Vector3(focusPoint.x + currentLookAhead.x, focusPoint.y + currentLookAhead.y, transform.position.z);

        // NEW: Map bounds clamp. Half the view's height is the camera's size, half its width is that times the aspect ratio
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // NEW: The furthest the camera's centre can go in each direction without showing past the map
        float minX = mapBounds.bounds.min.x + halfWidth;
        float maxX = mapBounds.bounds.max.x - halfWidth;
        float minY = mapBounds.bounds.min.y + halfHeight;
        float maxY = mapBounds.bounds.max.y - halfHeight;

        // NEW: If the map is narrower than the view, centre the camera on the map instead of clamping
        if (minX > maxX)
        {
            desiredPosition.x = mapBounds.bounds.center.x;
        }
        else
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        }

        // NEW: Same check for height
        if (minY > maxY)
        {
            desiredPosition.y = mapBounds.bounds.center.y;
        }
        else
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // Moves the camera part of the way there each frame, using velocity to stay smooth
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        // NEW: Eases the zoom back to the normal street level (does nothing if it's already there)
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, streetOrthoSize, ref orthoVelocity, smoothTime);
    }

    /* Void OnDrawGizmos Method:
     * ~ Draws the dead zone box and look ahead line in the Scene view (not in the game or the build)
     * ~ Uses the focus point while playing, and the camera's position while editing
     */
    private void OnDrawGizmos()
    {
        Vector3 boxCentre;

        if (Application.isPlaying)
        {
            boxCentre = focusPoint;
        }
        else
        {
            boxCentre = transform.position;
        }

        // Dead zone box: deadZone measures centre to edge, so the full box is twice as big
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCentre, deadZone * 2f);

        // Look ahead line: from the box centre to where the look ahead is pointing
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(boxCentre, boxCentre + (Vector3)currentLookAhead);
    }
}