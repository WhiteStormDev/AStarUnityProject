using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinding.Base;
using Pathfinding.Enums;
using Pathfinding.Interfaces;
using UnityEngine;

namespace Pathfinding.MonoBehaviours
{
    public class AStarPathfinding : MonoBehaviour
    {
        [Header("Find Stategy")]
        public DamageDetectionMode DamageDetectionMode;
        [Range (0f, 1f)]
        public float DamageInfluenceRatio;
        public float AverageDamage;
        [Range (0, 10)]
        public int HeuristicMultiplier = 1;
        [Header("Scan Settings")]
        [Tooltip("Node width & height")]
        public float NodeSize;
        public Bounds ScanBounds;

        [Header("Layers")]
        public CheckingMode CheckingMode;
        public LayerMask ObstaclesLayerMask;
        public LayerMask DamagingEnvironmentLayerMask;

        public static AStarPathfinding Instance { get; protected set; }
        public List<AStarPathNode> LastPath { get; private set; }
        public AStarNode[,] Grid { get; protected set; }
        public Bounds ClampedScanBounds => _clampedScanBounds;
        
        //public AStarNode GetAStarNode(Point gridPoint) => _grid.GetLength(0) <= gridPoint.X || _grid.GetLength(1) <= gridPoint.Y ? null : _grid[gridPoint.X, gridPoint.Y];
        public List<AStarPathNode> ClosedSet { get; protected set; }
        public List<AStarPathNode> OpenSet { get; protected set; }

        public int ClosetSetCount => ClosedSet == null ? 0 : ClosedSet.Count;
        public int OpenSetCount => OpenSet == null ? 0 : OpenSet.Count;

        private float _predictedAgentHP;
        private Bounds _clampedScanBounds;
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetClampedScanBounds(ScanBounds);
            Scan();
        }

        private void SetClampedScanBounds(Bounds bounds)
        {
            if (NodeSize > 0)
            {
                var x = bounds.extents.x - bounds.center.x;
                var y = bounds.extents.y - bounds.center.y;
                _clampedScanBounds.center = bounds.center + transform.position;

                var clampX = x - x % NodeSize;
                var clampY = y - y % NodeSize;
                _clampedScanBounds.extents = new Vector3(clampX, clampY);

            }
            else
            {
                Debug.LogError("NodeSize must be positive and not 0");
            }
        }

        public void Scan()
        {
            SetClampedScanBounds(ScanBounds);
            Grid = new AStarNode[(int)(_clampedScanBounds.extents.x / NodeSize) * 2, (int)(_clampedScanBounds.extents.y / NodeSize) * 2];
            var damagersCount = 0;
            var sumDamage = 0f;

            int i = 0;
            int j = 0;
            for (float x = _clampedScanBounds.min.x; x < _clampedScanBounds.max.x - NodeSize / 2; x += NodeSize)
            {
                for (float y = _clampedScanBounds.min.y; y < _clampedScanBounds.max.y - NodeSize / 2; y += NodeSize)
                {
                    var center = new Vector2(x + NodeSize / 2, y + NodeSize / 2);
                    var node = CreateNode(center, new Vector2Int(i, j));
                    if (node.Damaging)
                    {
                        damagersCount++;
                        sumDamage += node.DamageValue;
                    }
                    Grid[i, j] = node;
                    j++;
                }
                i++;
                j = 0;
            }

            AverageDamage = damagersCount > 0 ? sumDamage / damagersCount : 0;
        }

        private AStarNode CreateNode(Vector2 center, Vector2Int gridPosition)
        {
            AStarNode node = new AStarNode();
            node.Center = center;
            node.GridPosition = gridPosition;
            switch (CheckingMode)
            {
                case CheckingMode.Circle:
                    var wallCollider = Physics2D.OverlapCircle(center, NodeSize / 2.2f, ObstaclesLayerMask);
                    node.Walkable = wallCollider == null;
                    var damagerCollider = Physics2D.OverlapCircle(center, NodeSize / 2.2f, DamagingEnvironmentLayerMask);
                    if (damagerCollider != null)
                    {
                        var damager = damagerCollider.GetComponent<IAStarDamager>();
                        if (damager != null)
                            node.DamageValue = damager.Damage;
                    }
                    break;
                case CheckingMode.Point:
                    var pWallCollider = Physics2D.OverlapCircle(center, NodeSize / 10f, ObstaclesLayerMask);
                    node.Walkable = pWallCollider == null;
                    var pDamagerCollider = Physics2D.OverlapCircle(center, NodeSize / 10f, DamagingEnvironmentLayerMask);
                    if (pDamagerCollider != null)
                    {
                        var damager = pDamagerCollider.GetComponent<IAStarDamager>();
                        if (damager != null)
                            node.DamageValue = damager.Damage;
                    }
                    break;
            }
            return node;
        }

