using System;
using Pathfinding.Configs;

namespace Pathfinding.Tests
{
    public static class ConfigsTestHelper
    {
        public static T CreateConfig<T>() where T : ConfigBase
        {
            return ResourceSystem.Create<T>("/Test/" + typeof(T).Name + "/" + Guid.NewGuid().ToString("D"));
        }
    }
}