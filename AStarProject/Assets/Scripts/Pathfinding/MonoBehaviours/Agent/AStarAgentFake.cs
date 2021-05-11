using System.Collections.Generic;
using Pathfinding.Interfaces;

namespace Pathfinding.MonoBehaviours.Agent
{
    public class AStarAgentFake : IAStarAgent
    {
        public AStarAgentWeightProviderBase AgentWeightProvider { get; } = new AStarAgentWeightProviderBase();
        public List<IModifier> Modifiers { get; } = new List<IModifier>();
    }
}