        public List<AStarNode> GetMinimumPath(Vector2 startPosition, Vector2 goalPosition, AStarAgent agent)
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

            LastPath = FindPath(start, goal, agent);
            if (LastPath == null)
            {
                Debug.LogWarning("NULL pathNodes");
                return null;
            }
            Debug.Log("PATH DAMAGE: " + GetPathDamage(LastPath) + "\n" + "PATH LENGTH: " + LastPath.Count);
        
            List<AStarNode> result = new List<AStarNode>(LastPath.Count);
            LastPath.ForEach(pn => result.Add(GetNodeByIndex(pn.Position.x, pn.Position.y)));
            return result;
        }

        private List<AStarPathNode> FindPath(AStarNode start, AStarNode goal, AStarAgent agent)
        {
            Debug.Log("Finding path from " + start.GridPosition + " to " + goal.GridPosition);
            // Шаг 1.
            ClosedSet = new List<AStarPathNode>();
            OpenSet = new List<AStarPathNode>();
            // Шаг 2.
            AStarPathNode startNode = new AStarPathNode()
            {
                Position = start.GridPosition,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLength(start.GridPosition, goal.GridPosition),
                DamageValueFromStart = start.DamageValue,
                HeruisticEstimateDamageValue = GetHeuristicDamage(start.GridPosition, goal.GridPosition),
                DamageRatio = DamageDetectionMode == DamageDetectionMode.Average ? DamageInfluenceRatio : 0
            };
            OpenSet.Add(startNode);
        
            while (OpenSet.Count > 0)
            {
                // Шаг 3.
                var currentNode = OpenSet.OrderBy(node =>
                    node.F).First();
                // Шаг 4.
                if (currentNode.Position == goal.GridPosition)
                    //return openSet;
                    return GetPathForNode(currentNode);
                // Шаг 5.
                OpenSet.Remove(currentNode);
                ClosedSet.Add(currentNode);
                // Шаг 6.
                foreach (var neighbourNode in GetNeighbours(currentNode, goal.GridPosition, agent))
                {
                    // Шаг 7.
                    if (ClosedSet.Exists(node => node.Position == neighbourNode.Position))
                        continue;

                    var openNode = OpenSet.FirstOrDefault(node =>
                        node.Position == neighbourNode.Position);
                    // Шаг 8.
                    if (openNode == null)
                    {
                        OpenSet.Add(neighbourNode);   
                    }  
                    else
                    if (openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart)
                    {
                        // Шаг 9.
                        openNode.CameFrom = currentNode;
                        openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                    }
                }
            }
            // Шаг 10.
            return null;
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
                    sum += node.DamageValue;
                    count++;
                }
            }

            return sum / count;
        }

        private float GetHeuristicDamage(Vector2Int from, Vector2Int to) => DamageDetectionMode == DamageDetectionMode.Average ? GetAverageDamage(from, to) : 0;
    
        private int GetDistanceBetweenNeighbours() => 1;

        private int GetHeuristicPathLength(Vector2Int from, Vector2Int to) => HeuristicMultiplier * (Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y));

        private List<AStarPathNode> GetNeighbours(AStarPathNode pathNode, Vector2Int goal, AStarAgent agent)
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
                var damageFromStart = pathNode.DamageValueFromStart + Grid[point.x, point.y].DamageValue;
                if (DamageDetectionMode == DamageDetectionMode.LethalCheck)
                {
                    var node = Grid[point.x, point.y];
                    if (node.DamageValue >= agent.CurrentHP)
                        continue;
                }
                else
                if (DamageDetectionMode == DamageDetectionMode.PredictedLethalCheck)
                {
                    if (damageFromStart >= agent.CurrentHP)
                        continue;
                }
                // Заполняем данные для точки маршрута.
                var neighbourNode = new AStarPathNode()
                {
                    Position = point,
                    CameFrom = pathNode,
                    PathLengthFromStart = pathNode.PathLengthFromStart +
                                          GetDistanceBetweenNeighbours(),
                    HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal),
                    DamageValueFromStart = pathNode.DamageValueFromStart + Grid[point.x, point.y].DamageValue,
                    HeruisticEstimateDamageValue = GetHeuristicDamage(point, goal),
                    DamageRatio = DamageDetectionMode == DamageDetectionMode.Average ? DamageInfluenceRatio : 0
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
            return path.Last().DamageValueFromStart;
        }

        public AStarNode GetNearestNode(Vector2 position, Predicate<AStarNode> predicate = null, int maxCheckCount = -1)
        {
            if (!_clampedScanBounds.Contains(position))
                return null;

            var xDeltaIndex = (int)((position.x - _clampedScanBounds.min.x) / NodeSize);
            var yDeltaIndex = (int)((position.y - _clampedScanBounds.min.y) / NodeSize);

            if (predicate == null)
            {
                return GetNodeByIndex(xDeltaIndex, yDeltaIndex);
            }

            return FindByPredicate(xDeltaIndex, yDeltaIndex, maxCheckCount, predicate);
        }

        private AStarNode FindByPredicate(int startXIndex, int startYIndex, int maxCyclesCount, Predicate<AStarNode> predicate)
        {
            //поиск с рекурсивным расширением
            return GetNodeByIndex(startXIndex, startYIndex);
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
        
#if UNITY_EDITOR
        [ContextMenu("Scan")]
        private void Test()
        {
            Scan();
        }

        private void DrawGrid()
        {
            if (Grid == null)
                return;

            var damagers = new List<AStarNode>();
            var walls = new List<AStarNode>();
            var walkables = new List<AStarNode>();

            for (int i = 0; i < Grid.GetLength(0); i++)
            {
                for (int j = 0; j < Grid.GetLength(1); j++)
                {
                    var node = Grid[i, j];
                    if (node == null)
                        break;

                    if (node.Damaging)
                        damagers.Add(node);
                    if (node.Walkable)
                        walkables.Add(node);
                    else
                        walls.Add(node);
                }
            }

            Gizmos.color = UnityEngine.Color.gray;
            walkables.ForEach(w => Gizmos.DrawWireCube(w.Center, new Vector3(NodeSize, NodeSize, 0)));

            Gizmos.color = new UnityEngine.Color(0.9f, 0.9f, 0.9f, 0.3f);
            walls.ForEach(w => Gizmos.DrawCube(w.Center, new Vector3(NodeSize, NodeSize, 0)));

            Gizmos.color = new UnityEngine.Color(UnityEngine.Color.red.r, UnityEngine.Color.red.g, UnityEngine.Color.red.b, 0.3f);
            damagers.ForEach(d => Gizmos.DrawCube(d.Center, new Vector3(NodeSize, NodeSize, 0)));
        }

        private void OnValidate()
        {
            if (NodeSize > 0)
            {
                var x = ScanBounds.extents.x - ScanBounds.center.x;
                var y = ScanBounds.extents.y - ScanBounds.center.y;
                _clampedScanBounds.center = ScanBounds.center + transform.position;

                var clampX = x - x % NodeSize;
                var clampY = y - y % NodeSize;
                _clampedScanBounds.extents = new Vector3(clampX, clampY);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = UnityEngine.Color.blue;
            Gizmos.DrawLine(_clampedScanBounds.min, new Vector3(_clampedScanBounds.min.x, _clampedScanBounds.max.y));
            Gizmos.DrawLine(_clampedScanBounds.min, new Vector3(_clampedScanBounds.max.x, _clampedScanBounds.min.y));
            Gizmos.DrawLine(_clampedScanBounds.max, new Vector3(_clampedScanBounds.min.x, _clampedScanBounds.max.y));
            Gizmos.DrawLine(_clampedScanBounds.max, new Vector3(_clampedScanBounds.max.x, _clampedScanBounds.min.y));

            DrawGrid();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new UnityEngine.Color(UnityEngine.Color.magenta.r, UnityEngine.Color.magenta.g, UnityEngine.Color.magenta.b, 0.8f);

            ClosedSet?.ForEach(cs =>
            {
                if (cs.Position.x < Grid.GetLength(0) && cs.Position.y < Grid.GetLength(1))
                {
                    var node = Grid[cs.Position.x, cs.Position.y];
                    Gizmos.DrawSphere(node.Center, NodeSize / 4);
                } 
            });
            Gizmos.color = new UnityEngine.Color(UnityEngine.Color.green.r, UnityEngine.Color.green.g, UnityEngine.Color.green.b, 0.8f);
            OpenSet?.ForEach(os =>
            {
                if (os.Position.x < Grid.GetLength(0) && os.Position.y < Grid.GetLength(1))
                {
                    var node = Grid[os.Position.x, os.Position.y];
                    Gizmos.DrawSphere(node.Center, NodeSize / 4);
                }
            });
        }
#endif
    }
}