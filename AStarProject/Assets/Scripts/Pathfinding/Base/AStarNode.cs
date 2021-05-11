using Pathfinding.Components;
using UnityEngine;

namespace Pathfinding.Base
{
    public class AStarNode
    {
        public Vector2Int GridPosition;
        public Vector2 Center;
        public bool Walkable { get; set; }
        public float WeightValue { get; set; }
        public bool Weightable => WeightValue > 0;

        public AStarNode() { }
        public AStarNode(Vector2 center, bool walkable, float weightValue)
        {
            WeightValue = weightValue;
            Center = center;
            Walkable = walkable;
        }
        
        [TestOnly]
        public AStarNode(Vector2 center, Vector2Int gridPosition, bool walkable, float weightValue)
        {
            WeightValue = weightValue;
            Center = center;
            Walkable = walkable;
            GridPosition = gridPosition;
        }
    }
}
