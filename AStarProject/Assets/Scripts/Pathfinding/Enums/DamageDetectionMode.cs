namespace Pathfinding.Enums
{
    /// <summary>
    /// Different types of pathfinding behaviour that uses in AStarPathfinding.
    /// </summary>
    public enum DamageDetectionMode
    {
        None,
        Average, //Heuristic type of damager detection
        LethalCheck, // Only lethal damager will matters
        PredictedLethalCheck, // More complex lethalCheck
        SelfPreservationInstinct
    }
}