using Pathfinding.Components;

namespace Pathfinding.Configs
{
    public class ResourceSystem : Singleton<ResourceSystem>
    {
        private readonly IConfigManager _configManager = new EditorConfigManager();
        
        public static T Create<T>(string name) where T : ConfigBase
        {
            var configs = Instance._configManager;
            return configs?.Create<T>(name);
        }
    }
}