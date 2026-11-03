using UnityEngine;

public class ActionLock : StateMachineBehaviour
{
    /* ActionLock State Machine Behaviour:
     * ~ Added to the PickUp and Deliver states in Carrot's Animator (Add Behaviour in the Inspector)
     * ~ The Animator tells PlayerMovement when an action starts and ends, instead of a script
     *   checking the current animation state every frame (the polling anti-pattern from W6)
     */

    /* Void OnStateEnter Method:
     * ~ Unity calls this on the first frame the action state becomes active
     * ~ Locks Carrot's movement so the action can't be walked out of
     */
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerMovement playerMovement = animator.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.SetBusy(true);
            Debug.Log("Carrot started an action state, movement locked");
        }
    }

    /* Void OnStateExit Method:
     * ~ Unity calls this once the action state has finished its exit transition back to Idle
     * ~ Unlocks Carrot's movement, so any buffered input can be used straight away
     */
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerMovement playerMovement = animator.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.SetBusy(false);
            Debug.Log("Carrot finished an action state, movement unlocked");
        }
    }
}