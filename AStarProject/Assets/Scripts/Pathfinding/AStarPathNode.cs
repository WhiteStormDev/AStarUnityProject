using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class AStarPathNode
{
    public DamageDetectionMode DamageDetectionMode;

	public Point Position { get; set; }

	public int PathLengthFromStart { get; set; }
	
	public AStarPathNode CameFrom { get; set; }
	
	public int HeuristicEstimatePathLength { get; set; }

	public int EstimateFullPathLength => PathLengthFromStart + HeuristicEstimatePathLength;

	public float DamageValueFromStart { get; set; }
	public float HeruisticEstimateDamageValue { get; set; }
	public float EstimateFullDamageValue => DamageValueFromStart + HeruisticEstimateDamageValue;

    private float _damageRatio;
    public float DamageRatio { get => _damageRatio; set => _damageRatio = Mathf.Clamp(value, 0, 1); }
    public float F => EstimateFullPathLength + DamageRatio * EstimateFullDamageValue;

	//public AStarNode GridNode { get; set; }
}
