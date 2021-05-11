using Unity.Collections;
using UnityEngine;

namespace Pathfinding.MonoBehaviours.Agent
{
    public class AStarAgentWeightProviderBase : MonoBehaviour
    {
        [SerializeField] protected bool _isActiveNodeWeightInfluenceOverride;
        [SerializeField] protected float _nodeWeightInfluenceOverride;
        [SerializeField] protected float _currentAgentWeight;

        public float AgentWeight => _currentAgentWeight;
        public float AgentNodeWeighInfluenceOverride => _nodeWeightInfluenceOverride;
        public bool IsActiveAgentNodeWeightInfluenceOverride => _isActiveNodeWeightInfluenceOverride;
    }
}