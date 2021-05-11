using Pathfinding.Enums;
using UnityEngine;

namespace Pathfinding.Configs
{
    [CreateAssetMenu(menuName = "AStarPathfinding/SettingsConfig", fileName = "AStarPathfindingSettings")]
    public class AStarPathfindingSettingsConfig : ConfigBase
    {
        [Header("Find Strategy")] 
        [Tooltip("Mode of damage detection for agents. They check ")]
        public WeightDetectionMode WeightDetectionMode = WeightDetectionMode.Average;

        [Range(0f, 1f)] 
        public float DefaultWeightInfluenceRatio = 0.5f;

        [Range (0, 10)]
        public int HeuristicMultiplier = 1;
    }
}