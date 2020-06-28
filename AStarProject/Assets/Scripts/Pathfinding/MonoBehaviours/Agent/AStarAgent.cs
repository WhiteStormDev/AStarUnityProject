using System.Collections.Generic;
using DG.Tweening;
using Game.Units.Controllers;
using Pathfinding.Base;
using UnityEditor;
using UnityEngine;

namespace Pathfinding.MonoBehaviours.Agent
{
	public class AStarAgent : MonoBehaviour
	{
		public bool IsActive = true;
		[Space (10f)]
		public Transform DestinationTarget;
		public float PathUpdateTimer = 1f;

		[Header("Movement")]
		public int MinimumDistance = -1;
		public bool CanMove;
		public float OneNodeMoveDuration;
		public float NodeStopTimer = 0.2f;
		private float _currentNodeStopTimer;
		private float _currentPathUpdateTimer;

		[Header("Unit")]
		[SerializeField] private UnitController _unit;

		private List<AStarNode> _path;
		private int _currentNodeIndex;

		public float CurrentHP => _unit.CurrentHP;
		
		private void Update()
		{
			if (!IsActive)
				return;

			if (DestinationTarget != null)
			{
				if (_currentPathUpdateTimer > 0)
				{
					_currentPathUpdateTimer -= Time.deltaTime;
				}
				else
				{
					SetPath(DestinationTarget.position);
					_currentPathUpdateTimer = PathUpdateTimer;
				}
			}

			if (_path == null || !CanMove)
				return;

			if (_currentNodeStopTimer > 0)
			{
				_currentNodeStopTimer -= Time.deltaTime;
				return;
			}
			if (_currentNodeIndex >= _path.Count)
			{
				_path = null;
				_currentNodeIndex = 0;
				return;
			}
			_currentNodeIndex++;
			if (_currentNodeIndex >= _path.Count)
				return;
			MoveToNode(_path[_currentNodeIndex]);
		}
		
		private void MoveToNode(AStarNode node)
		{
			if (node == null)
				return;
			Debug.Log("MOVE: " + node.Center);
			transform.DOMove(node.Center, OneNodeMoveDuration);
			_currentNodeStopTimer = NodeStopTimer;
		}
		
		/// <summary>
		/// return calculated path
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		public List<AStarNode> SetPath(Vector2 position)
		{
			var path = AStarPathfinding.Instance.GetMinimumPath(transform.position, position, this);
			_currentNodeIndex = 0;

			if (path != null)
				_path = path;
			else
				Debug.LogWarning("NULL path");

			return path;
		}

#if UNITY_EDITOR
		private void DrawPath()
		{
			if (_path == null)
				return;
			Gizmos.color = Color.yellow;
			for (int i = 0; i < _path.Count - 1; i++)
			{
				Gizmos.DrawLine(_path[i].Center, _path[i + 1].Center);
			}
		}

		private void DrawPathBezier()
		{
			if (_path == null)
				return;
			Gizmos.color = Color.cyan;
			for (int i = 0; i < _path.Count - 3; i += 3)
			{
				Handles.DrawBezier(_path[i].Center, _path[i + 3].Center, _path[i+1].Center, _path[i+2].Center, Color.cyan, Texture2D.whiteTexture, 1);
			}
	    
		}

		private void OnDrawGizmos()
		{
			DrawPath();
			DrawPathBezier();
		}
#endif
	}
}
