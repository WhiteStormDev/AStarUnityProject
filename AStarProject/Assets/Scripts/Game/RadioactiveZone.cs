using System.Collections;
using System.Collections.Generic;
using Pathfinding.Modifiers;
using UnityEngine;

public class RadioactiveZone : MonoBehaviour, IAStarDamager
{
	[SerializeField] private float _damage;
    public float Damage { get => _damage; }


	private void OnTriggerEnter2D(Collider2D collision)
	{
		var unit = collision.GetComponentInParent<UnitController>();
		if (unit == null)
			return;

		unit.InstantDamage(Damage);
	}
}
