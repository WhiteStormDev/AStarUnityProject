using System;
using Pathfinding.Enums;
using UnityEngine;

namespace Pathfinding.MonoBehaviours
{
    [Serializable]
    public class ScanSettings
    {
        [Header("Scan Settings")] 
        [Tooltip("Node width & height")]
        public float NodeSize = 1;
        public Bounds ScanBounds;

        [Header("Layers")] 
        public CheckingMode CheckingMode = CheckingMode.Circle;
        public LayerMask ObstaclesLayerMask;
        public LayerMask WeightEnvironmentLayerMask;
    }
}