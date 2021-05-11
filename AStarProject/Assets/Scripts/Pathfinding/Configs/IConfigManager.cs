namespace Pathfinding.Configs
{
    public interface IConfigManager
    {
        T Create<T>(string name) where T : ConfigBase;
    }
}