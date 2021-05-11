using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pathfinding.Components;
using Pathfinding.Configs;
using Pathfinding.Enums;
using Pathfinding.Interfaces;
using Pathfinding.MonoBehaviours;
using Pathfinding.MonoBehaviours.Agent;
using UnityEngine;

namespace Pathfinding.Base
{
    public class AStarPathfindingMechanics
    {
        private readonly AStarPathfindingSettingsConfig _pathfindingSettingsConfig;
        private readonly ScanSettings _scanSettings;
        public List<AStarPathNode> LastPath { get; private set; }
        public AStarNode[,] Grid { get; private set; } = new AStarNode[0, 0];
        public List<AStarPathNode> ClosedSet { get; private set; }
        public List<AStarPathNode> OpenSet { get; private set; }
        public Bounds ClampedScanBounds => _clampedScanBounds;
        public int ClosetSetCount => ClosedSet?.Count ?? 0;
        public int OpenSetCount => OpenSet?.Count ?? 0;

        private float _predictedAgentHp;
        private Bounds _clampedScanBounds;
        private float _averageWeight;
        private float _averageDamage;
        
        public AStarPathfindingMechanics(AStarPathfindingSettingsConfig pathfindingSettingsConfig,
            ScanSettings scanSettings)
        {
            _pathfindingSettingsConfig = pathfindingSettingsConfig;
            _scanSettings = scanSettings;
        }
        
        [TestOnly]
        public void SetGrid(AStarNode[,] grid, float nodeSize)
        {
            Grid = grid;
            _scanSettings.NodeSize = nodeSize;
            if (grid.GetLength(0) == 0 || grid.GetLength(1) == 0)
            {
                Debug.LogError("[AStarPathfinding] Zero dimension grid");
                return;
            }

            var length0 = grid.GetLength(0);
            var length1 = grid.GetLength(1);
            var center = grid[length0 / 2, length1 / 2].Center;
            var min = grid[0, 0];
            var max = grid[length0 - 1, length1 - 1];

            var xMinDelta = center.x - min.Center.x;
            var xMaxDelta = max.Center.x - center.x;
            var xExtend = Mathf.Max(xMaxDelta, xMinDelta) + _scanSettings.NodeSize / 2;

            var yMinDelta = center.y - min.Center.y;
            var yMaxDelta = max.Center.y - center.y;
            var yExtend = Mathf.Max(yMaxDelta, yMinDelta) + _scanSettings.NodeSize / 2;
           
            _clampedScanBounds = new Bounds(center, new Vector3(xExtend * 2, yExtend * 2));
        }
        
     /// <summary>
        /// Returns nearest node for game world position 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="maxCheckCount"></param>
        /// <returns></returns>
        public AStarNode GetNearestNode(Vector2 position, int maxCheckCount = -1)
        {
            if (!_clampedScanBounds.Contains(position))
                return null;

            var xDeltaIndex = (int)((position.x - _clampedScanBounds.min.x) / _scanSettings.NodeSize);
            var yDeltaIndex = (int)((position.y - _clampedScanBounds.min.y) / _scanSettings.NodeSize);

            return GetNodeByIndex(xDeltaIndex, yDeltaIndex);
        }

        /// <summary>
        /// Scan game field for calculating the grid
        /// </summary>
        public void Scan(Bounds clampedScanBounds)
        {
            _clampedScanBounds = clampedScanBounds;
            Grid = new AStarNode[(int)(_clampedScanBounds.extents.x / _scanSettings.NodeSize) * 2, (int)(_clampedScanBounds.extents.y / _scanSettings.NodeSize) * 2];
            var weightablesCount = 0;
            var sumDamage = 0f;

            int i = 0;
            int j = 0;
            for (var x = _clampedScanBounds.min.x; x < _clampedScanBounds.max.x - _scanSettings.NodeSize / 2; x += _scanSettings.NodeSize)
            {
                for (var y = _clampedScanBounds.min.y; y < _clampedScanBounds.max.y - _scanSettings.NodeSize / 2; y += _scanSettings.NodeSize)
                {
                    var center = new Vector2(x + _scanSettings.NodeSize / 2, y + _scanSettings.NodeSize / 2);
                    var node = CreateNode(center, new Vector2Int(i, j));
                    if (node.Weightable)
                    {
                        weightablesCount++;
                        sumDamage += node.WeightValue;
                    }
                    Grid[i, j] = node;
                    j++;
                }
                i++;
                j = 0;
            }

            _averageDamage = weightablesCount > 0 ? sumDamage / weightablesCount : 0;
        }
        
