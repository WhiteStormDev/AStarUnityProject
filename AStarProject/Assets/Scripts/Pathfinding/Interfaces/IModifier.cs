using System.Collections.Generic;
using Pathfinding.Base;

namespace Pathfinding.Interfaces
{
    /// <summary>
    /// You can right your own modifiers.
    /// This is path postprocessing tool that takes ready path from AStarPathfinding.
    /// All modifiers will be applied in classic order (scripts order on GameObject).
    /// </summary>
    public interface IModifier
    {
        /// <summary>
        /// Apply modifier to path
        /// </summary>
        /// <param name="path">Current aStar path</param>
        /// <returns>Returns modified path</returns>
        List<AStarPathNode> ApplyModifier(List<AStarPathNode> path);
    }
}