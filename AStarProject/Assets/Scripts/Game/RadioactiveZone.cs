using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioactiveZone : MonoBehaviour, IDamager
{
	[SerializeField] private float _damage;
    public float Damage { get => _damage; }
}
