using System;
using Pathfinding.MonoBehaviours.Agent;
using UnityEngine;

namespace Game.Units.Controllers
{
	public abstract class UnitController : MonoBehaviour
	{
		[SerializeField] private float _maxHP;
		[SerializeField] private float _currentHP;
		[SerializeField] private AStarAgentWeightProviderBase _agentWeightProvider;
		public event Action<UnitController> DeathEvent;

		public bool IsDead => _currentHP <= 0;
		public float CurrentHp => _currentHP;
		
		public event Action<float> HpChangeEvent;
		
		private void OnEnable()
		{
			_currentHP = _maxHP;
		}

		public void InstantDamage(float damageValue)
		{
			SetHealth(_currentHP - damageValue);
			OnDamage();
		}

		private void SetHealth(float healthValue)
		{
			_currentHP = healthValue;
			
			if (healthValue <= 0)
				DeathEvent?.Invoke(this);
		}

		private void OnDamage()
		{
			HpChangeEvent?.Invoke(_currentHP);
		}
		public void Revive() => SetHealth(_maxHP);
	}
}
