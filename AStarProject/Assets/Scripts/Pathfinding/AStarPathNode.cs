using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class AStarPathNode
{
    public DamageDetectionMode DamageDetectionMode;
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

	public float DamageValueFromStart { get; set; }
	public float HeruisticEstimateDamageValue { get; set; }
	public float EstimateFullDamageValue => DamageValueFromStart + HeruisticEstimateDamageValue;

    private float _damageRatio;
    public float DamageRatio { get => _damageRatio; set => _damageRatio = Mathf.Clamp(value, 0, 1); }
    public float F => EstimateFullPathLength + DamageRatio * EstimateFullDamageValue;

	//public AStarNode GridNode { get; set; }
}
