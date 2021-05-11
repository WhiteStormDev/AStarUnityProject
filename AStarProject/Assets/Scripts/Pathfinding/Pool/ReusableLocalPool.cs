using System.Collections.Generic;
using Base;
using DG.Tweening;
using UnityEngine;

namespace Pool
{
	public class ReusableLocalPool : MonoSingleton<ReusableLocalPool>
	{
		private readonly Dictionary<string, GameObject> ResourceCache = new Dictionary<string, GameObject>();
		private readonly Dictionary<string, Stack<GameObject>> DisabledSceneObjectsCache = new Dictionary<string, Stack<GameObject>>();

		private static int OneTypeGameObjectsMaxCount = 128;
		/// <summary>Returns an inactive instance of a networked GameObject, to be used by PUN.</summary>
		/// <param name="prefabId">String identifier for the networked object.</param>
		/// <param name="position">Location of the new object.</param>
		/// <param name="rotation">Rotation of the new object.</param>
		/// <returns></returns>
		public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
		{
			GameObject instance = null;

			if (!DisabledSceneObjectsCache.TryGetValue(prefabId, out Stack<GameObject> cachedGameObjectStack))
			{
				instance = CreateObject(prefabId, position, rotation);
			}
			else
			{
				if (cachedGameObjectStack.Count > 0 && DisabledSceneObjectsCache[prefabId].Peek() != null)
				{
					instance = DisabledSceneObjectsCache[prefabId].Pop();
					instance.transform.position = position;
					instance.transform.rotation = rotation;
					var poolObjects = instance.GetComponents<IPoolable>();
					for (int i = 0; i < poolObjects.Length; i++)
					{
						poolObjects[i].SetDefaultValues();
					}
				}
				else
				{
					instance = CreateObject(prefabId, position, rotation);
				}
			}

			instance.SetActive(true);
			return instance;
		}

		public GameObject Instantiate(string prefabId, Transform parent)
		{
			GameObject instance = null;

			if (!DisabledSceneObjectsCache.TryGetValue(prefabId, out Stack<GameObject> cachedGameObjectStack))
			{
				instance = CreateObject(prefabId, parent);
			}
			else
			{
				if (cachedGameObjectStack.Count > 0 && DisabledSceneObjectsCache[prefabId].Peek() != null)
				{
					instance = DisabledSceneObjectsCache[prefabId].Pop();
					//instance.transform.position = position;
					//instance.transform.rotation = rotation;
					var poolObjects = instance.GetComponents<IPoolable>();
					for (int i = 0; i < poolObjects.Length; i++)
					{
						poolObjects[i].SetDefaultValues();
					}
				}
				else
				{
					instance = CreateObject(prefabId, parent);
				}
			}

			instance.SetActive(true);
			return instance;
		}

		private GameObject CreateObject(string prefabId, Transform parent)
		{
			GameObject res = null;
			bool cachedResource = this.ResourceCache.TryGetValue(prefabId, out res);
			if (!cachedResource)
			{
				res = (GameObject)Resources.Load(prefabId, typeof(GameObject));
				if (res == null)
				{
					Debug.LogError("ReusableLocalPool failed to load \"" + prefabId + "\" . Make sure it's in a \"Resources\" folder.");
				}
				else
				{
					this.ResourceCache.Add(prefabId, res);
				}
			}

			bool wasActive = res.activeSelf;
			if (wasActive) res.SetActive(false);

			var instance = GameObject.Instantiate(res, parent) as GameObject;
			var poolObjects = instance.GetComponents<IPoolable>();
			for (int i = 0; i < poolObjects.Length; i++)
			{
				poolObjects[i].PrefabId = prefabId;
				poolObjects[i].SaveDefaultValues();
			}

			if (wasActive) res.SetActive(true);
			return instance;
		}

		private GameObject CreateObject(string prefabId, Vector3 position, Quaternion rotation)
		{
			GameObject res = null;
			bool cachedResource = this.ResourceCache.TryGetValue(prefabId, out res);
			if (!cachedResource)
			{
				res = (GameObject)Resources.Load(prefabId, typeof(GameObject));
				if (res == null)
				{
					Debug.LogError("ReusableLocalPool failed to load \"" + prefabId + "\" . Make sure it's in a \"Resources\" folder.");
				}
				else
				{
					this.ResourceCache.Add(prefabId, res);
				}
			}

			bool wasActive = res.activeSelf;
			if (wasActive) res.SetActive(false);

			var instance = GameObject.Instantiate(res, position, rotation) as GameObject;
			var poolObjects = instance.GetComponents<IPoolable>();
			for (int i = 0; i < poolObjects.Length; i++)
			{
				poolObjects[i].PrefabId = prefabId;
				poolObjects[i].SaveDefaultValues();
			}

			if (wasActive) res.SetActive(true);
			return instance;
		}

		public void Destroy(GameObject go, float delay)
		{
			var seq = DOTween.Sequence()
				.AppendCallback(() => Destroy(go))
				.SetDelay(delay);
			seq.Play();
		}

		public void Destroy(GameObject gameObject)
		{
			if (gameObject == null) return;
			var poolable = gameObject.GetComponent<IPoolable>();
			if (poolable == null || poolable.PrefabId == null)
			{
				GameObject.Destroy(gameObject);
				return;
			}
			string path = poolable.PrefabId;

			//Debug.Log("Asset Path PrefabId For Destroy (disable): " + path);

			if (!this.DisabledSceneObjectsCache.ContainsKey(path))
			{
				DisabledSceneObjectsCache[path] = new Stack<GameObject>();
			}


			if (DisabledSceneObjectsCache[path].Count < OneTypeGameObjectsMaxCount)
			{
				gameObject.SetActive(false);
				DisabledSceneObjectsCache[path].Push(gameObject);
			}
			else
			{
				GameObject.Destroy(gameObject);
				Debug.LogWarning("Max pool object count of type: " + path);
			}
		}
	}
}

