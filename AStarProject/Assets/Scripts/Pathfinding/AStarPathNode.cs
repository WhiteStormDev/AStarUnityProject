using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class AStarPathNode
{
	// Координаты точки на карте.
	public Point Position { get; set; }
	// Длина пути от старта (G).
	public int PathLengthFromStart { get; set; }
	// Точка, из которой пришли в эту точку.
	public AStarPathNode CameFrom { get; set; }
	// Примерное расстояние до цели (H).
	public int HeuristicEstimatePathLength { get; set; }
	// Ожидаемое полное расстояние до цели (F).
	public int EstimateFullPathLength => PathLengthFromStart + HeuristicEstimatePathLength;

	public AStarNode GridNode { get; set; }
}
