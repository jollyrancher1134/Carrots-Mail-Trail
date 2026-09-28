using UnityEngine;

// The different moods a customer can be in while waiting (a simple state machine)
public enum CustomerMood { Happy, Impatient, Angry, Gone }

[System.Serializable]
public class Order
{
    /* Order data (hard coded in the OrderManager inspector):
     * ~ One order = one customer, one package, one patience window (matches the GDD taxonomy)
     */
    // Who the package is for
    public string customerName;
    // Which house they live in (must match the delivery zone's House Name exactly)
    public string houseName;
    // What they ordered
    public string packageName;
    // How long they will wait, in seconds
    public float patienceSeconds;
    // Points awarded when this order is delivered
    public int pointValue = 10;

    /* Live patience state (changes during play):
     * ~ Public so it can be watched in the inspector while testing
     */
    // How many seconds of patience are left
    public float timeRemaining;
    // The customer's current mood
    public CustomerMood mood;

    /* Public StartTimer Method:
     * ~ Fills the customer's patience back to full
     * ~ Every customer starts Happy
     */
    public void StartTimer()
    {
        timeRemaining = patienceSeconds;
        mood = CustomerMood.Happy;
    }

    /* Public Tick Method:
     * ~ Called every frame by the OrderManager to count down this customer's patience
     * ~ Picks a mood from how much patience is left, and logs it only when it changes
     */
    public void Tick(float deltaTime)
    {
        // A customer who is Gone has forfeited their order, so their timer stops
        if (mood == CustomerMood.Gone)
        {
            return;
        }

        // Counts down patience (deltaTime keeps it in real seconds on any computer)
        timeRemaining -= deltaTime;

        // How much patience is left, from 1 (full) to 0 (none)
        float fraction = timeRemaining / patienceSeconds;

        // Works out the mood from the lowest threshold up
        CustomerMood newMood;

        if (fraction <= 0f)
        {
            newMood = CustomerMood.Gone;
            timeRemaining = 0f;
        }
        else if (fraction <= 0.25f)
        {
            newMood = CustomerMood.Angry;
        }
        else if (fraction <= 0.5f)
        {
            newMood = CustomerMood.Impatient;
        }
        else
        {
            newMood = CustomerMood.Happy;
        }

        // Only reports the mood when it actually changes, so the Console isn't flooded every frame
        if (newMood != mood)
        {
            Debug.Log(customerName + " is now " + newMood);
            mood = newMood;
        }
    }
}
