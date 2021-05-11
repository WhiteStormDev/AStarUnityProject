using System;

namespace Pathfinding.Configs
{
    public abstract class ConfigManagerBase : IConfigManager
    {
        public T Create<T>(string name) where T : ConfigBase
        {
            return (T)Create(typeof(T));
        }
        protected abstract ConfigBase Create(Type type);
    }
}