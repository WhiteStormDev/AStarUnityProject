using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class AStarNode
{
	public Point GridPosition;
    public Vector2 Center;
    public bool Walkable { get; set; }
    public float DamageValue { get; set; }
    public bool Damaging => DamageValue > 0;

    public AStarNode() { }
    public AStarNode(Vector2 center, bool walkable, float damageValue)
    {
        DamageValue = damageValue;
        Center = center;
        Walkable = walkable;
    }
}
