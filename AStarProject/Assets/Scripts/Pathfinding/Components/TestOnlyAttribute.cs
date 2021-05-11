using System;

namespace Pathfinding.Components
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor)]
    public class TestOnlyAttribute : Attribute
    {
    }
}