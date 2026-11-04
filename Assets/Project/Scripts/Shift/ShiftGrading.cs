
using UnityEngine;

/* New Grading System:
 *  In the previous system, the grades were based only on total delivery points, 
 *   which were fixed based on completion rather than customer satisfaction.
 *  
 *  With the new system, grades are calculated using 70% of the points gained via delivery (points are determined on customer patience level),
 *   and 30% of totally delivery speed (time at the end of completing all deliveries), normalized to 0.00-1.00.
 *   
 *  Strikes add penalties to that calculated score:
 *   1 strike = -0.15
 *   2 strikes = -0.40
 *   3 strikes = Automatically result in an F
 *   
 *  Incomplete shifts (time runs out and not all deliveries have been made) give a severe penalty and cannot earn a grade above a D.
 *  
 *  The new grading scale is from F to S instead of F to A.
 *  The final score is then displayed by a multiple of 100 (so a 1.00 is S grade and the displayed score is 1000).
 */

public static class ShiftGrading
{
    /* Public CalculateScore Method:
     * ~ Calculates the final normalized grade from 0.00 to 1.00
     * ~ Uses customer points, remaining time, strikes, and completion
     */
    public static float CalculateScore(
        int earnedPoints,
        int maxPossiblePoints,
        float timeRemaining,
        float shiftDuration,
        int strikes,
        bool allDelivered)
    {
        // Three strikes always results in an F
        if (strikes >= 3)
            return 0f;

        // Normalize customer satisfaction points
        float pointsPerformance = maxPossiblePoints > 0
            ? Mathf.Clamp01((float)earnedPoints / maxPossiblePoints)
            : 0f;

        // Time only contributes if all deliveries were completed
        float timePerformance = 0f;

        if (allDelivered && shiftDuration > 0f)
        {
            float fullTimeThreshold = shiftDuration * 0.30f;

            timePerformance = Mathf.Clamp01(
                timeRemaining / fullTimeThreshold
            );
        }

        // Calculate weighted performance
        float grade =
            (pointsPerformance * 0.70f) +
            (timePerformance * 0.30f);

        // Apply strike deductions
        if (strikes == 1)
            grade -= 0.15f;
        else if (strikes == 2)
            grade -= 0.40f;

        // Apply non-completion penalty
        if (!allDelivered)
        {
            grade -= 0.30f;

            // Incomplete shifts cannot earn above D
            grade = Mathf.Min(grade, 0.30f);
        }

        // Keep result between 0 and 1
        grade = Mathf.Clamp01(grade);

        // Round to exactly two decimal places
        return Mathf.Round(grade * 100f) / 100f;
    }

    /* Public GetLetterGrade Method:
     * ~ Converts the normalized score into a letter grade
     */
    public static string GetLetterGrade(float grade)
    {
        if (grade >= 1.00f)
            return "S";
        if (grade >= 0.80f)
            return "A";
        if (grade >= 0.61f)
            return "B";
        if (grade >= 0.31f)
            return "C";
        if (grade >= 0.16f)
            return "D";

        return "F";
    }
}
