using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Base.Pool;
using UnityEngine;

namespace Pathfinding
{
	public enum AnalyseMode
	{
		DamageRatioValidating,
		HeuristicDistanceValidating
	}

	public class AStarAnalyser : MonoBehaviour
	{
		public KeyCode AnalyseInputKeyCode;
		[Header("Main")]
		public AStarAgent Agent;
		public Transform Destination;

		public AnalyseMode AnalyseMode;

		[Header ("DamageRatio Validating")]
		public float DamageInfluenceRatioStep = 0.01f;
		public float DamageInfluenceRatioMaxValue = 0.5f;

		[Header ("HeuristicDistance Validating")]
		public int HeuristicDistanceStep = 1;
		public int HeuristicDistanceMaxValue = 5;

		[Header("Draw Settigs")]
		public GameObject DeathPathPrefab;
		public Transform Zero;
		public LineRenderer MainLine;
		public LineRenderer XLine;
		public LineRenderer YLine;
		public LineRenderer ZLine;
		[Space (10)]
		public float XSize;
		public float YSize;
		public float ZSize;
		[Space (10f)]
		public Color XColor;
		public Color YColor;
		public Color ZColor;
		[Space(10f)]
		public Color LineColor;

		private float _damageStep = 0f;
		private int _heuristicStep = 0;

		private List<PathResult> _pathResults;

		private class PathResult
		{
			public float DamageSum;
			public int PathLength;
			public int CheckedNodesCount;
			public List<AStarPathNode> Path;
		}

		private List<GameObject> _instantiatedDrawers = new List<GameObject>();

		private void Update()
		{
			if (Input.GetKeyDown(AnalyseInputKeyCode))
			{
				Analyse();
			}

			DrawResult(AnalyseMode);
		}

		private void Analyse(AnalyseMode? analyseMode = null)
		{
        

			var mode = analyseMode != null ? (AnalyseMode)analyseMode : AnalyseMode;
			_pathResults = new List<PathResult>();
			_damageStep = 0;
			_heuristicStep = 0;
			switch (mode)
			{
				case AnalyseMode.DamageRatioValidating:
					while (_damageStep < DamageInfluenceRatioMaxValue)
					{
						AStarPathfinding.Instance.DamageInfluenceRatio = _damageStep;
						ValidatePath();
						_damageStep += DamageInfluenceRatioStep;
					}
					break;
				case AnalyseMode.HeuristicDistanceValidating:
					while (_heuristicStep < HeuristicDistanceMaxValue)
					{
						AStarPathfinding.Instance.DamageInfluenceRatio = _damageStep;
						ValidatePath();
						_heuristicStep += HeuristicDistanceStep;
					}
					break;
			}
			DrawResult(mode);
		}

		private void ValidatePath()
		{
			if (Destination == null || Agent == null)
				return;

			Agent.SetPath(Destination.position);
			var path = AStarPathfinding.Instance.LastPath;
			if (path != null)
			{
				_pathResults.Add(new PathResult
				{
					DamageSum = path[path.Count - 1].DamageValueFromStart,
					PathLength = path.Count,
					CheckedNodesCount = AStarPathfinding.Instance.ClosetSetCount + AStarPathfinding.Instance.OpenSetCount,
					Path = path
				});
			}
		}

		private void DrawResult(AnalyseMode mode)
		{
			if (_pathResults == null)
				return;
			_instantiatedDrawers.ForEach(d => ReusableLocalPool.Instance.Destroy(d));
			_instantiatedDrawers.Clear();
			//TODO сделать единичный отрезок относительно дельты минимального и максимального дамага за пути и тд (под каждую ось свое)
			//И потом когда расчитываем позицию для отрисовки линии соответвенно относительно конкретных результатов
			var minDamage = _pathResults.Find(p => _pathResults.TrueForAll(pp => p.DamageSum <= pp.DamageSum));
			var maxDamage = _pathResults.Find(p => _pathResults.TrueForAll(pp => p.DamageSum >= pp.DamageSum));

			var minLength = _pathResults.Find(p => _pathResults.TrueForAll(pp => p.PathLength <= pp.PathLength));
			var maxLength = _pathResults.Find(p => _pathResults.TrueForAll(pp => p.PathLength >= pp.PathLength));
			var damageDelta = maxDamage.DamageSum - minDamage.DamageSum;
			var pathLengthDelta = Mathf.Round(maxLength.PathLength - minLength.PathLength);

			float xStep = (XSize - XSize * 0.1f) / _pathResults.Count; //ратио
			float yStep = damageDelta == 0 ? 0 : (YSize - YSize * 0.1f) / damageDelta;
			float zStep = pathLengthDelta == 0 ? 0 : (ZSize - ZSize * 0.1f) / pathLengthDelta;
			//switch (mode)
			//{
			//    case AnalyseMode.DamageRatioValidating:
			//        xStep = (XSize - XSize * 0.1f) / _pathResults.Count;
			//        break;
			//    case AnalyseMode.HeuristicDistanceValidating:
			//        break;
			//}
			//XLine.positionCount = 2;
			//XLine.SetPositions(new Vector3[] { Zero.position, Zero.position + Vector3.right * XSize });
			//XLine.startColor = XColor;
			//XLine.endColor = XColor;
			//XLine.material = new Material(Shader.Find("Sprites/Default"));
			//XLine.widthMultiplier = 0.2f;

			//YLine.positionCount = 2;
			//YLine.SetPositions(new Vector3[] { Zero.position, Zero.position + Vector3.up * YSize });
			//YLine.startColor = YColor;
			//YLine.endColor = YColor;
			//YLine.material = new Material(Shader.Find("Sprites/Default"));
			//YLine.widthMultiplier = 0.2f;

			//ZLine.positionCount = 2;
			//ZLine.SetPositions(new Vector3[] { Zero.position, Zero.position + Vector3.forward * ZSize });
			//ZLine.startColor = ZColor;
			//ZLine.endColor = ZColor;
			//ZLine.material = new Material(Shader.Find("Sprites/Default"));
			//ZLine.widthMultiplier = 0.2f;

			Debug.DrawLine(Zero.position, Zero.position + Vector3.right * XSize, XColor);
			Debug.DrawLine(Zero.position, Zero.position + Vector3.forward * YSize, YColor);
			Debug.DrawLine(Zero.position, Zero.position + Vector3.up * ZSize, ZColor);

			Vector3[] linePoints = new Vector3[_pathResults.Count];
			for (int i = 0; i < _pathResults.Count; i++)
			{
				var r = _pathResults[i];
				linePoints[i] = Zero.position + new Vector3(xStep * i, yStep * (r.DamageSum - minDamage.DamageSum), zStep * (r.PathLength - minLength.PathLength));
				if (r.Path.Find(n => n.DamageValueFromStart >= Agent.CurrentHP) != null)
				{
					_instantiatedDrawers.Add(ReusableLocalPool.Instance.Instantiate(Path.Combine("Prefabs", DeathPathPrefab.name), linePoints[i], Quaternion.identity));
				}
			}

			//MainLine.positionCount = linePoints.Length;
			//MainLine.SetPositions(linePoints);
			//MainLine.startColor = LineColor;
			//MainLine.endColor = LineColor;
			//MainLine.material = new Material(Shader.Find("Sprites/Default"));
			//MainLine.widthMultiplier = 0.2f;
			for (int i = 0; i < linePoints.Length - 1; i++)
			{
				Debug.DrawLine(linePoints[i], linePoints[i + 1], LineColor);
			}
		}
	}
}