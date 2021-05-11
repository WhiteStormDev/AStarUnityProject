using Game.Units.Controllers;
using Pathfinding.MonoBehaviours.Agent;
using UnityEngine;

namespace Pathfinding.DemoGame.Game.Units.Controllers
{
    public class AStarAgentHpWeightProvider : AStarAgentWeightProviderBase
    {
        [SerializeField] private UnitController _unit;
        
        private void OnEnable()
        {
            _unit.HpChangeEvent += OnHpChanged;
        }

        private void Start()
        {
            OnHpChanged(_unit.CurrentHp);
        }

        private void OnHpChanged(float hp)
        {
            _currentAgentWeight = hp;
        }

        private void OnDisable()
        {
            _unit.HpChangeEvent -= OnHpChanged;
        }
    }
}