using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode
{
    public Vector2 Center;
    public bool Walkable { get; set; }
    public float DamageValue { get; set; }
    public bool Damaging => DamageValue > 0;

    // Длина пути от старта (G).
    public int PathLengthFromStart { get; set; }
    // Точка, из которой пришли в эту точку.
    public AStarNode CameFrom { get; set; }
    // Примерное расстояние до цели (H).
    public int HeuristicEstimatePathLength { get; set; }
    // Ожидаемое полное расстояние до цели (F).
    public int EstimateFullPathLength
    {
        get
        {
            return this.PathLengthFromStart + this.HeuristicEstimatePathLength;
        }
    }

    public AStarNode() { }
    public AStarNode(Vector2 center, bool walkable, float damageValue)
    {
        DamageValue = damageValue;
        Center = center;
        Walkable = walkable;
    }
}
