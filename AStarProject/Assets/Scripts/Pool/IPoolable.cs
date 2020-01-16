using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Base.Pool
{
	public interface IPoolable
	{
		string PrefabId { get; set; }
		void SaveDefaultValues();
		void SetDefaultValues();
		void OnPop();
		void OnPush();
	}

}
