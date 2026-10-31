using UnityEngine;
using UnityEngine.InputSystem;

public class HandheldInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HandheldPackage handheldPackage;

    // Tracks which doorstep Carrot is currently standing at
    private DeliveryZone currentZone;

    /* Update Method:
     * ~ Listens for Tab on keyboard or Y on gamepad
     * ~ Opens or closes the handheld
     */
    private void Update()
    {
        bool togglePressed =
            (Keyboard.current != null &&
             Keyboard.current.tabKey.wasPressedThisFrame)
            ||
            (Gamepad.current != null &&
             Gamepad.current.buttonNorth.wasPressedThisFrame);

        if (!togglePressed)
            return;

        if (handheldPackage.IsOpen)
        {
            handheldPackage.Close();
        }
        else if (currentZone != null)
        {
            Order order = currentZone.GetCurrentOrder();

            if (order != null)
                handheldPackage.Open(order);
            else
                handheldPackage.OpenEmpty();
        }
        else
        {
            handheldPackage.OpenEmpty();
        }
    }

    /* SetCurrentZone Method:
     * ~ Remembers the doorstep Carrot entered
     */
    public void SetCurrentZone(DeliveryZone zone)
    {
        currentZone = zone;
    }

    /* ClearCurrentZone Method:
     * ~ Forgets the doorstep Carrot left
     */
    public void ClearCurrentZone(DeliveryZone zone)
    {
        if (currentZone == zone)
            currentZone = null;
    }
}