using Pathfinding.Enums;
using UnityEngine;

namespace Pathfinding.Base
{
	public class AStarPathNode
	{
		public WeightDetectionMode WeightDetectionMode;

		public Vector2Int Position { get; set; }

		public int PathLengthFromStart { get; set; }
	
		public AStarPathNode CameFrom { get; set; }
	
		public int HeuristicEstimatePathLength { get; set; }

		public int EstimateFullPathLength => PathLengthFromStart + HeuristicEstimatePathLength;

		public float WeightValueFromStart { get; set; }
		public float HeruisticEstimateWeightValue { get; set; }
		public float EstimateFullWeightValue => WeightValueFromStart + HeruisticEstimateWeightValue;

		private float _weightRatio;
		public float WeightRatio { get => _weightRatio; set => _weightRatio = Mathf.Clamp(value, 0, 1); }
		public float F => EstimateFullPathLength + WeightRatio * EstimateFullWeightValue;
	}
}