        /// <summary>
        /// Returns minimum path to goal position, apply all agent modifiers
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="goalPosition"></param>
        /// <param name="agent"></param>
        /// <returns></returns>
        [CanBeNull]
        public List<AStarNode> GetMinimumPath(Vector2 startPosition, Vector2 goalPosition, IAStarAgent agent)
        {
            var start = GetNearestNode(startPosition);
            var goal = GetNearestNode(goalPosition);

            if (start == null)
            {
                Debug.LogWarning("Can't find StartNode");
                return null;
            }

            if (goal == null)
            {
                Debug.LogWarning("Can't find EndNode");
                return null;
            }

            LastPath = FindPath(start, goal, agent.AgentWeightProvider);
            if (LastPath == null)
            {
                Debug.LogWarning("NULL pathNodes");
                return null;
            }
            Debug.Log("PATH DAMAGE: " + GetPathDamage(LastPath) + "\n" + "PATH LENGTH: " + LastPath.Count);
        
            agent.Modifiers.ForEach(m => m.ApplyModifier(LastPath));
            
            List<AStarNode> result = new List<AStarNode>(LastPath.Count);
            LastPath.ForEach(pn => result.Add(GetNodeByIndex(pn.Position.x, pn.Position.y)));
            return result;
        }

        private List<AStarPathNode> FindPath(AStarNode start, AStarNode goal, AStarAgentWeightProviderBase aStarAgentWeightProviderBase)
        {
            Debug.Log("Finding path from " + start.GridPosition + " to " + goal.GridPosition);
        
            ClosedSet = new List<AStarPathNode>();
            OpenSet = new List<AStarPathNode>();
       
            AStarPathNode startNode = new AStarPathNode()
            {
                Position = start.GridPosition,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLength(start.GridPosition, goal.GridPosition),
                WeightValueFromStart = start.WeightValue,
                HeruisticEstimateWeightValue = GetHeuristicWeight(start.GridPosition, goal.GridPosition),
                WeightRatio = _pathfindingSettingsConfig.WeightDetectionMode == WeightDetectionMode.Average ? _pathfindingSettingsConfig.DefaultWeightInfluenceRatio : 0
            };
            OpenSet.Add(startNode);
        
            while (OpenSet.Count > 0)
            {
                var currentNode = OpenSet.OrderBy(node =>
                    node.F).First();
                
                if (currentNode.Position == goal.GridPosition)
                    //return openSet;
                    return GetPathForNode(currentNode);
           
                OpenSet.Remove(currentNode);
                ClosedSet.Add(currentNode);
              
                foreach (var neighbourNode in GetNeighbours(currentNode, goal.GridPosition, aStarAgentWeightProviderBase))
                {
                    if (ClosedSet.Exists(node => node.Position == neighbourNode.Position))
                        continue;

                    var openNode = OpenSet.FirstOrDefault(node =>
                        node.Position == neighbourNode.Position);
                  
                    if (openNode == null)
                    {
                        OpenSet.Add(neighbourNode);   
                    }  
                    else
                    if (openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart)
                    {
                        openNode.CameFrom = currentNode;
                        openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                    }
                }
            }
     
            return null;
        }

        private AStarNode CreateNode(Vector2 center, Vector2Int gridPosition)
        {
            var node = new AStarNode {Center = center, GridPosition = gridPosition};
            switch (_scanSettings.CheckingMode)
            {
                case CheckingMode.Circle:
                    var wallCollider = Physics2D.OverlapCircle(center, _scanSettings.NodeSize / 2.2f, _scanSettings.ObstaclesLayerMask);
                    node.Walkable = wallCollider == null;
                    var weightCollider = Physics2D.OverlapCircle(center, _scanSettings.NodeSize / 2.2f, _scanSettings.WeightEnvironmentLayerMask);
                    if (weightCollider != null)
                    {
                        var weightNode = weightCollider.GetComponent<IAStarWeightNode>();
                        if (weightNode != null)
                            node.WeightValue = weightNode.Weight;
                    }
                    break;
                case CheckingMode.Point:
                    var pWallCollider = Physics2D.OverlapCircle(center, _scanSettings.NodeSize / 10f, _scanSettings.ObstaclesLayerMask);
                    node.Walkable = pWallCollider == null;
                    var pointWeightCollider = Physics2D.OverlapCircle(center, _scanSettings.NodeSize / 10f, _scanSettings.WeightEnvironmentLayerMask);
                    if (pointWeightCollider != null)
                    {
                        var weightNode = pointWeightCollider.GetComponent<IAStarWeightNode>();
                        if (weightNode != null)
                            node.WeightValue = weightNode.Weight;
                    }
                    break;
            }
            return node;
        }

        
        private float GetAverageDamage(Vector2Int from, Vector2Int to)
        {
            if (Grid == null)
                return 0;

            var xLength = Grid.GetLength(0);
            var yLength = Grid.GetLength(1);

            if (from.x >= xLength || from.y >= yLength || to.x >= xLength || to.y >= yLength)
                return 0;

            var count = 0;
            var sum = 0f;
            for (int i = 0; i < xLength; i++)
            {
                for (int j = 0; j < yLength; j++)
                {
                    var node = Grid[i, j];
                    sum += node.WeightValue;
                    count++;
                }
            }

            return sum / count;
        }

