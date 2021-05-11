using Pathfinding.Base;
using UnityEngine;

namespace Pathfinding.Tests
{
    public static class GridTestHelper
    {
        public static AStarNode[,] CreateGrid(int[,] testGrid, int weight, float nodeSize)
        {
            var grid = new AStarNode[testGrid.GetLength(0), testGrid.GetLength(1)];
            for (var x = 0; x < testGrid.GetLength(0); x++)
            {
                for (var y = 0; y < testGrid.GetLength(1); y++)
                {
                    grid[x, y] = new AStarNode(
                        new Vector2(x * nodeSize, y * nodeSize),
                        new Vector2Int(x, y),
                        testGrid[x, y] == 0,
                        testGrid[x, y] == 1 ? weight : 0);
                }
            }

            return grid;
        }
    }
}