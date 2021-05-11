using Game.Units.Controllers;
using Pathfinding.Interfaces;
using UnityEngine;

public class RadioactiveZone : MonoBehaviour, IAStarWeightNode
{
	[SerializeField] private float _damage;
    public float Weight { get => _damage; }
    
	private void OnTriggerEnter2D(Collider2D collision)
	{
		var unit = collision.GetComponentInParent<UnitController>();
		if (unit == null)
			return;

		unit.InstantDamage(Weight);
	}
}