        private float GetHeuristicWeight(Vector2Int from, Vector2Int to) => 
            _pathfindingSettingsConfig.WeightDetectionMode == WeightDetectionMode.Average ? GetAverageDamage(from, to) : 0;
    
        private int GetDistanceBetweenNeighbours() => 1;

        private int GetHeuristicPathLength(Vector2Int from, Vector2Int to) => 
            _pathfindingSettingsConfig.HeuristicMultiplier * (Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y));
        
        private List<AStarPathNode> GetNeighbours(AStarPathNode pathNode, Vector2Int goal, AStarAgentWeightProviderBase aStarAgentWeightProviderBase)
        {
            var result = new List<AStarPathNode>();

            // Соседними точками являются соседние по стороне клетки.
            Vector2Int[] neighbourPoints = new Vector2Int[4];
            neighbourPoints[0] = new Vector2Int(pathNode.Position.x + 1, pathNode.Position.y);
            neighbourPoints[1] = new Vector2Int(pathNode.Position.x - 1, pathNode.Position.y);
            neighbourPoints[2] = new Vector2Int(pathNode.Position.x, pathNode.Position.y + 1);
            neighbourPoints[3] = new Vector2Int(pathNode.Position.x, pathNode.Position.y - 1);

            foreach (var point in neighbourPoints)
            {
                // Проверяем, что не вышли за границы карты.
                if (point.x < 0 || point.x >= Grid.GetLength(0))
                    continue;
                if (point.y < 0 || point.y >= Grid.GetLength(1))
                    continue;
                // Проверяем, что по клетке можно ходить.
                if (Grid[point.x, point.y] != null && !Grid[point.x, point.y].Walkable)
                    continue;
                var weightFromStart = pathNode.WeightValueFromStart + Grid[point.x, point.y].WeightValue;
                if (_pathfindingSettingsConfig.WeightDetectionMode == WeightDetectionMode.OnlyCriticalWeightCheck)
                {
                    var node = Grid[point.x, point.y];
                    if (node.WeightValue >= aStarAgentWeightProviderBase.AgentWeight)
                        continue;
                }
                else
                if (_pathfindingSettingsConfig.WeightDetectionMode == WeightDetectionMode.PredictedLethalCheck)
                {
                    if (weightFromStart >= aStarAgentWeightProviderBase.AgentWeight)
                        continue;
                }
                // Заполняем данные для точки маршрута.
                var neighbourNode = new AStarPathNode()
                {
                    Position = point,
                    CameFrom = pathNode,
                    PathLengthFromStart = pathNode.PathLengthFromStart + GetDistanceBetweenNeighbours(),
                    HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal),
                    WeightValueFromStart = pathNode.WeightValueFromStart + Grid[point.x, point.y].WeightValue,
                    HeruisticEstimateWeightValue = GetHeuristicWeight(point, goal),
                    WeightRatio = _pathfindingSettingsConfig.WeightDetectionMode == WeightDetectionMode.Average 
                        ? _pathfindingSettingsConfig.DefaultWeightInfluenceRatio 
                        : 0
                };

                result.Add(neighbourNode);
            }
            return result;
        }

        private List<AStarPathNode> GetPathForNode(AStarPathNode pathNode)
        {
            var result = new List<AStarPathNode>();
            var currentNode = pathNode;
            while (currentNode != null)
            {
        	
                result.Add(currentNode);
                currentNode = currentNode.CameFrom;
            }
            result.Reverse();
            return result;
        }
        
        private float GetPathDamage(List<AStarPathNode> path)
        {
            return path.Last().WeightValueFromStart;
        }
        
        private AStarNode GetNodeByIndex(int xIndex, int yIndex)
        {
            if (Grid == null)
            {
                Debug.LogWarning("NULL grid");
                return null;
            }
            if (xIndex >= Grid.GetLength(0) || yIndex >= Grid.GetLength(1))
            {
                Debug.LogWarning("wrong grid index");
                return null;
            }

            return Grid[xIndex, yIndex];
        }
    }
}
