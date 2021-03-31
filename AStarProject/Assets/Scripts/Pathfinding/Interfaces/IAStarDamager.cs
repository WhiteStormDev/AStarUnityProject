namespace Pathfinding.Interfaces
{
    /// <summary>
    /// The Damager interface.
    /// If you want to use your own Game Damagers in pathfinding algorithm they must be nested from IAStarDamager
    /// and have DamagerLayer that you enter in AStarPathfinding gameObject settings.
    /// </summary>
    public interface IAStarDamager
    {
        float Damage { get; }
    }
}
