namespace Pathfinding.Enums
{
    /// <summary>
    /// Different types of pathfinding behaviour that uses in AStarPathfinding.
    /// </summary>
    public enum WeightDetectionMode
    {
        None,
        Average, //Heuristic type of damager detection
        OnlyCriticalWeightCheck, // Only critical value of weight matters. For example if weight means damage then it will be lethal check.
        PredictedLethalCheck, // More complex lethalCheck
        SelfPreservationInstinct
    }
}