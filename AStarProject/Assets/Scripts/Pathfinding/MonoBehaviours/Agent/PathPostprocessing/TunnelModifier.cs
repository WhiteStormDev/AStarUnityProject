using System.Collections.Generic;
using Pathfinding.Base;
using Pathfinding.Interfaces;
using UnityEngine;

namespace Pathfinding.MonoBehaviours.Agent.PathPostprocessing
{
    public class TunnelModifier : MonoBehaviour, IModifier
    {
        public List<AStarPathNode> ApplyModifier(List<AStarPathNode> path)
        {
            return path;
        }
    }
}