using Assets.Scripts.Base.Pool;
using UnityEngine;

namespace Pathfinding.Drawing
{
	public class IndicatorPoint : MonoBehaviour, IPoolable
	{
		public string PrefabId { get; set; }

		public void OnPop()
		{
			//throw new System.NotImplementedException();
		}

		public void OnPush()
		{
			//throw new System.NotImplementedException();
		}

		public void SaveDefaultValues()
		{
			//throw new System.NotImplementedException();
		}

		public void SetDefaultValues()
		{
			//throw new System.NotImplementedException();
		}
	}
}
