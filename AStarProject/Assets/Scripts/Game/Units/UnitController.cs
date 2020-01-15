using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitController : MonoBehaviour
{
	[SerializeField] private float _maxHP;

	public event Action<UnitController> DeathEvent;

	public bool IsDead => _currentHP <= 0;

	[SerializeField] private float _currentHP;

    public float CurrentHP => _currentHP;

	private void OnEnable()
	{
		_currentHP = _maxHP;
	}

	public void InstantDamage(float damageValue)
	{
		SetHealth(_currentHP - damageValue);
	}

	private void SetHealth(float healthValue)
	{
		_currentHP = healthValue;

		if (healthValue <= 0)
			DeathEvent?.Invoke(this);
	}

	public void Revive() => SetHealth(_maxHP);
}
