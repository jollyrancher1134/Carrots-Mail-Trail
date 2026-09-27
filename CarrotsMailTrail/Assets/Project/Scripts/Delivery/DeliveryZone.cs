using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    /*Serialize fields: 
     * ~ this allows for the value to be viewed inside the inspector
     */
    // The name of the house this zone belongs to (typed in the inspector for each zone)
    [SerializeField] private string houseName;

    /* Void OnTriggerEnter2D:
     * ~ Unity calls this automatically when another collider enters this zone's trigger
     * ~ Checks the Player tag so only Carrot counts as arriving
     */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Carrot has arrived at " + houseName);
        }
    }

    /* Void OnTriggerExit2D:
     * ~ Unity calls this automatically when another collider leaves this zone's trigger
     * ~ Later this will close the package list and return to the street camera
     */
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Carrot has left " + houseName);
        }
    }
}