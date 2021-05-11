using System.Collections.Generic;
using Pathfinding.Interfaces;

namespace Pathfinding.MonoBehaviours.Agent
{
    public interface IAStarAgent
    {
        AStarAgentWeightProviderBase AgentWeightProvider { get; }
        List<IModifier> Modifiers { get; }
    }
}