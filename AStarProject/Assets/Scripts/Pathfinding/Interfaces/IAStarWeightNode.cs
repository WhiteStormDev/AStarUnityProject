namespace Pathfinding.Interfaces
{
    /// <summary>
    /// The WeightNode interface.
    /// If you want to use your own Game weight nodes in pathfinding algorithm they must be nested from IAStarWeightNode
    /// and have Weight Layer that you set in AStarPathfinding gameObject settings.
    /// </summary>
    public interface IAStarWeightNode
    {
        float Weight { get; }
    }
}
