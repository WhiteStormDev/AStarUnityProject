using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Drawing;

public enum CheckingMode
{
    Circle,
    Point
}
public class AStarPathfinding : MonoBehaviour
{
    [Header("Scan Settings")]
    [Tooltip("Node width & height")]
    public float NodeSize;
    public Bounds ScanBounds;

    [Header("Layers")]
    public CheckingMode CheckingMode;
    public LayerMask ObstaclesLayerMask;
    public LayerMask DamagingEnvironmentLayerMask;

    private AStarNode[,] _grid;
    private Bounds _clampedScanBounds;

    public static AStarPathfinding Instance { get; protected set; }

	//public AStarNode GetAStarNode(Point gridPoint) => _grid.GetLength(0) <= gridPoint.X || _grid.GetLength(1) <= gridPoint.Y ? null : _grid[gridPoint.X, gridPoint.Y];
	
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
        _grid = new AStarNode[(int)(_clampedScanBounds.extents.x / NodeSize) * 2, (int)(_clampedScanBounds.extents.y / NodeSize) * 2];

        int i = 0;
        int j = 0;
        for (float x = _clampedScanBounds.min.x; x < _clampedScanBounds.max.x - NodeSize / 2; x += NodeSize)
        {
            for (float y = _clampedScanBounds.min.y; y < _clampedScanBounds.max.y - NodeSize / 2; y += NodeSize)
            {
                var center = new Vector2(x + NodeSize / 2, y + NodeSize / 2);
                _grid[i, j] = CreateNode(center, new Point(i, j));
                j++;
            }
            i++;
            j = 0;
        }
    }

    private AStarNode CreateNode(Vector2 center, Point gridPosition)
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
                    var damager = damagerCollider.GetComponent<IDamager>();
                    if (damager != null)
                        node.DamageValue = damager.Damage;
                }
                break;
            case CheckingMode.Point:
                break;
        }
        return node;
    }

    public List<AStarNode> GetMinimumPath(Vector2 position)
    {
		return null;
    }

	public List<AStarNode> GetMinimumPath(Vector2 startPosition, Vector2 goalPosition, int minimumDistance = -1)
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

		var pathNodes = FindPath(start, goal, minimumDistance);
		if (pathNodes == null)
		{
			Debug.LogWarning("NULL pathNodes");
			return null;
		}
		Debug.Log("PATH DAMAGE: " + GetPathDamage(pathNodes) + "\n" + "PATH LENGTH: " + pathNodes.Count);
        
		List<AStarNode> result = new List<AStarNode>(pathNodes.Count);
		pathNodes.ForEach(pn => result.Add(GetNodeByIndex(pn.Position.X, pn.Position.Y)));
		return result;
	}

    private List<AStarPathNode> FindPath(AStarNode start, AStarNode goal, int minimumDistance = -1)
    {
		Debug.Log("Finding path from " + start.GridPosition + " to " + goal.GridPosition);
        // Шаг 1.
        var closedSet = new List<AStarPathNode>();
        var openSet = new List<AStarPathNode>();
		// Шаг 2.
		AStarPathNode startNode = new AStarPathNode()
		{
			Position = start.GridPosition,
			CameFrom = null,
			PathLengthFromStart = 0,
			HeuristicEstimatePathLength = GetHeuristicPathLength(start.GridPosition, goal.GridPosition),
			DamageValueFromStart = start.DamageValue
			//HeruisticEstimateDamageValue = 
        };
        openSet.Add(startNode);
        while (openSet.Count > 0)
        {
            // Шаг 3.
            var currentNode = openSet.OrderBy(node =>
              node.EstimateFullPathLength).First();
			// Шаг 4.
			if (currentNode.Position == goal.GridPosition)
				//return openSet;
                return GetPathForNode(currentNode);
            // Шаг 5.
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            // Шаг 6.
            foreach (var neighbourNode in GetNeighbours(currentNode, goal.GridPosition, minimumDistance))
            {
                // Шаг 7.
                if (closedSet.Exists(node => node.Position == neighbourNode.Position))
                    continue;

                var openNode = openSet.FirstOrDefault(node =>
                  node.Position == neighbourNode.Position);
                // Шаг 8.
                if (openNode == null)
                {
                    openSet.Add(neighbourNode);   
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

	private int GetDistanceBetweenNeighbours()
	{
		return 1;
	}

	private int GetHeuristicPathLength(Point from, Point to) => Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);

	private List<AStarPathNode> GetNeighbours(AStarPathNode pathNode, Point goal, int minimumDistance = -1)
	{
		var result = new List<AStarPathNode>();

		// Соседними точками являются соседние по стороне клетки.
		Point[] neighbourPoints = new Point[4];
		neighbourPoints[0] = new Point(pathNode.Position.X + 1, pathNode.Position.Y);
		neighbourPoints[1] = new Point(pathNode.Position.X - 1, pathNode.Position.Y);
		neighbourPoints[2] = new Point(pathNode.Position.X, pathNode.Position.Y + 1);
		neighbourPoints[3] = new Point(pathNode.Position.X, pathNode.Position.Y - 1);

		foreach (var point in neighbourPoints)
		{
			// Проверяем, что не вышли за границы карты.
			if (point.X < 0 || point.X >= _grid.GetLength(0))
				continue;
			if (point.Y < 0 || point.Y >= _grid.GetLength(1))
				continue;
			// Проверяем, что по клетке можно ходить.
			if (_grid[point.X, point.Y] != null && !_grid[point.X, point.Y].Walkable)
				continue;
			// Заполняем данные для точки маршрута.
			var neighbourNode = new AStarPathNode()
			{
				Position = point,
				CameFrom = pathNode,
				PathLengthFromStart = pathNode.PathLengthFromStart +
				GetDistanceBetweenNeighbours(),
				HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal),
				DamageValueFromStart = pathNode.DamageValueFromStart + _grid[point.X, point.Y].DamageValue
			};

            if (neighbourNode.EstimateFullPathLength >= minimumDistance)
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
        if (_grid == null)
        {
            Debug.LogWarning("NULL grid");
            return null;
        }
        if (xIndex >= _grid.GetLength(0) || yIndex >= _grid.GetLength(1))
        {
            Debug.LogWarning("wrong grid index");
            return null;
        }

        return _grid[xIndex, yIndex];
    }
#if UNITY_EDITOR
    [ContextMenu("Scan")]
    private void Test()
    {
        Scan();
    }

    private void DrawGrid()
    {
        if (_grid == null)
            return;

        var damagers = new List<AStarNode>();
        var walls = new List<AStarNode>();
        var walkables = new List<AStarNode>();

        for (int i = 0; i < _grid.GetLength(0); i++)
        {
            for (int j = 0; j < _grid.GetLength(1); j++)
            {
                var node = _grid[i, j];
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
        //if (NodeSize <= 0)
        //    return;

        //Gizmos.color = Color.cyan;

        //for (float x = _clampedScanBounds.min.x + NodeSize;  x <= _clampedScanBounds.max.x; x += NodeSize)
        //{
        //    Gizmos.DrawLine(new Vector3(x, _clampedScanBounds.max.y), new Vector3(x, _clampedScanBounds.min.y));
        //}

        //for (float y = _clampedScanBounds.max.y + NodeSize; y <= _clampedScanBounds.min.y; y += NodeSize)
        //{
        //    Gizmos.DrawLine(new Vector3(_clampedScanBounds.min.x, y), new Vector3(_clampedScanBounds.max.x, y));
        //}
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(AStarPathfinding)), CanEditMultipleObjects]
public class AStarPathfindingEditor : Editor
{
	
}
#endif
